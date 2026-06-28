using System;
using System.Linq;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public class PdhpdlOrderExecutor
{
    private const string LabelPrefix = "PDHPDL_V1";
    private const string Tp1Comment = "TP1";
    private const string RunnerComment = "RUNNER";

    private readonly Robot _robot;
    private readonly Symbol _symbol;
    private readonly string _symbolName;

    private readonly double _riskPct;
    private readonly int _stopOffsetTicks;
    private readonly double _tp1R;
    private readonly double _tp2R;

    private readonly string _timeFrame;
    private readonly PdhpdlTradeCsvLogger _csvLogger;

    public PdhpdlOrderExecutor(
        Robot robot,
        Symbol symbol,
        string symbolName,
        string timeFrame,
        double riskPct,
        int stopOffsetTicks,
        double tp1R,
        double tp2R,
        PdhpdlTradeCsvLogger csvLogger
    )
    {
        _robot = robot;
        _symbol = symbol;
        _symbolName = symbolName;
        _timeFrame = timeFrame;

        _riskPct = riskPct;
        _stopOffsetTicks = stopOffsetTicks;
        _tp1R = tp1R;
        _tp2R = tp2R;
        _csvLogger = csvLogger;
    }

    public void ExecuteIfSignal(PdhpdlSignal signal)
    {
        if (signal == null || !signal.HasData)
            return;

        if (!signal.IsLongSignal && !signal.IsShortSignal)
            return;

        if (HasOpenStrategyPosition())
        {
            _robot.Print("*****Order skipped | Existing PDHPDL position found.");
            return;
        }

        PdhpdlOrderPlan plan = CreatePlan(signal);

        if (!plan.IsValid)
        {
            _robot.Print("*****Order rejected | Reason: {0}", plan.RejectReason);
            return;
        }

        ExecutePlan(plan);
    }

    private bool HasOpenStrategyPosition()
    {
        return _robot.Positions.Any(position =>
            position.SymbolName == _symbolName &&
            position.Label != null &&
            position.Label.StartsWith(LabelPrefix)
        );
    }

    private PdhpdlOrderPlan CreatePlan(PdhpdlSignal signal)
    {
        PdhpdlOrderPlan plan = new PdhpdlOrderPlan();

        TradeType tradeType = signal.IsLongSignal
            ? TradeType.Buy
            : TradeType.Sell;

        double entry = signal.Close;
        double stopOffset = _symbol.TickSize * _stopOffsetTicks;

        double stop;
        double riskPrice;
        double tp1;
        double tp2;

        if (tradeType == TradeType.Buy)
        {
            stop = signal.Low - stopOffset;
            riskPrice = entry - stop;
            tp1 = entry + _tp1R * riskPrice;
            tp2 = entry + _tp2R * riskPrice;
        }
        else
        {
            stop = signal.High + stopOffset;
            riskPrice = stop - entry;
            tp1 = entry - _tp1R * riskPrice;
            tp2 = entry - _tp2R * riskPrice;
        }

        if (riskPrice <= 0.0)
        {
            plan.RejectReason = "Risk price is not positive.";
            return plan;
        }

        double stopLossPips = riskPrice / _symbol.PipSize;
        double tp1Pips = Math.Abs(tp1 - entry) / _symbol.PipSize;
        double tp2Pips = Math.Abs(tp2 - entry) / _symbol.PipSize;

        double riskMoney = RiskUtil.CalcRiskMoney(_robot.Account.Equity, _riskPct);

        double totalVolumeInUnits = _symbol.VolumeForProportionalRisk(
            ProportionalAmountType.Equity,
            _riskPct,
            stopLossPips,
            RoundingMode.Down
        );

        totalVolumeInUnits = _symbol.NormalizeVolumeInUnits(
            totalVolumeInUnits,
            RoundingMode.Down
        );

        if (totalVolumeInUnits < _symbol.VolumeInUnitsMin)
        {
            plan.RejectReason =
                $"Calculated volume is too small. TotalVolume={totalVolumeInUnits}, Min={_symbol.VolumeInUnitsMin}";
            return plan;
        }

        if (totalVolumeInUnits > _symbol.VolumeInUnitsMax)
        {
            plan.RejectReason =
                $"Calculated volume is above broker maximum. TotalVolume={totalVolumeInUnits}, Max={_symbol.VolumeInUnitsMax}";
            return plan;
        }

        double volumePerLegInUnits = _symbol.NormalizeVolumeInUnits(
            totalVolumeInUnits / 2.0,
            RoundingMode.Down
        );

        if (volumePerLegInUnits < _symbol.VolumeInUnitsMin)
        {
            plan.RejectReason =
                $"Volume per leg is too small. VolumePerLeg={volumePerLegInUnits}, Min={_symbol.VolumeInUnitsMin}";
            return plan;
        }

        double executableTotalVolumeInUnits = volumePerLegInUnits * 2.0;
        double estimatedRiskMoney = _symbol.AmountRisked(
            executableTotalVolumeInUnits,
            stopLossPips
        );

        string side = tradeType == TradeType.Buy ? "L" : "S";

        plan.IsValid = true;
        plan.TradeType = tradeType;
        plan.EntryPrice = entry;
        plan.StopPrice = stop;
        plan.Tp1Price = tp1;
        plan.Tp2Price = tp2;
        plan.RiskPrice = riskPrice;
        plan.StopLossPips = stopLossPips;
        plan.Tp1Pips = tp1Pips;
        plan.Tp2Pips = tp2Pips;
        plan.TotalLots = executableTotalVolumeInUnits / _symbol.LotSize;
        plan.TotalVolumeInUnits = executableTotalVolumeInUnits;
        plan.VolumePerLegInUnits = volumePerLegInUnits;
        plan.RiskMoney = riskMoney;
        plan.EstimatedRiskMoney = estimatedRiskMoney;
        plan.Tp1Label = $"{LabelPrefix}_{side}_TP1";
        plan.RunnerLabel = $"{LabelPrefix}_{side}_RUNNER";

        return plan;
    }

    private void ExecutePlan(PdhpdlOrderPlan plan)
    {
        _robot.Print(
            "*****Order plan | Side: {0}, Entry: {1}, Stop: {2}, TP1: {3}, TP2: {4}, RiskPrice: {5}, StopLossPips: {6}, RiskMoney: {7}, EstimatedRiskMoney: {8}, Lots: {9}, TotalVolumeUnits: {10}, VolumePerLegUnits: {11}",
            plan.TradeType,
            plan.EntryPrice,
            plan.StopPrice,
            plan.Tp1Price,
            plan.Tp2Price,
            plan.RiskPrice,
            plan.StopLossPips,
            plan.RiskMoney,
            plan.EstimatedRiskMoney,
            plan.TotalLots,
            plan.TotalVolumeInUnits,
            plan.VolumePerLegInUnits
        );

        TradeResult tp1Result = _robot.ExecuteMarketOrder(
            plan.TradeType,
            _symbolName,
            plan.VolumePerLegInUnits,
            plan.Tp1Label,
            plan.StopLossPips,
            plan.Tp1Pips,
            Tp1Comment
        );

        if (!tp1Result.IsSuccessful)
        {
            _robot.Print("*****TP1 order failed | Error: {0}", tp1Result.Error);
            return;
        }

        TradeResult runnerResult = _robot.ExecuteMarketOrder(
            plan.TradeType,
            _symbolName,
            plan.VolumePerLegInUnits,
            plan.RunnerLabel,
            plan.StopLossPips,
            plan.Tp2Pips,
            RunnerComment
        );

        if (!runnerResult.IsSuccessful)
        {
            _robot.Print("*****Runner order failed | Error: {0}", runnerResult.Error);

            if (tp1Result.Position != null)
                _robot.ClosePosition(tp1Result.Position);

            return;
        }

        _robot.Print(
            "*****Orders opened | TP1: {0}, Runner: {1}",
            plan.Tp1Label,
            plan.RunnerLabel
        );
        WriteCsvRecord(plan);
    }

    private void WriteCsvRecord(PdhpdlOrderPlan plan)
    {
        try
        {
            string side = plan.TradeType == TradeType.Buy ? "B" : "S";
            string keyLevel = plan.TradeType == TradeType.Buy ? "PDL" : "PDH";

            PdhpdlTradeCsvRecord record = new PdhpdlTradeCsvRecord
            {
                Side = side,
                KeyLevel = keyLevel,
                Signal = "false-breakout",
                CloseEntryResult = "",
                Pullback25Result = "",
                Pullback382Result = "",
                Pullback50Result = "",
                Comment = "",
                Symbol = _symbolName,
                TimeFrame = _timeFrame,
                EntryTime = _robot.Server.Time,
                EntryPrice = plan.EntryPrice,
                StopPrice = plan.StopPrice,
                Tp1Price = plan.Tp1Price,
                Tp2Price = plan.Tp2Price,
                RiskPrice = plan.RiskPrice,
                VolumeInUnits = plan.TotalVolumeInUnits
            };

            _csvLogger.Append(record);

            _robot.Print("*****CSV trade record added. Path: {0}", _csvLogger.FilePath);
        }
        catch (Exception ex)
        {
            _robot.Print("*****CSV write failed | {0}", ex.Message);
        }
    }
}

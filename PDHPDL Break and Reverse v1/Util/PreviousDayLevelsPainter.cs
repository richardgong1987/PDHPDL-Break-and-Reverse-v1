using System;
using cAlgo.API;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

public class PreviousDayLevelsPainter
{
    private const string PdhLineName = "PDH_LINE";
    private const string PdlLineName = "PDL_LINE";

    private readonly Chart _chart;
    private readonly Bars _dailyBar;
    private DateTime _lastPreviousDailyOpenTime = DateTime.MinValue;

    public PreviousDayLevelsPainter(Chart chart, MarketData marketData, string symbolName)
    {
        _chart = chart;
        _dailyBar = marketData.GetBars(TimeFrame.Daily, symbolName);
    }

    public void Draw()
    {
        if (_dailyBar.Count < 2)
            return;

        int previousDailyIndex = _dailyBar.Count - 2;

        DateTime previousDailyOpenTime = _dailyBar.OpenTimes[previousDailyIndex];

        if (previousDailyOpenTime == _lastPreviousDailyOpenTime)
            return;

        _lastPreviousDailyOpenTime = previousDailyOpenTime;

        double pdh = _dailyBar.HighPrices[previousDailyIndex];
        double pdl = _dailyBar.LowPrices[previousDailyIndex];

        _chart.RemoveObject(PdhLineName);
        _chart.RemoveObject(PdlLineName);
        _chart.DrawHorizontalLine(PdhLineName, pdh, Color.Red, 5, LineStyle.Solid);
        _chart.DrawHorizontalLine(PdlLineName, pdl, Color.Blue, 5, LineStyle.Solid);
    }
}
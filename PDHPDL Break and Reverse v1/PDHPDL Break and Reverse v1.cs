using System;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

[Robot(AccessRights = AccessRights.None, AddIndicators = true)]
public class PDHPDLBreakandReversev1 : Robot
{
    [Parameter("Line Thickness", DefaultValue = 3)]
    public int LineThickness { get; set; }

    [Parameter("Show Debug Logs", DefaultValue = false)]
    public bool ShowDebugLogs { get; set; }

    private PdhpdlLines _pdhpdlLines;
    private Bars _dailyBars;

    protected override void OnStart()
    {
        int daysToDraw = PdhpdlUtils.GetDaysToDraw(Bars);
        _pdhpdlLines = new PdhpdlLines(
            Chart,
            MarketData,
            SymbolName,
            daysToDraw,
            LineThickness
        );
        _dailyBars = MarketData.GetBars(TimeFrame.Daily, SymbolName);
        _pdhpdlLines.Draw();

        Print("PDH/PDL step painter started. DaysToDraw: {0}", daysToDraw);
    }

    protected override void OnBar()
    {
        _pdhpdlLines.Draw();
        DetectFalseBreakoutOnCloseBar();
    }

    protected override void OnTick()
    {
    }

    protected override void OnStop()
    {
    }

    private void DetectFalseBreakoutOnCloseBar()
    {
        PdhpdlSignal signal = PdhpdlUtils.DetectFalseBreakoutOnClosedBar(
            Bars,
            _dailyBars
        );

        if (!signal.HasData)
            return;

        if (ShowDebugLogs)
        {
            Print(
                "*****Bar closed | Time: {0}, High: {1}, Low: {2}, Close: {3}, PDH: {4}, PDL: {5}",
                signal.BarTime,
                signal.High,
                signal.Low,
                signal.Close,
                signal.Pdh,
                signal.Pdl
            );
        }

        if (signal.IsLongSignal)
        {
            Print(
                "*****LONG trigger | Time: {0}, Low: {1}, Close: {2}, PDL: {3}",
                signal.BarTime,
                signal.Low,
                signal.Close,
                signal.Pdl
            );
        }

        if (signal.IsShortSignal)
        {
            Print(
                "*****SHORT trigger | Time: {0}, High: {1}, Close: {2}, PDH: {3}",
                signal.BarTime,
                signal.High,
                signal.Close,
                signal.Pdh
            );
        }
    }
}
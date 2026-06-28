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
        PdhpdlUtils.DetectFalseBreakoutOnClosedBar(Bars, _dailyBars, ShowDebugLogs);
    }
}
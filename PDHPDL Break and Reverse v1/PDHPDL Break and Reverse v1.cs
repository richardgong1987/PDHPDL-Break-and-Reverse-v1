using System;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots;

[Robot(AccessRights = AccessRights.None, AddIndicators = true)]
public class PDHPDLBreakandReversev1 : Robot
{
    [Parameter(DefaultValue = "Hello world!")]
    public string Message { get; set; }

    private PreviousDayLevelsPainter _previousDayLevelsPainter;

    protected override void OnStart()
    {
        _previousDayLevelsPainter = new PreviousDayLevelsPainter(Chart, MarketData, SymbolName);
        _previousDayLevelsPainter.Draw();
        Print("PDH/PDL painter started.");
    }

    protected override void OnBar()
    {
        _previousDayLevelsPainter.Draw();
    }

    protected override void OnTick()
    {
        // Handle price updates here
    }

    protected override void OnStop()
    {
        // Handle cBot stop here
    }
}
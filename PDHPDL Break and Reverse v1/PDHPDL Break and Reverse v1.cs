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

    [Parameter("Days To Draw", DefaultValue = 10)]
    public int DaysToDraw { get; set; }

    [Parameter("Line Thickness", DefaultValue = 3)]
    public int LineThickness { get; set; }

    private PreviousDayLevelsPainter _previousDayLevelsPainter;

    protected override void OnStart()
    {
        _previousDayLevelsPainter = new PreviousDayLevelsPainter(
            Chart,
            MarketData,
            SymbolName,
            DaysToDraw,
            LineThickness
        );

        _previousDayLevelsPainter.Draw();

        Print("PDH/PDL step painter started.");
    }

    protected override void OnBar()
    {
        _previousDayLevelsPainter.Draw();
    }

    protected override void OnTick()
    {
    }

    protected override void OnStop()
    {
    }
}
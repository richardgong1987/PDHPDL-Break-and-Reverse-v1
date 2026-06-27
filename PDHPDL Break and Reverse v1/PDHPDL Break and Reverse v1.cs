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

    private PdhpdlLines _pdhpdlLines;

    protected override void OnStart()
    {
        int daysToDraw = PdhpdlLineUtils.GetDaysToDraw(Bars);
        _pdhpdlLines = new PdhpdlLines(
            Chart,
            MarketData,
            SymbolName,
            daysToDraw,
            LineThickness
        );

        _pdhpdlLines.Draw();

        Print("PDH/PDL step painter started. DaysToDraw: {0}", daysToDraw);
    }

    protected override void OnBar()
    {
        _pdhpdlLines.Draw();
    }

    protected override void OnTick()
    {
    }

    protected override void OnStop()
    {
    }
}
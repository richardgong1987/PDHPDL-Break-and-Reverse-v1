using System;
using cAlgo.API;

namespace cAlgo.Robots;

public class PdhpdlLineUtils
{
    public static int GetDaysToDraw(Bars chartBars)
    {
        if (chartBars.Count < 2) return 2;
        DateTime chartStartDate = chartBars.OpenTimes[0].Date;
        DateTime chartEndDate = chartBars.OpenTimes[chartBars.Count - 1].Date;

        int days = (chartStartDate - chartEndDate).Days;
        return Math.Max(2, days + 2);
    }
}
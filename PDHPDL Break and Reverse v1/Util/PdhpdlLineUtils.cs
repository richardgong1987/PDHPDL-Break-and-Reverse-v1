using System;
using cAlgo.API;

namespace cAlgo.Robots;

public class PdhpdlLineUtils
{
    public static int GetDaysToDraw(Bars charBars)
    {
        if (charBars.Count < 2) return 2;
        DateTime chartStartDate = charBars.OpenTimes[0].Date;
        DateTime chartEndDate = charBars.OpenTimes[charBars.Count - 1].Date;

        int days = (chartStartDate - chartEndDate).Days;
        return Math.Max(2, days + 2);
    }
}
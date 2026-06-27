using System;

namespace cAlgo.Robots
{
    /// <summary>
    /// Pure position-sizing rules: given account risk tolerance and a stop distance,
    /// decide how much volume to trade. Independent of the cAlgo framework so it can be
    /// unit-tested without a running cBot or market connection.
    /// </summary>
    public static class RiskUtil
    {
        /// <summary>
        /// Rounds <paramref name="value"/> down to the nearest multiple of <paramref name="step"/>.
        /// Used to snap a raw volume onto the broker's permitted volume increments.
        /// </summary>
        public static double FloorToStep(double value, double step)
        {
            if (step <= 0.0)
                return 0.0;
            return Math.Floor(value / step) * step;
        }

        public static double Clamp(double value, double minValue, double maxValue)
        {
            if (value < minValue)
                return minValue;
            if (value > maxValue)
                return maxValue;
            return value;
        }

        /// <summary>
        /// Money the account is willing to lose on a single trade, derived from equity and
        /// a risk percentage (e.g. 1.0 means risk 1% of equity).
        /// </summary>
        public static double CalcRiskMoney(double equity, double riskPct)
        {
            if (equity <= 0.0 || riskPct <= 0.0)
                return 0.0;
            return equity * riskPct / 100.0;
        }

        /// <summary>
        /// Money lost per one lot if price travels from <paramref name="entry"/> to
        /// <paramref name="stop"/>, expressed via the instrument's tick size and tick value.
        /// </summary>
        public static double CalcLossPerLot(double entry, double stop, double tickSize, double tickValue)
        {
            double distance = Math.Abs(entry - stop);
            if (distance <= 0.0 || tickSize <= 0.0 || tickValue <= 0.0)
                return 0.0;
            return distance / tickSize * tickValue;
        }

        /// <summary>
        /// Snaps a raw volume onto the broker's volume step and bounds. Returns 0 when the
        /// stepped volume falls below the minimum tradable size — too small to trade safely.
        /// </summary>
        public static double NormalizeVolume(
            double volume,
            double minVolume,
            double maxVolume,
            double volumeStep)
        {
            if (volume <= 0.0 || volumeStep <= 0.0)
                return 0.0;
            double normalized = FloorToStep(volume, volumeStep);
            if (normalized < minVolume)
                return 0.0;
            return Clamp(normalized, minVolume, maxVolume);
        }

        /// <summary>
        /// Position size such that hitting the stop loses approximately <paramref name="riskPct"/>
        /// of equity, normalized to the broker's tradable volume constraints.
        /// </summary>
        public static double CalcVolumeByRisk(
            double equity,
            double riskPct,
            double entry,
            double stop,
            double tickSize,
            double tickValue,
            double minVolume,
            double maxVolume,
            double volumeStep)
        {
            double riskMoney = CalcRiskMoney(equity, riskPct);
            double lossPerLot = CalcLossPerLot(entry, stop, tickSize, tickValue);
            if (riskMoney <= 0.0 || lossPerLot <= 0.0)
                return 0.0;

            double rawVolume = riskMoney / lossPerLot;
            return NormalizeVolume(rawVolume, minVolume, maxVolume, volumeStep);
        }
    }
}

using cAlgo.Robots;
using Xunit;

namespace RiskUtil.Tests
{
    public class FloorToStepTests
    {
        [Fact]
        public void snaps_value_down_to_nearest_step()
        {
            Assert.Equal(0.23, global::cAlgo.Robots.RiskUtil.FloorToStep(0.237, 0.01), precision: 10);
        }

        [Fact]
        public void returns_exact_value_when_already_on_a_step()
        {
            Assert.Equal(1.0, global::cAlgo.Robots.RiskUtil.FloorToStep(1.0, 0.5), precision: 10);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        public void returns_zero_when_step_is_not_positive(double step)
        {
            Assert.Equal(0.0, global::cAlgo.Robots.RiskUtil.FloorToStep(5.0, step));
        }
    }

    public class ClampTests
    {
        [Fact]
        public void returns_min_when_below_range()
        {
            Assert.Equal(1.0, global::cAlgo.Robots.RiskUtil.Clamp(0.5, 1.0, 10.0));
        }

        [Fact]
        public void returns_max_when_above_range()
        {
            Assert.Equal(10.0, global::cAlgo.Robots.RiskUtil.Clamp(99.0, 1.0, 10.0));
        }

        [Fact]
        public void returns_value_when_inside_range()
        {
            Assert.Equal(5.0, global::cAlgo.Robots.RiskUtil.Clamp(5.0, 1.0, 10.0));
        }
    }

    public class CalcRiskMoneyTests
    {
        [Fact]
        public void risks_the_given_percentage_of_equity()
        {
            Assert.Equal(100.0, global::cAlgo.Robots.RiskUtil.CalcRiskMoney(10000.0, 1.0), precision: 10);
        }

        [Theory]
        [InlineData(0.0, 1.0)]
        [InlineData(-1.0, 1.0)]
        [InlineData(10000.0, 0.0)]
        [InlineData(10000.0, -1.0)]
        public void returns_zero_for_non_positive_inputs(double equity, double riskPct)
        {
            Assert.Equal(0.0, global::cAlgo.Robots.RiskUtil.CalcRiskMoney(equity, riskPct));
        }
    }

    public class CalcLossPerLotTests
    {
        [Fact]
        public void converts_stop_distance_into_money_per_lot()
        {
            // 10 ticks of distance (1.0 / 0.1) at 1.0 per tick = 10.0
            Assert.Equal(10.0, global::cAlgo.Robots.RiskUtil.CalcLossPerLot(100.0, 99.0, 0.1, 1.0), precision: 10);
        }

        [Fact]
        public void distance_is_unsigned_so_stop_above_entry_is_equivalent()
        {
            double down = global::cAlgo.Robots.RiskUtil.CalcLossPerLot(100.0, 99.0, 0.1, 1.0);
            double up = global::cAlgo.Robots.RiskUtil.CalcLossPerLot(99.0, 100.0, 0.1, 1.0);
            Assert.Equal(down, up, precision: 10);
        }

        [Theory]
        [InlineData(100.0, 100.0, 0.1, 1.0)] // zero distance
        [InlineData(100.0, 99.0, 0.0, 1.0)]  // zero tick size
        [InlineData(100.0, 99.0, 0.1, 0.0)]  // zero tick value
        public void returns_zero_for_degenerate_inputs(double entry, double stop, double tickSize, double tickValue)
        {
            Assert.Equal(0.0, global::cAlgo.Robots.RiskUtil.CalcLossPerLot(entry, stop, tickSize, tickValue));
        }
    }

    public class NormalizeVolumeTests
    {
        [Fact]
        public void snaps_volume_onto_the_step()
        {
            Assert.Equal(0.13, global::cAlgo.Robots.RiskUtil.NormalizeVolume(0.137, 0.01, 100.0, 0.01), precision: 10);
        }

        [Fact]
        public void returns_zero_when_stepped_volume_is_below_minimum()
        {
            Assert.Equal(0.0, global::cAlgo.Robots.RiskUtil.NormalizeVolume(0.005, 0.01, 100.0, 0.01));
        }

        [Fact]
        public void clamps_to_maximum_volume()
        {
            Assert.Equal(100.0, global::cAlgo.Robots.RiskUtil.NormalizeVolume(250.0, 0.01, 100.0, 0.01), precision: 10);
        }

        [Theory]
        [InlineData(0.0, 0.01)]
        [InlineData(1.0, 0.0)]
        public void returns_zero_for_non_positive_volume_or_step(double volume, double step)
        {
            Assert.Equal(0.0, global::cAlgo.Robots.RiskUtil.NormalizeVolume(volume, 0.01, 100.0, step));
        }
    }

    public class CalcVolumeByRiskTests
    {
        [Fact]
        public void sizes_position_so_stop_loss_equals_risk_budget()
        {
            // Risk = 1% of 10,000 = 100 money.
            // Loss per lot = (100-99)/0.1 * 1.0 = 10 money.
            // Raw volume = 100 / 10 = 10 lots, on-step and within bounds.
            double volume = global::cAlgo.Robots.RiskUtil.CalcVolumeByRisk(
                equity: 10000.0, riskPct: 1.0,
                entry: 100.0, stop: 99.0,
                tickSize: 0.1, tickValue: 1.0,
                minVolume: 0.01, maxVolume: 100.0, volumeStep: 0.01);

            Assert.Equal(10.0, volume, precision: 10);
        }

        [Fact]
        public void rounds_raw_volume_down_to_broker_step()
        {
            // Raw volume = 100 / (0.3/0.1 * 1.0) = 100/3 = 33.333..., floored to 33.33.
            double volume = global::cAlgo.Robots.RiskUtil.CalcVolumeByRisk(
                equity: 10000.0, riskPct: 1.0,
                entry: 100.0, stop: 99.7,
                tickSize: 0.1, tickValue: 1.0,
                minVolume: 0.01, maxVolume: 100.0, volumeStep: 0.01);

            Assert.Equal(33.33, volume, precision: 10);
        }

        [Fact]
        public void returns_zero_when_risk_budget_is_zero()
        {
            double volume = global::cAlgo.Robots.RiskUtil.CalcVolumeByRisk(
                equity: 10000.0, riskPct: 0.0,
                entry: 100.0, stop: 99.0,
                tickSize: 0.1, tickValue: 1.0,
                minVolume: 0.01, maxVolume: 100.0, volumeStep: 0.01);

            Assert.Equal(0.0, volume);
        }

        [Fact]
        public void returns_zero_when_stop_equals_entry()
        {
            double volume = global::cAlgo.Robots.RiskUtil.CalcVolumeByRisk(
                equity: 10000.0, riskPct: 1.0,
                entry: 100.0, stop: 100.0,
                tickSize: 0.1, tickValue: 1.0,
                minVolume: 0.01, maxVolume: 100.0, volumeStep: 0.01);

            Assert.Equal(0.0, volume);
        }

        [Fact]
        public void returns_zero_when_computed_volume_is_below_minimum()
        {
            // Tiny risk budget yields a sub-minimum lot, which is not tradable.
            double volume = global::cAlgo.Robots.RiskUtil.CalcVolumeByRisk(
                equity: 100.0, riskPct: 0.01,
                entry: 100.0, stop: 99.0,
                tickSize: 0.1, tickValue: 1.0,
                minVolume: 1.0, maxVolume: 100.0, volumeStep: 0.01);

            Assert.Equal(0.0, volume);
        }
    }
}

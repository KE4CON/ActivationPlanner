using ActivationPlanner.Services.Power;

namespace ActivationPlanner.Services.Tests.Power;

public sealed class BatteryEstimatorTests
{
    [Fact]
    public void Estimates_runtime_for_a_100w_ssb_setup()
    {
        var result = BatteryEstimator.Estimate(new PowerBudgetInput
        {
            CapacityAh = 20, UsableFraction = 0.9,
            TxPowerWatts = 100, SupplyVoltage = 12.0, TxEfficiencyFraction = 0.55,
            RxCurrentAmps = 1.0, TxDutyFraction = 0.25,
        });

        // TX current = 100 / (12 * 0.55) = 15.15 A
        Assert.Equal(15.15, result.TxCurrentAmps, 1);
        // Avg = 0.25*15.15 + 0.75*1 = 4.54 A
        Assert.Equal(4.54, result.AverageCurrentAmps, 1);
        // Runtime = (20 * 0.9) / 4.54 = ~3.97 h
        Assert.Equal(3.97, result.RuntimeHours, 1);
    }

    [Fact]
    public void Qrp_runs_far_longer_than_100w()
    {
        var input = new PowerBudgetInput { CapacityAh = 20, TxPowerWatts = 5 };
        var qrp = BatteryEstimator.Estimate(input);
        var full = BatteryEstimator.Estimate(input with { TxPowerWatts = 100 });

        Assert.True(qrp.RuntimeHours > full.RuntimeHours * 3, "QRP should last much longer");
        Assert.True(qrp.TxCurrentAmps < 1.5);
    }

    [Fact]
    public void Duty_fraction_is_clamped_to_0_1()
    {
        var full = BatteryEstimator.Estimate(new PowerBudgetInput
        {
            CapacityAh = 10, TxPowerWatts = 100, TxDutyFraction = 5.0, // absurd -> clamps to 1.0
        });
        // At 100% duty, average current equals TX current.
        Assert.Equal(full.TxCurrentAmps, full.AverageCurrentAmps, 3);
    }

    [Fact]
    public void Degenerate_inputs_return_zero_rather_than_throwing()
    {
        var noVolts = BatteryEstimator.Estimate(new PowerBudgetInput
        {
            CapacityAh = 10, TxPowerWatts = 100, SupplyVoltage = 0,
        });
        Assert.Equal(0, noVolts.TxCurrentAmps);

        var noDraw = BatteryEstimator.Estimate(new PowerBudgetInput
        {
            CapacityAh = 10, TxPowerWatts = 0, RxCurrentAmps = 0, TxDutyFraction = 0,
        });
        Assert.Equal(0, noDraw.RuntimeHours);
    }
}

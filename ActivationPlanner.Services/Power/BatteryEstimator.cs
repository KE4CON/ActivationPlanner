namespace ActivationPlanner.Services.Power;

/// <summary>
/// Inputs for a field power-budget estimate. Defaults suit a typical portable LiFePO4 setup; the UI
/// prefills capacity/power from owned gear where it can, and every value stays operator-editable.
/// </summary>
public sealed record PowerBudgetInput
{
    /// <summary>Battery capacity in amp-hours.</summary>
    public required double CapacityAh { get; init; }

    /// <summary>Usable fraction of capacity (LiFePO4 ~0.9–1.0; lead-acid ~0.5 to protect the battery).</summary>
    public double UsableFraction { get; init; } = 0.9;

    /// <summary>Transmit power output in watts.</summary>
    public required double TxPowerWatts { get; init; }

    /// <summary>Supply voltage (nominal 12 V for most portable LiFePO4 rigs).</summary>
    public double SupplyVoltage { get; init; } = 12.0;

    /// <summary>Transmitter efficiency (DC input → RF out); ~0.5–0.6 is typical for HF finals.</summary>
    public double TxEfficiencyFraction { get; init; } = 0.55;

    /// <summary>Receive/idle current draw in amps.</summary>
    public double RxCurrentAmps { get; init; } = 1.0;

    /// <summary>Fraction of operating time spent transmitting (SSB ragchew ~0.25; contesting higher).</summary>
    public double TxDutyFraction { get; init; } = 0.25;
}

/// <summary>Result of a power-budget estimate.</summary>
public sealed record PowerBudgetResult(double TxCurrentAmps, double AverageCurrentAmps, double RuntimeHours);

/// <summary>
/// Pure field power-budget math: estimate transmit current from power output, blend it with receive
/// current by transmit duty cycle to get average draw, then divide usable capacity by that draw for
/// runtime. Tolerant of degenerate inputs (returns zeros rather than throwing) so it can drive a
/// live-updating UI. No I/O — fully unit-tested.
/// </summary>
public static class BatteryEstimator
{
    public static PowerBudgetResult Estimate(PowerBudgetInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        double duty = Math.Clamp(input.TxDutyFraction, 0.0, 1.0);
        double denom = input.SupplyVoltage * input.TxEfficiencyFraction;
        double txCurrent = denom > 0 ? input.TxPowerWatts / denom : 0.0;
        double avgCurrent = duty * txCurrent + (1.0 - duty) * Math.Max(0.0, input.RxCurrentAmps);
        double usable = input.CapacityAh * Math.Clamp(input.UsableFraction, 0.0, 1.0);
        double runtime = avgCurrent > 0 ? usable / avgCurrent : 0.0;

        return new PowerBudgetResult(txCurrent, avgCurrent, runtime);
    }
}

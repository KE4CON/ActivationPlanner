namespace ActivationPlanner.PropagationModel.Antennas;

/// <summary>Modeled gain at one elevation angle.</summary>
/// <param name="ElevationAngleDeg">Elevation above the horizon in degrees (0 = horizon, 90 = zenith).</param>
/// <param name="GainDbi">Total gain in dBi at that elevation.</param>
public sealed record AntennaPatternSample(double ElevationAngleDeg, double GainDbi);

/// <summary>
/// A modeled antenna pattern for one band, produced by the NEC2 custom modeling path (Option B).
/// Summarizes what the planner needs: peak gain, the take-off (elevation) angle at which it
/// occurs, the feed-point impedance, and the full elevation cut.
/// <para>
/// Elevation angle is measured from the horizon (NEC theta from zenith converted to
/// <c>90 − theta</c>). Domain data — no NEC/VOACAP wire concepts leak in.
/// </para>
/// </summary>
public sealed record AntennaPattern
{
    /// <summary>Frequency the antenna was modeled at, MHz.</summary>
    public required double FrequencyMhz { get; init; }

    /// <summary>Peak total gain, dBi.</summary>
    public required double PeakGainDbi { get; init; }

    /// <summary>Elevation angle of peak gain, degrees above the horizon — the take-off angle.</summary>
    public required double TakeoffAngleDeg { get; init; }

    /// <summary>Feed-point resistance, ohms, if the model reported it.</summary>
    public double? FeedpointResistanceOhms { get; init; }

    /// <summary>Feed-point reactance, ohms, if the model reported it.</summary>
    public double? FeedpointReactanceOhms { get; init; }

    /// <summary>Gain vs elevation angle (ascending elevation).</summary>
    public required IReadOnlyList<AntennaPatternSample> Elevation { get; init; }

    /// <summary>
    /// Set when the model rested on a substituted assumption the operator did not supply — e.g. a
    /// resonant length filled in because the antenna's length was left blank. Null for a fully
    /// specified model. The UI shows this so an estimated pattern is never mistaken for an exact one.
    /// </summary>
    public string? EstimateNote { get; init; }
}

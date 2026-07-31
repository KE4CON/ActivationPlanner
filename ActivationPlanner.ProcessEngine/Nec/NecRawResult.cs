namespace ActivationPlanner.ProcessEngine.Nec;

/// <summary>Feed-point impedance read from NEC's ANTENNA INPUT PARAMETERS, in ohms.</summary>
public sealed record NecImpedance(double ResistanceOhms, double ReactanceOhms);

/// <summary>One radiation-pattern sample from NEC's RADIATION PATTERNS table.</summary>
public sealed record NecRadiationSample(
    double ThetaDeg,
    double PhiDeg,
    double VerticalGainDb,
    double HorizontalGainDb,
    double TotalGainDb);

/// <summary>
/// The raw result of a NEC2 run: feed-point impedance plus the radiation-pattern samples. This is
/// the Layer-1 output; PropagationModel elevates it into a domain antenna pattern (peak gain,
/// take-off angle, elevation cut).
/// </summary>
public sealed record NecRawResult
{
    /// <summary>Feed-point impedance, if the output included ANTENNA INPUT PARAMETERS.</summary>
    public NecImpedance? Impedance { get; init; }

    /// <summary>Radiation-pattern samples in file order.</summary>
    public required IReadOnlyList<NecRadiationSample> Pattern { get; init; }

    /// <summary>The sample with the greatest total gain, or null if the pattern is empty.</summary>
    public NecRadiationSample? PeakGain =>
        Pattern.Count == 0 ? null : Pattern.MaxBy(s => s.TotalGainDb);
}

using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Geo;

namespace ActivationPlanner.PropagationModel.Voacap;

/// <summary>
/// A planner-domain request for a propagation prediction over one HF circuit: from the
/// operator's site to a target receive location, for a set of bands and UTC hours, under
/// the current solar conditions.
/// <para>
/// This is the clean domain input the propagation engine consumes; the ProcessEngine
/// boundary translates it into a VOACAP deck. Mission framing (e.g. EMCOMM's near-in
/// regional receive point vs. a DX target) is decided upstream and arrives here as a
/// concrete <see cref="Receiver"/> — the query itself is mission-agnostic, consistent
/// with the rule that mission type changes the question asked, never the physics.
/// </para>
/// <para>
/// Phase 2 predicts with a default isotropic antenna at both ends; owned-antenna matching
/// and Option A/B modeling arrive in Phase 3.
/// </para>
/// </summary>
public sealed record CircuitQuery
{
    /// <summary>Operator (transmit) site.</summary>
    public required GeoLocation Transmitter { get; init; }

    /// <summary>Target (receive) site — the DX or served-agency location being planned for.</summary>
    public required GeoLocation Receiver { get; init; }

    /// <summary>Prediction month, 1-12.</summary>
    public required int Month { get; init; }

    /// <summary>Prediction year (four digits).</summary>
    public required int Year { get; init; }

    /// <summary>Smoothed sunspot number for the current solar conditions.</summary>
    public required double SunspotNumber { get; init; }

    /// <summary>Bands to evaluate. Defaults to all HF bands.</summary>
    public IReadOnlyList<HamBand> Bands { get; init; } = HamBands.All;

    /// <summary>First UTC hour to predict, inclusive (1-24).</summary>
    public int StartHourUtc { get; init; } = 1;

    /// <summary>Last UTC hour to predict, inclusive (1-24).</summary>
    public int StopHourUtc { get; init; } = 24;

    /// <summary>Hour step.</summary>
    public int HourIncrement { get; init; } = 1;

    /// <summary>Receive-site man-made noise environment.</summary>
    public NoiseEnvironment Noise { get; init; } = NoiseEnvironment.Residential;

    /// <summary>Transmit power in watts.</summary>
    public double TransmitPowerWatts { get; init; } = 100.0;

    /// <summary>Required circuit reliability, percent (VOACAP grade-of-service target).</summary>
    public double RequiredReliabilityPercent { get; init; } = 90.0;

    /// <summary>Required signal-to-noise ratio for the mode, dB-Hz.</summary>
    public double RequiredSnrDb { get; init; } = 24.0;

    /// <summary>Use the long great-circle path instead of the short path.</summary>
    public bool UseLongPath { get; init; }

    /// <summary>Great-circle distance between the two sites, km.</summary>
    public double DistanceKm => Transmitter.GreatCircleKmTo(Receiver);
}

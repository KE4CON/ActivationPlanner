using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.Services.Antennas;

namespace ActivationPlanner.Services.Planning;

/// <summary>
/// An owned antenna considered for a band, paired with its Option A/B modeling decision at
/// that band's frequency. Only owned antennas appear here — acquisition suggestions are a
/// separate, secondary concern and are never mixed in (CLAUDE.md gear-suggestion rule).
/// </summary>
public sealed record AntennaSuggestion
{
    /// <summary>The owned antenna.</summary>
    public required AntennaProfile Antenna { get; init; }

    /// <summary>How this antenna would be modeled on this band (Option A library vs Option B custom).</summary>
    public required AntennaModelingDecision Modeling { get; init; }

    /// <summary>True when a community-library pattern suffices (Option A) — ready to use as-is.</summary>
    public bool IsLibraryReady => Modeling.Option == AntennaModelingOption.LibraryMatch;
}

/// <summary>
/// One band's recommendation: the propagation prediction plus the operator's own antennas
/// ranked by readiness (library-ready first, then those needing custom modeling).
/// </summary>
public sealed record BandRecommendation
{
    /// <summary>The band.</summary>
    public required HamBand Band { get; init; }

    /// <summary>Band display name, e.g. "20m".</summary>
    public string BandName => HamBands.DisplayName(Band);

    /// <summary>Representative frequency the prediction was made at, MHz.</summary>
    public required double FrequencyMhz { get; init; }

    /// <summary>Full per-hour propagation prediction for this band.</summary>
    public required BandPrediction Prediction { get; init; }

    /// <summary>Owned antennas for this band, library-ready first.</summary>
    public required IReadOnlyList<AntennaSuggestion> OwnedAntennas { get; init; }

    /// <summary>Mean reliability across the predicted hours, 0-1.</summary>
    public double AverageReliability => Prediction.AverageReliability;

    /// <summary>Best single-hour reliability, 0-1.</summary>
    public double BestReliability => Prediction.BestReliability;

    /// <summary>UTC hour of best reliability, or null if none evaluated.</summary>
    public int? BestHourUtc => Prediction.BestHourUtc;
}

/// <summary>
/// The core planning result: bands ranked by predicted quality, each carrying its hourly
/// prediction and the operator's matching antennas. Consumed directly by the planning UI.
/// </summary>
public sealed record SessionPlan
{
    /// <summary>Bands ranked best-first by average reliability.</summary>
    public required IReadOnlyList<BandRecommendation> Bands { get; init; }

    /// <summary>UTC hours covered by the prediction, ascending.</summary>
    public required IReadOnlyList<int> HoursUtc { get; init; }

    /// <summary>Great-circle path length, km.</summary>
    public required double DistanceKm { get; init; }

    /// <summary>Transmit-site label, if VOACAP provided one.</summary>
    public string? TransmitterLabel { get; init; }

    /// <summary>Receive-site label, if VOACAP provided one.</summary>
    public string? ReceiverLabel { get; init; }

    /// <summary>True when the operator owns at least one antenna to plan around.</summary>
    public bool HasOwnedAntennas => Bands.Any(b => b.OwnedAntennas.Count > 0);
}

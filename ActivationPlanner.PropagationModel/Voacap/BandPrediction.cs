using ActivationPlanner.PropagationModel.Bands;

namespace ActivationPlanner.PropagationModel.Voacap;

/// <summary>One band's predicted quality at one UTC hour.</summary>
public sealed record BandHourSample
{
    /// <summary>UTC hour of the prediction (1-24).</summary>
    public required int HourUtc { get; init; }

    /// <summary>Predicted circuit reliability, 0-1 — the primary quality figure.</summary>
    public double? Reliability { get; init; }

    /// <summary>Predicted signal-to-noise ratio, dB.</summary>
    public double? Snr { get; init; }

    /// <summary>Median signal power at the receiver, dBW.</summary>
    public double? SignalPowerDbw { get; init; }

    /// <summary>Dominant propagation mode, e.g. "1F2".</summary>
    public string? Mode { get; init; }

    /// <summary>Maximum usable frequency for this hour, MHz (shared across bands in the hour).</summary>
    public double? MufMhz { get; init; }

    /// <summary>True when the band's frequency is above the hour's MUF (propagation unlikely).</summary>
    public bool IsAboveMuf { get; init; }
}

/// <summary>
/// A band's predicted propagation across all requested hours, with simple aggregates the
/// planning UI ranks on. Reliability aggregates ignore hours VOACAP did not evaluate.
/// </summary>
public sealed record BandPrediction
{
    /// <summary>The band.</summary>
    public required HamBand Band { get; init; }

    /// <summary>Representative frequency the band was predicted at, MHz.</summary>
    public required double FrequencyMhz { get; init; }

    /// <summary>Per-hour samples in ascending UTC-hour order.</summary>
    public required IReadOnlyList<BandHourSample> Hours { get; init; }

    /// <summary>Highest reliability across the predicted hours (0-1), or 0 if none evaluated.</summary>
    public double BestReliability =>
        Hours.Select(h => h.Reliability).OfType<double>().DefaultIfEmpty(0).Max();

    /// <summary>UTC hour at which <see cref="BestReliability"/> occurs, or null if none evaluated.</summary>
    public int? BestHourUtc =>
        Hours.Where(h => h.Reliability is not null)
             .OrderByDescending(h => h.Reliability)
             .Select(h => (int?)h.HourUtc)
             .FirstOrDefault();

    /// <summary>Mean reliability across evaluated hours (0-1), or 0 if none evaluated.</summary>
    public double AverageReliability =>
        Hours.Select(h => h.Reliability).OfType<double>().DefaultIfEmpty(0).Average();
}

/// <summary>
/// The full result of a <see cref="CircuitQuery"/>: one <see cref="BandPrediction"/> per
/// requested band, plus the circuit's geometry. Bands are returned lowest-frequency first;
/// callers rank by whatever aggregate suits the view.
/// </summary>
public sealed record CircuitPrediction
{
    /// <summary>Predictions, one per requested band (lowest band first).</summary>
    public required IReadOnlyList<BandPrediction> Bands { get; init; }

    /// <summary>UTC hours that were predicted, ascending.</summary>
    public required IReadOnlyList<int> HoursUtc { get; init; }

    /// <summary>Great-circle path length, km.</summary>
    public required double DistanceKm { get; init; }

    /// <summary>Transmit-site label echoed by VOACAP, if available.</summary>
    public string? TransmitterLabel { get; init; }

    /// <summary>Receive-site label echoed by VOACAP, if available.</summary>
    public string? ReceiverLabel { get; init; }

    /// <summary>Bands ranked best-first by mean reliability across the predicted hours.</summary>
    public IReadOnlyList<BandPrediction> RankByAverageReliability() =>
        Bands.OrderByDescending(b => b.AverageReliability).ThenBy(b => b.FrequencyMhz).ToList();
}

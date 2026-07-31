using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.Services.Planning;

namespace ActivationPlanner.Services.Trend;

/// <summary>
/// One sample in the propagation trend: each band's reliability at the sampled UTC hour, tagged
/// with when it was captured. Session-local working data.
/// </summary>
public sealed record TrendSnapshot
{
    /// <summary>When this sample was captured (UTC).</summary>
    public required DateTime CapturedAtUtc { get; init; }

    /// <summary>The UTC hour (1-24) the reliabilities were read from.</summary>
    public required int HourUtc { get; init; }

    /// <summary>Per-band reliability (0-1) at <see cref="HourUtc"/>; null where a band was not evaluated.</summary>
    public required IReadOnlyDictionary<HamBand, double?> Reliability { get; init; }
}

/// <summary>
/// A rolling, session-local history of propagation samples — "what was predicted recently vs.
/// now" — to support the replanning moment (a band going dead). Keeps only samples within a
/// trailing window and is never persisted, consistent with the stateless-replanning rule.
/// </summary>
public sealed class PropagationTrend
{
    private readonly List<TrendSnapshot> _snapshots = [];
    private readonly TimeSpan _window;

    /// <param name="window">How far back to keep samples (default 4 hours).</param>
    public PropagationTrend(TimeSpan? window = null)
    {
        _window = window ?? TimeSpan.FromHours(4);
    }

    /// <summary>Samples oldest-first.</summary>
    public IReadOnlyList<TrendSnapshot> Snapshots => _snapshots;

    /// <summary>The most recent sample, or null if none yet.</summary>
    public TrendSnapshot? Latest => _snapshots.Count > 0 ? _snapshots[^1] : null;

    /// <summary>Add a sample and drop any now older than the trailing window relative to the newest.</summary>
    public void Add(TrendSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _snapshots.Add(snapshot);

        DateTime cutoff = _snapshots[^1].CapturedAtUtc - _window;
        _snapshots.RemoveAll(s => s.CapturedAtUtc < cutoff);
    }

    /// <summary>The recent reliability series for a band, oldest-first (nulls where unevaluated).</summary>
    public IReadOnlyList<double?> SeriesFor(HamBand band) =>
        _snapshots.Select(s => s.Reliability.TryGetValue(band, out double? r) ? r : null).ToList();

    /// <summary>
    /// Build a snapshot from a <see cref="SessionPlan"/>, reading each band's reliability at the UTC
    /// hour of <paramref name="capturedAtUtc"/>.
    /// </summary>
    public static TrendSnapshot SnapshotFrom(SessionPlan plan, DateTime capturedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(plan);

        // Prediction hours are 1-24; midnight (0) maps to 24.
        int hour = capturedAtUtc.Hour == 0 ? 24 : capturedAtUtc.Hour;

        var reliability = new Dictionary<HamBand, double?>();
        foreach (BandRecommendation band in plan.Bands)
        {
            double? value = band.Prediction.Hours
                .FirstOrDefault(h => h.HourUtc == hour)?.Reliability;
            reliability[band.Band] = value;
        }

        return new TrendSnapshot
        {
            CapturedAtUtc = capturedAtUtc,
            HourUtc = hour,
            Reliability = reliability,
        };
    }
}

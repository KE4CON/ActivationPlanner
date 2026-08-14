using ActivationPlanner.PropagationModel.Geo;

namespace ActivationPlanner.Services.GreyLine;

/// <summary>Which solar event a grey-line window is built around.</summary>
public enum GreyLineEventKind
{
    Sunrise,
    Sunset,
}

/// <summary>
/// One grey-line window: the twilight band of enhanced propagation around a sunrise or sunset,
/// spanning <c>Event ± window</c>. Times are UTC instants.
/// </summary>
public sealed record GreyLinePeriod(GreyLineEventKind Kind, DateTime StartUtc, DateTime EventUtc, DateTime EndUtc)
{
    /// <summary>True when <paramref name="utc"/> falls inside this window.</summary>
    public bool Contains(DateTime utc) => utc >= StartUtc && utc <= EndUtc;
}

/// <summary>
/// The grey-line state at a moment: whether a window is active now, and either the one in progress
/// (with time left) or the next one coming up (with time until it starts).
/// </summary>
public sealed record GreyLineStatus(
    bool IsActive,
    GreyLinePeriod? Current,
    GreyLinePeriod? Next,
    TimeSpan? UntilStart,
    TimeSpan? UntilEnd);

/// <summary>
/// Builds grey-line windows (sunrise/sunset ± a half-window) around "now" and reports whether the
/// grey line is active at a given instant or when the next one starts. Pure: the clock is passed in
/// so it is fully testable. Layer-3 service composing <see cref="SolarCalculator"/> solar data.
/// </summary>
public static class GreyLineTiming
{
    /// <summary>
    /// The grey-line windows spanning yesterday→tomorrow (UTC) for <paramref name="location"/>,
    /// chronologically. The three-day span guarantees "current" and "next" resolve even when a
    /// window straddles UTC midnight.
    /// </summary>
    public static IReadOnlyList<GreyLinePeriod> Windows(
        GeoLocation location, DateTime nowUtc, double windowHours = SolarCalculator.DefaultGreyLineWindowHours)
    {
        var periods = new List<GreyLinePeriod>();
        for (int dayOffset = -1; dayOffset <= 1; dayOffset++)
        {
            DateTime date = nowUtc.Date.AddDays(dayOffset);
            var (sunrise, sunset) = SolarCalculator.EventTimesUtc(location, date.Year, date.Month, date.Day);
            if (sunrise is { } r)
                periods.Add(Window(GreyLineEventKind.Sunrise, r, windowHours));
            if (sunset is { } s)
                periods.Add(Window(GreyLineEventKind.Sunset, s, windowHours));
        }

        // EventTimesUtc anchors each event to its own solar day, so adjacent ForDate days can yield
        // the same instant twice near the boundary — dedupe by event minute, then order by start.
        return periods
            .GroupBy(p => (p.Kind, p.EventUtc.Ticks / TimeSpan.TicksPerMinute))
            .Select(g => g.First())
            .OrderBy(p => p.StartUtc)
            .ToList();
    }

    /// <summary>The grey-line status at <paramref name="nowUtc"/> for <paramref name="location"/>.</summary>
    public static GreyLineStatus StatusAt(
        GeoLocation location, DateTime nowUtc, double windowHours = SolarCalculator.DefaultGreyLineWindowHours)
    {
        var windows = Windows(location, nowUtc, windowHours);

        GreyLinePeriod? current = windows.FirstOrDefault(p => p.Contains(nowUtc));
        if (current is not null)
            return new GreyLineStatus(true, current, null, null, current.EndUtc - nowUtc);

        GreyLinePeriod? next = windows.FirstOrDefault(p => p.StartUtc > nowUtc);
        return new GreyLineStatus(false, null, next, next is null ? null : next.StartUtc - nowUtc, null);
    }

    private static GreyLinePeriod Window(GreyLineEventKind kind, DateTime eventUtc, double windowHours) =>
        new(kind, eventUtc.AddHours(-windowHours), eventUtc, eventUtc.AddHours(windowHours));
}

using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.Services.Planning;

namespace ActivationPlanner.Services.GreyLine;

/// <summary>One band's reliability at a grey-line hour.</summary>
public sealed record GreyLineBand(HamBand Band, string BandName, double FrequencyMhz, double? Reliability);

/// <summary>Bands ranked at the sunrise or sunset grey-line hour.</summary>
public sealed record GreyLineWindow(string Label, int HourUtc, double EventHourUtc, IReadOnlyList<GreyLineBand> Bands);

/// <summary>
/// The grey-line correlation for a location/date: sunrise and sunset windows, each listing the
/// bands VOACAP already ranks (best first) at that hour. Highlights an existing correlation — it
/// never boosts a band's ranking (CLAUDE.md rule).
/// </summary>
public sealed record GreyLineReport(
    bool HasGreyLine,
    double? SunriseUtcHour,
    double? SunsetUtcHour,
    IReadOnlyList<GreyLineWindow> Windows);

/// <summary>
/// Correlates a session plan with the grey-line window: for the plan hours nearest sunrise and
/// sunset, ranks the bands by their predicted reliability. Pure and deterministic.
/// </summary>
public static class GreyLineAnalysis
{
    /// <summary>Build the grey-line report from a plan and the day's solar events.</summary>
    public static GreyLineReport Analyze(SessionPlan plan, SolarEvents events)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(events);

        if (!events.HasGreyLine)
            return new GreyLineReport(false, events.SunriseUtcHour, events.SunsetUtcHour, []);

        var windows = new List<GreyLineWindow>();
        if (events.SunriseUtcHour is { } sunrise)
            windows.Add(BuildWindow("Sunrise", sunrise, plan));
        if (events.SunsetUtcHour is { } sunset)
            windows.Add(BuildWindow("Sunset", sunset, plan));

        return new GreyLineReport(true, events.SunriseUtcHour, events.SunsetUtcHour, windows);
    }

    private static GreyLineWindow BuildWindow(string label, double eventHourUtc, SessionPlan plan)
    {
        int hour = NearestPlanHour(eventHourUtc);

        var bands = plan.Bands
            .Select(b => new GreyLineBand(
                b.Band, b.BandName, b.FrequencyMhz,
                b.Prediction.Hours.FirstOrDefault(h => h.HourUtc == hour)?.Reliability))
            .OrderByDescending(b => b.Reliability ?? -1.0)
            .ToList();

        return new GreyLineWindow(label, hour, eventHourUtc, bands);
    }

    /// <summary>Round a fractional UTC hour to the nearest plan hour (1-24; midnight = 24).</summary>
    private static int NearestPlanHour(double hourUtc) =>
        ((int)Math.Round(hourUtc) + 23) % 24 + 1;
}

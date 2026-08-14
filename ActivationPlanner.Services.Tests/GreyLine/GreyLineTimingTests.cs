using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.Services.GreyLine;

namespace ActivationPlanner.Services.Tests.GreyLine;

public sealed class GreyLineTimingTests
{
    // A mid-latitude location with clear sunrise/sunset on the chosen date.
    private static readonly GeoLocation Loc = new(40, 0);

    [Fact]
    public void Grey_line_is_active_at_the_sunrise_instant()
    {
        var (sunrise, _) = SolarCalculator.EventTimesUtc(Loc, 2026, 3, 21);
        Assert.NotNull(sunrise);

        GreyLineStatus status = GreyLineTiming.StatusAt(Loc, sunrise!.Value);

        Assert.True(status.IsActive);
        Assert.NotNull(status.Current);
        Assert.Equal(GreyLineEventKind.Sunrise, status.Current!.Kind);
        Assert.NotNull(status.UntilEnd);
        // Default window is +/- 1 hour around the event.
        Assert.Equal(status.Current.EventUtc.AddHours(-1), status.Current.StartUtc);
        Assert.Equal(status.Current.EventUtc.AddHours(1), status.Current.EndUtc);
    }

    [Fact]
    public void Three_hours_after_sunrise_is_not_active_and_reports_the_next_window()
    {
        var (sunrise, _) = SolarCalculator.EventTimesUtc(Loc, 2026, 3, 21);
        DateTime midMorning = sunrise!.Value.AddHours(3);

        GreyLineStatus status = GreyLineTiming.StatusAt(Loc, midMorning);

        Assert.False(status.IsActive);
        Assert.Null(status.Current);
        Assert.NotNull(status.Next);
        Assert.NotNull(status.UntilStart);
        Assert.True(status.UntilStart > TimeSpan.Zero);
        // The next window after morning is the sunset window.
        Assert.Equal(GreyLineEventKind.Sunset, status.Next!.Kind);
    }

    [Fact]
    public void Windows_are_returned_in_chronological_order()
    {
        var windows = GreyLineTiming.Windows(Loc, new DateTime(2026, 3, 21, 12, 0, 0, DateTimeKind.Utc));

        Assert.NotEmpty(windows);
        for (int i = 1; i < windows.Count; i++)
            Assert.True(windows[i].StartUtc >= windows[i - 1].StartUtc);
    }

    [Fact]
    public void Polar_day_location_has_no_active_grey_line()
    {
        // 85N in midsummer: no sunrise/sunset, so no windows and never active.
        var when = new DateTime(2026, 6, 21, 12, 0, 0, DateTimeKind.Utc);
        GreyLineStatus status = GreyLineTiming.StatusAt(new GeoLocation(85, 0), when);

        Assert.False(status.IsActive);
        Assert.Null(status.Current);
    }
}

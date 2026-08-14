using ActivationPlanner.PropagationModel.Geo;

namespace ActivationPlanner.PropagationModel.Tests.Geo;

public sealed class SolarCalculatorTests
{
    [Fact]
    public void EventTimesUtc_matches_ForDate_hours_and_returns_utc_instants()
    {
        var loc = new GeoLocation(39.74, -104.99);
        var events = SolarCalculator.ForDate(loc, 2026, 6, 21);
        var (sunrise, sunset) = SolarCalculator.EventTimesUtc(loc, 2026, 6, 21);

        Assert.NotNull(sunrise);
        Assert.NotNull(sunset);
        Assert.Equal(DateTimeKind.Utc, sunrise!.Value.Kind);

        // The absolute instant's hour-of-day matches the hour-of-day form.
        Assert.Equal(events.SunriseUtcHour!.Value, sunrise.Value.TimeOfDay.TotalHours, 3);

        // At 40N/105W in summer, sunset falls after UTC midnight — i.e. the day AFTER the query date.
        Assert.Equal(new DateTime(2026, 6, 22, 0, 0, 0, DateTimeKind.Utc).Date, sunset!.Value.Date);
    }

    [Fact]
    public void EventTimesUtc_is_null_during_polar_day()
    {
        // Far north in midsummer: the sun never sets — no events.
        var (sunrise, sunset) = SolarCalculator.EventTimesUtc(new GeoLocation(80, 0), 2026, 6, 21);
        Assert.Null(sunrise);
        Assert.Null(sunset);
    }

    [Fact]
    public void Computes_plausible_sunrise_sunset_for_colorado_summer()
    {
        // Denver-ish (40N, 105W) on the June solstice: sunrise ~11:33 UTC, sunset ~02:32 UTC.
        var events = SolarCalculator.ForDate(new GeoLocation(39.74, -104.99), 2026, 6, 21);

        Assert.True(events.HasGreyLine);
        Assert.InRange(events.SunriseUtcHour!.Value, 10.5, 12.5);
        Assert.InRange(events.SunsetUtcHour!.Value, 1.5, 3.5);
    }

    [Fact]
    public void Computes_plausible_sunrise_sunset_for_london_equinox()
    {
        // London (~51.5N, 0) near the March equinox: ~06:00 UTC sunrise, ~18:00 UTC sunset.
        var events = SolarCalculator.ForDate(new GeoLocation(51.48, -0.0015), 2026, 3, 20);

        Assert.InRange(events.SunriseUtcHour!.Value, 5.5, 6.7);
        Assert.InRange(events.SunsetUtcHour!.Value, 17.5, 18.7);
    }

    [Fact]
    public void Polar_summer_has_no_grey_line()
    {
        // High Arctic at the solstice: 24-hour daylight, no sunrise/sunset.
        var events = SolarCalculator.ForDate(new GeoLocation(80.0, 0.0), 2026, 6, 21);
        Assert.False(events.HasGreyLine);
        Assert.Null(events.SunriseUtcHour);
        Assert.Null(events.SunsetUtcHour);
    }

    [Fact]
    public void IsWithinGreyLine_flags_hours_near_sunrise_and_sunset()
    {
        var events = new SolarEvents(SunriseUtcHour: 12.0, SunsetUtcHour: 2.0);

        Assert.True(SolarCalculator.IsWithinGreyLine(12.0, events));  // at sunrise
        Assert.True(SolarCalculator.IsWithinGreyLine(2.5, events));   // near sunset
        Assert.False(SolarCalculator.IsWithinGreyLine(18.0, events)); // midday, far from both
    }

    [Fact]
    public void IsWithinGreyLine_wraps_across_midnight()
    {
        var events = new SolarEvents(SunriseUtcHour: 6.0, SunsetUtcHour: 23.5);
        // 00:30 UTC is one hour after a 23:30 sunset, across the midnight wrap.
        Assert.True(SolarCalculator.IsWithinGreyLine(0.5, events));
    }
}

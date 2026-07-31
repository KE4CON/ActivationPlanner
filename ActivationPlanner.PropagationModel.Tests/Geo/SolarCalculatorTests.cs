using ActivationPlanner.PropagationModel.Geo;

namespace ActivationPlanner.PropagationModel.Tests.Geo;

public sealed class SolarCalculatorTests
{
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

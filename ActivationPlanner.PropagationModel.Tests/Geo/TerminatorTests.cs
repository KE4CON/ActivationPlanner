using ActivationPlanner.PropagationModel.Geo;

namespace ActivationPlanner.PropagationModel.Tests.Geo;

public sealed class TerminatorTests
{
    // June solstice, noon UTC: sun is over the equator's 0° meridian at ~+23.4° declination.
    private static readonly DateTime JuneSolstice = new(2026, 6, 21, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Subsolar_point_at_june_solstice_noon_is_near_tropic_of_cancer_and_greenwich()
    {
        var sub = SolarCalculator.SubsolarPoint(JuneSolstice);
        Assert.Equal(23.4, sub.LatitudeDeg, precision: 0);   // ~ +23.4° declination
        Assert.InRange(sub.LongitudeDeg, -3.0, 3.0);          // ~ 0° at 12:00 UTC
    }

    [Fact]
    public void Subsolar_longitude_tracks_utc_hour()
    {
        var at0 = SolarCalculator.SubsolarPoint(new DateTime(2026, 6, 21, 0, 0, 0, DateTimeKind.Utc));
        var at18 = SolarCalculator.SubsolarPoint(new DateTime(2026, 6, 21, 18, 0, 0, DateTimeKind.Utc));
        Assert.InRange(Math.Abs(at0.LongitudeDeg), 177, 180); // ~ ±180 at 00:00 UTC
        Assert.InRange(at18.LongitudeDeg, -93, -87);          // ~ -90 (90°W) at 18:00 UTC
    }

    [Fact]
    public void Subsolar_point_is_in_daylight_its_antipode_is_night()
    {
        var sub = SolarCalculator.SubsolarPoint(JuneSolstice);
        var here = new GeoLocation(sub.LatitudeDeg, sub.LongitudeDeg);
        var antipode = new GeoLocation(-sub.LatitudeDeg, SolarCalculator.NormalizeLongitude(sub.LongitudeDeg + 180));

        Assert.False(Terminator.IsNight(here, JuneSolstice));
        Assert.True(Terminator.IsNight(antipode, JuneSolstice));
    }

    [Fact]
    public void Poles_follow_the_season()
    {
        // Northern summer solstice: north pole in continuous daylight, south pole in darkness.
        Assert.False(Terminator.IsNight(new GeoLocation(89.9, 0), JuneSolstice));
        Assert.True(Terminator.IsNight(new GeoLocation(-89.9, 0), JuneSolstice));
    }

    [Fact]
    public void Solar_elevation_is_high_at_the_subsolar_point()
    {
        var sub = SolarCalculator.SubsolarPoint(JuneSolstice);
        double elevation = Terminator.SolarElevationDeg(new GeoLocation(sub.LatitudeDeg, sub.LongitudeDeg), JuneSolstice);
        Assert.InRange(elevation, 89.0, 90.0); // sun overhead
    }

    [Fact]
    public void Terminator_latitude_is_opposite_hemisphere_from_the_summer_sun()
    {
        // In northern summer the terminator dips into the southern hemisphere at the noon meridian.
        double lat = Terminator.TerminatorLatitudeDeg(0.0, JuneSolstice);
        Assert.True(lat < 0, "terminator latitude at the subsolar meridian should be southern in June");
    }
}

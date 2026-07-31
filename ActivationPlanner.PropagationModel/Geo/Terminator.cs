namespace ActivationPlanner.PropagationModel.Geo;

/// <summary>
/// The day/night terminator — the "grey line" itself. Given a UTC instant it can say whether any
/// point is in daylight or darkness and, for a world-map view, the latitude of the terminator at a
/// given longitude. Built on the sub-solar point from <see cref="SolarCalculator"/>.
/// <para>Pure domain math — no clock, no I/O.</para>
/// </summary>
public static class Terminator
{
    /// <summary>True when the point is in darkness (sun below the horizon) at <paramref name="utc"/>.</summary>
    public static bool IsNight(GeoLocation point, DateTime utc) =>
        SinSolarElevation(point, SolarCalculator.SubsolarPoint(utc)) < 0;

    /// <summary>Solar elevation angle (degrees) at a point — positive by day, negative by night.</summary>
    public static double SolarElevationDeg(GeoLocation point, DateTime utc) =>
        Rad2Deg(Math.Asin(Math.Clamp(SinSolarElevation(point, SolarCalculator.SubsolarPoint(utc)), -1, 1)));

    /// <summary>
    /// Latitude of the terminator at <paramref name="longitudeDeg"/> for <paramref name="utc"/> — the
    /// grey line curve for a world map. Returns 0 at the equinoxes (declination ≈ 0), where the
    /// terminator runs along meridians.
    /// </summary>
    public static double TerminatorLatitudeDeg(double longitudeDeg, DateTime utc)
    {
        GeoLocation sub = SolarCalculator.SubsolarPoint(utc);
        double declRad = Deg2Rad(sub.LatitudeDeg);

        if (Math.Abs(Math.Tan(declRad)) < 1e-6)
            return 0.0; // near-equinox: terminator is essentially a meridian, latitude is indeterminate

        double dLon = Deg2Rad(longitudeDeg - sub.LongitudeDeg);
        double phi = Math.Atan(-Math.Cos(dLon) / Math.Tan(declRad));
        return Rad2Deg(phi);
    }

    private static double SinSolarElevation(GeoLocation point, GeoLocation subsolar)
    {
        double phi = Deg2Rad(point.LatitudeDeg);
        double decl = Deg2Rad(subsolar.LatitudeDeg);
        double dLon = Deg2Rad(point.LongitudeDeg - subsolar.LongitudeDeg);
        return Math.Sin(phi) * Math.Sin(decl) + Math.Cos(phi) * Math.Cos(decl) * Math.Cos(dLon);
    }

    private static double Deg2Rad(double d) => d * Math.PI / 180.0;
    private static double Rad2Deg(double r) => r * 180.0 / Math.PI;
}

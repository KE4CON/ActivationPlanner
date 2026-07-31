namespace ActivationPlanner.PropagationModel.Geo;

/// <summary>Sunrise and sunset for a location and date, in UTC hours (0-24), or null when the sun does not cross the horizon.</summary>
public sealed record SolarEvents(double? SunriseUtcHour, double? SunsetUtcHour)
{
    /// <summary>True when both a sunrise and a sunset occur on the day (i.e. not polar day/night).</summary>
    public bool HasGreyLine => SunriseUtcHour is not null && SunsetUtcHour is not null;
}

/// <summary>
/// Computes sunrise/sunset from latitude, longitude, and date using the standard sunrise equation.
/// Feeds the grey-line indicator: the app highlights when a band VOACAP already ranks well
/// coincides with the sunrise/sunset window — it never boosts a band's ranking (CLAUDE.md), so
/// this is purely informational.
/// <para>Pure domain math — no clock, no I/O.</para>
/// </summary>
public static class SolarCalculator
{
    /// <summary>Default grey-line half-window around sunrise/sunset, in hours.</summary>
    public const double DefaultGreyLineWindowHours = 1.0;

    private const double SunAltitudeDeg = -0.833; // standard refraction-corrected sunrise/sunset altitude

    /// <summary>Sunrise/sunset (UTC) for <paramref name="location"/> on the given date.</summary>
    public static SolarEvents ForDate(GeoLocation location, int year, int month, int day)
    {
        double lat = location.LatitudeDeg;
        double lonEast = location.LongitudeDeg;

        double jdn = JulianDayNumber(year, month, day);
        double n = jdn - 2451545.0 + 0.0008;
        // Solar noon shifts later in UTC for western (negative-east) longitudes.
        double meanSolarNoon = n - lonEast / 360.0;

        double m = Mod360(357.5291 + 0.98560028 * meanSolarNoon);
        double mRad = Deg2Rad(m);
        double center = 1.9148 * Math.Sin(mRad) + 0.0200 * Math.Sin(2 * mRad) + 0.0003 * Math.Sin(3 * mRad);
        double lambda = Mod360(m + center + 180 + 102.9372);
        double lambdaRad = Deg2Rad(lambda);

        double jTransit = 2451545.0 + meanSolarNoon + 0.0053 * Math.Sin(mRad) - 0.0069 * Math.Sin(2 * lambdaRad);
        double sinDecl = Math.Sin(lambdaRad) * Math.Sin(Deg2Rad(23.44));
        double decl = Math.Asin(sinDecl);

        double latRad = Deg2Rad(lat);
        double cosOmega = (Math.Sin(Deg2Rad(SunAltitudeDeg)) - Math.Sin(latRad) * sinDecl)
                          / (Math.Cos(latRad) * Math.Cos(decl));

        // |cos| > 1: the sun never crosses the horizon (polar day or night) — no grey line.
        if (cosOmega is > 1 or < -1)
            return new SolarEvents(null, null);

        double omega = Rad2Deg(Math.Acos(cosOmega));
        double jRise = jTransit - omega / 360.0;
        double jSet = jTransit + omega / 360.0;

        return new SolarEvents(ToUtcHour(jRise), ToUtcHour(jSet));
    }

    /// <summary>
    /// The point on Earth where the sun is directly overhead at <paramref name="utc"/> — latitude is
    /// the solar declination, longitude is where it is local solar noon. Used to draw the day/night
    /// terminator (grey line) on a world map. Longitude uses the mean sun (equation of time ignored —
    /// within ~1° for display).
    /// </summary>
    public static GeoLocation SubsolarPoint(DateTime utc)
    {
        double utcHour = utc.TimeOfDay.TotalHours;
        double jdn = JulianDayNumber(utc.Year, utc.Month, utc.Day);
        double n = jdn - 2451545.0 + 0.0008 + (utcHour - 12.0) / 24.0;

        double m = Mod360(357.5291 + 0.98560028 * n);
        double mRad = Deg2Rad(m);
        double center = 1.9148 * Math.Sin(mRad) + 0.0200 * Math.Sin(2 * mRad) + 0.0003 * Math.Sin(3 * mRad);
        double lambda = Mod360(m + center + 180 + 102.9372);
        double declDeg = Rad2Deg(Math.Asin(Math.Sin(Deg2Rad(lambda)) * Math.Sin(Deg2Rad(23.44))));

        double subLon = NormalizeLongitude(15.0 * (12.0 - utcHour));
        return new GeoLocation(declDeg, subLon);
    }

    /// <summary>Normalize a longitude into the range (-180, 180].</summary>
    public static double NormalizeLongitude(double lon)
    {
        double x = (lon + 180.0) % 360.0;
        if (x < 0) x += 360.0;
        return x - 180.0;
    }

    /// <summary>
    /// True when <paramref name="hourUtc"/> lies within <paramref name="windowHours"/> of either the
    /// sunrise or sunset (wrapping across midnight).
    /// </summary>
    public static bool IsWithinGreyLine(double hourUtc, SolarEvents events, double windowHours = DefaultGreyLineWindowHours)
    {
        return (events.SunriseUtcHour is { } sr && CircularHourDistance(hourUtc, sr) <= windowHours)
            || (events.SunsetUtcHour is { } ss && CircularHourDistance(hourUtc, ss) <= windowHours);
    }

    private static double CircularHourDistance(double a, double b)
    {
        double d = Math.Abs(a - b) % 24.0;
        return Math.Min(d, 24.0 - d);
    }

    // JDN at noon of the given calendar date (Fliegel & Van Flandern).
    private static double JulianDayNumber(int year, int month, int day)
    {
        int a = (14 - month) / 12;
        int y = year + 4800 - a;
        int m = month + 12 * a - 3;
        return day + (153 * m + 2) / 5 + 365 * y + y / 4 - y / 100 + y / 400 - 32045;
    }

    // Julian date .0 == 12:00 UTC, so shift by +12 h to get an hour-of-day.
    private static double ToUtcHour(double julianDate)
    {
        double frac = julianDate - Math.Floor(julianDate);
        return Mod24(frac * 24.0 + 12.0);
    }

    private static double Deg2Rad(double d) => d * Math.PI / 180.0;
    private static double Rad2Deg(double r) => r * 180.0 / Math.PI;
    private static double Mod360(double x) => x - 360.0 * Math.Floor(x / 360.0);
    private static double Mod24(double x) => x - 24.0 * Math.Floor(x / 24.0);
}

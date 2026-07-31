namespace ActivationPlanner.PropagationModel.Geo;

/// <summary>
/// A geographic point in signed decimal degrees (North and East positive), the form
/// GPS and mapping sources produce. VOACAP's magnitude + hemisphere representation is a
/// wire-format detail handled at the ProcessEngine boundary, not here.
/// </summary>
public readonly record struct GeoLocation
{
    public GeoLocation(double latitudeDeg, double longitudeDeg)
    {
        if (latitudeDeg is < -90 or > 90)
            throw new ArgumentOutOfRangeException(nameof(latitudeDeg), latitudeDeg, "Latitude must be within ±90°.");
        if (longitudeDeg is < -180 or > 180)
            throw new ArgumentOutOfRangeException(nameof(longitudeDeg), longitudeDeg, "Longitude must be within ±180°.");
        LatitudeDeg = latitudeDeg;
        LongitudeDeg = longitudeDeg;
    }

    /// <summary>Latitude in signed decimal degrees, North positive.</summary>
    public double LatitudeDeg { get; }

    /// <summary>Longitude in signed decimal degrees, East positive.</summary>
    public double LongitudeDeg { get; }

    /// <summary>Great-circle distance to <paramref name="other"/> in kilometres (haversine, spherical Earth).</summary>
    public double GreatCircleKmTo(GeoLocation other)
    {
        const double earthRadiusKm = 6371.0;
        double dLat = DegToRad(other.LatitudeDeg - LatitudeDeg);
        double dLon = DegToRad(other.LongitudeDeg - LongitudeDeg);
        double lat1 = DegToRad(LatitudeDeg);
        double lat2 = DegToRad(other.LatitudeDeg);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                   + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double DegToRad(double deg) => deg * Math.PI / 180.0;
}

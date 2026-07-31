using System.Globalization;
using ActivationPlanner.PropagationModel.Geo;

namespace ActivationPlanner.Services.Location;

/// <summary>
/// Parses a single NMEA 0183 sentence from a GPS receiver into a position fix. Handles the two
/// position-bearing sentences (GGA and RMC) from any talker (GP/GN/GL/GA...), validates the
/// checksum when present, and returns null for sentences that carry no valid fix.
/// <para>Pure and deterministic — no serial I/O — so it is fully unit-tested; the serial reading
/// lives in <see cref="SerialGpsLocationProvider"/>.</para>
/// </summary>
public static class NmeaLocationParser
{
    /// <summary>Parse a position fix from an NMEA sentence, or null if it is not a valid fix.</summary>
    public static GeoLocation? TryParse(string? sentence)
    {
        if (string.IsNullOrWhiteSpace(sentence))
            return null;

        string line = sentence.Trim();
        if (line.Length < 6 || line[0] != '$')
            return null;

        // Split off and verify the checksum (if any).
        int star = line.IndexOf('*');
        if (star >= 0)
        {
            string body = line[1..star];
            string checksumText = line[(star + 1)..].Trim();
            if (checksumText.Length >= 2 && !ChecksumMatches(body, checksumText[..2]))
                return null;
            line = line[..star];
        }

        string[] f = line.Split(',');
        if (f[0].Length < 6)
            return null;
        string type = f[0][^3..]; // GGA / RMC (ignore talker prefix)

        return type switch
        {
            "GGA" => ParseGga(f),
            "RMC" => ParseRmc(f),
            _ => null,
        };
    }

    // $--GGA,time,lat,N/S,lon,E/W,fixQuality,...
    private static GeoLocation? ParseGga(string[] f)
    {
        if (f.Length < 7)
            return null;
        // Fix quality 0 = no fix.
        if (f[6] is "0" or "")
            return null;
        return BuildFix(f[2], f[3], f[4], f[5]);
    }

    // $--RMC,time,status(A/V),lat,N/S,lon,E/W,...
    private static GeoLocation? ParseRmc(string[] f)
    {
        if (f.Length < 7)
            return null;
        if (f[2] != "A") // V = navigation receiver warning (no valid fix)
            return null;
        return BuildFix(f[3], f[4], f[5], f[6]);
    }

    private static GeoLocation? BuildFix(string lat, string ns, string lon, string ew)
    {
        double? latitude = ParseCoordinate(lat, degreeDigits: 2);
        double? longitude = ParseCoordinate(lon, degreeDigits: 3);
        if (latitude is null || longitude is null)
            return null;

        double latSigned = ns.Equals("S", StringComparison.OrdinalIgnoreCase) ? -latitude.Value : latitude.Value;
        double lonSigned = ew.Equals("W", StringComparison.OrdinalIgnoreCase) ? -longitude.Value : longitude.Value;

        if (latSigned is < -90 or > 90 || lonSigned is < -180 or > 180)
            return null;
        return new GeoLocation(latSigned, lonSigned);
    }

    // NMEA coordinates are (d)ddmm.mmmm — degrees then minutes, packed together.
    private static double? ParseCoordinate(string value, int degreeDigits)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double raw))
            return null;

        double degrees = Math.Floor(raw / 100.0);
        double minutes = raw - degrees * 100.0;
        _ = degreeDigits; // documents the format; the /100 split handles both 2- and 3-digit degrees
        return degrees + minutes / 60.0;
    }

    private static bool ChecksumMatches(string body, string expectedHex)
    {
        int checksum = 0;
        foreach (char c in body)
            checksum ^= c;
        return checksum.ToString("X2", CultureInfo.InvariantCulture)
            .Equals(expectedHex, StringComparison.OrdinalIgnoreCase);
    }
}

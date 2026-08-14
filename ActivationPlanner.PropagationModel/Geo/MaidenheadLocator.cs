using System;
using System.Text;

namespace ActivationPlanner.PropagationModel.Geo;

/// <summary>
/// Maidenhead grid-locator conversion (the "grid square" hams use, e.g. EM29 / EM29ok). Lets an
/// operator read a grid off their phone (POTA app, APRS, radio) and type it in — a reliable way to
/// set location in the field, where geo-IP over a cellular hotspot is often far off. Pure domain math.
/// </summary>
public static class MaidenheadLocator
{
    /// <summary>
    /// Grid square for a location. <paramref name="pairs"/> = 2 gives a 4-character grid (e.g. "EM29");
    /// 3 gives a 6-character grid (e.g. "EM29ok").
    /// </summary>
    public static string ToGrid(GeoLocation location, int pairs = 3)
    {
        double lon = Math.Clamp(location.LongitudeDeg + 180.0, 0.0, 359.99999);
        double lat = Math.Clamp(location.LatitudeDeg + 90.0, 0.0, 179.99999);

        var sb = new StringBuilder(pairs * 2);

        sb.Append((char)('A' + (int)(lon / 20)));
        sb.Append((char)('A' + (int)(lat / 10)));
        lon %= 20; lat %= 10;

        sb.Append((char)('0' + (int)(lon / 2)));
        sb.Append((char)('0' + (int)(lat / 1)));
        lon %= 2; lat %= 1;

        if (pairs >= 3)
        {
            sb.Append((char)('a' + (int)(lon / (2.0 / 24))));
            sb.Append((char)('a' + (int)(lat / (1.0 / 24))));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parse a 4- or 6-character grid square to the location at the <b>center</b> of that square.
    /// Case-insensitive; returns false for anything that isn't a valid grid.
    /// </summary>
    public static bool TryParse(string? grid, out GeoLocation location)
    {
        location = default!;
        if (string.IsNullOrWhiteSpace(grid))
            return false;

        string g = grid.Trim();
        if (g.Length < 4)
            return false;

        char f0 = char.ToUpperInvariant(g[0]);
        char f1 = char.ToUpperInvariant(g[1]);
        if (f0 is < 'A' or > 'R' || f1 is < 'A' or > 'R')
            return false;
        if (!char.IsDigit(g[2]) || !char.IsDigit(g[3]))
            return false;

        double lon = (f0 - 'A') * 20 + (g[2] - '0') * 2;
        double lat = (f1 - 'A') * 10 + (g[3] - '0') * 1;

        if (g.Length >= 6)
        {
            char s4 = char.ToLowerInvariant(g[4]);
            char s5 = char.ToLowerInvariant(g[5]);
            if (s4 is < 'a' or > 'x' || s5 is < 'a' or > 'x')
                return false;
            lon += (s4 - 'a') * (2.0 / 24) + (2.0 / 24) / 2; // + half a subsquare = center
            lat += (s5 - 'a') * (1.0 / 24) + (1.0 / 24) / 2;
        }
        else
        {
            lon += 1.0; // + half a square = center
            lat += 0.5;
        }

        location = new GeoLocation(lat - 90.0, lon - 180.0);
        return true;
    }
}

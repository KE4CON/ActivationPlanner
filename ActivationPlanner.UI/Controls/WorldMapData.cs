using System;
using System.Collections.Generic;
using System.Text.Json;
using Avalonia.Platform;

namespace ActivationPlanner.UI.Controls;

/// <summary>A closed ring of (longitude, latitude) points — one landmass outline.</summary>
public sealed record MapRing(IReadOnlyList<(double Lon, double Lat)> Points);

/// <summary>
/// Loads the bundled world land outlines (Natural Earth 110m, public domain) once and caches them
/// for the grey-line map. Only exterior rings are kept — enough to render recognizable continents
/// at this scale. Failures degrade gracefully to an empty set (the map still draws the graticule
/// and terminator).
/// </summary>
public static class WorldMapData
{
    private static IReadOnlyList<MapRing>? _land;

    /// <summary>Land outlines, loaded lazily. Empty if the asset can't be read.</summary>
    public static IReadOnlyList<MapRing> Land => _land ??= Load();

    private static IReadOnlyList<MapRing> Load()
    {
        try
        {
            var uri = new Uri("avares://ActivationPlanner.UI/Assets/ne_110m_land.geojson");
            using var stream = AssetLoader.Open(uri);
            using var doc = JsonDocument.Parse(stream);

            var rings = new List<MapRing>();
            if (!doc.RootElement.TryGetProperty("features", out var features))
                return rings;

            foreach (var feature in features.EnumerateArray())
            {
                if (!feature.TryGetProperty("geometry", out var geom)
                    || !geom.TryGetProperty("type", out var typeEl)
                    || !geom.TryGetProperty("coordinates", out var coords))
                    continue;

                switch (typeEl.GetString())
                {
                    case "Polygon":
                        AddPolygon(coords, rings);
                        break;
                    case "MultiPolygon":
                        foreach (var poly in coords.EnumerateArray())
                            AddPolygon(poly, rings);
                        break;
                }
            }

            return rings;
        }
        catch
        {
            return [];
        }
    }

    // A polygon is an array of rings; keep the exterior (first) ring.
    private static void AddPolygon(JsonElement polygon, List<MapRing> rings)
    {
        var enumerator = polygon.EnumerateArray();
        if (!enumerator.MoveNext())
            return;

        JsonElement exterior = enumerator.Current;
        var points = new List<(double, double)>(exterior.GetArrayLength());
        foreach (var coord in exterior.EnumerateArray())
        {
            if (coord.GetArrayLength() >= 2)
                points.Add((coord[0].GetDouble(), coord[1].GetDouble()));
        }

        if (points.Count >= 3)
            rings.Add(new MapRing(points));
    }
}

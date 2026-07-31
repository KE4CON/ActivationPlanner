using System.Globalization;
using System.Text.Json;

namespace ActivationPlanner.Services.Pota;

/// <summary>
/// Pure parsing of api.pota.app JSON into the POTA models. Kept separate from the HTTP client so
/// it is unit-testable against captured real API payloads (the response shapes are pinned by
/// fixtures in the Services tests). Tolerant of missing/null fields.
/// </summary>
public static class PotaJson
{
    /// <summary>Parse the activator spot feed (a JSON array of spot objects).</summary>
    /// <exception cref="PotaFormatException">The body is not a JSON array of spots.</exception>
    public static IReadOnlyList<PotaSpot> ParseSpots(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using JsonDocument doc = ParseDocument(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            throw new PotaFormatException("Expected a JSON array of POTA spots.");

        var spots = new List<PotaSpot>(doc.RootElement.GetArrayLength());
        foreach (JsonElement el in doc.RootElement.EnumerateArray())
        {
            if (el.ValueKind != JsonValueKind.Object)
                continue;

            long? spotId = ReadLong(el, "spotId");
            string? activator = ReadString(el, "activator");
            double? khz = ReadDouble(el, "frequency");
            string? reference = ReadString(el, "reference");

            // Skip malformed rows rather than failing the whole feed.
            if (spotId is null || activator is null || khz is null || reference is null)
                continue;

            spots.Add(new PotaSpot
            {
                SpotId = spotId.Value,
                Activator = activator,
                FrequencyKhz = khz.Value,
                Reference = reference,
                Mode = ReadString(el, "mode"),
                ParkName = ReadString(el, "name") ?? ReadString(el, "parkName"),
                Spotter = ReadString(el, "spotter"),
                Comments = ReadString(el, "comments"),
                SpotTimeUtc = ReadUtc(el, "spotTime"),
                LocationDesc = ReadString(el, "locationDesc"),
                Grid = ReadString(el, "grid6") ?? ReadString(el, "grid4"),
                Latitude = ReadDouble(el, "latitude"),
                Longitude = ReadDouble(el, "longitude"),
            });
        }

        return spots;
    }

    /// <summary>Parse a single park detail object.</summary>
    /// <exception cref="PotaFormatException">The body is not a park object with the required fields.</exception>
    public static PotaPark ParsePark(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using JsonDocument doc = ParseDocument(json);
        JsonElement el = doc.RootElement;
        if (el.ValueKind != JsonValueKind.Object)
            throw new PotaFormatException("Expected a JSON object for the POTA park.");

        int? parkId = (int?)ReadLong(el, "parkId");
        string? reference = ReadString(el, "reference");
        string? name = ReadString(el, "name");
        if (parkId is null || reference is null || name is null)
            throw new PotaFormatException("POTA park response was missing required fields.");

        return new PotaPark
        {
            ParkId = parkId.Value,
            Reference = reference,
            Name = name,
            Latitude = ReadDouble(el, "latitude"),
            Longitude = ReadDouble(el, "longitude"),
            Grid = ReadString(el, "grid6") ?? ReadString(el, "grid4"),
            ParkType = ReadString(el, "parktypeDesc"),
            Active = ReadLong(el, "active") is > 0,
            LocationDesc = ReadString(el, "locationDesc"),
            LocationName = ReadString(el, "locationName"),
            EntityName = ReadString(el, "entityName"),
            Website = ReadString(el, "website"),
            Comments = ReadString(el, "parkComments"),
        };
    }

    private static JsonDocument ParseDocument(string json)
    {
        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new PotaFormatException("POTA response was not valid JSON.", ex);
        }
    }

    private static string? ReadString(JsonElement el, string name) =>
        el.TryGetProperty(name, out JsonElement p) && p.ValueKind == JsonValueKind.String
            ? p.GetString()
            : null;

    private static long? ReadLong(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out JsonElement p))
            return null;
        return p.ValueKind switch
        {
            JsonValueKind.Number when p.TryGetInt64(out long v) => v,
            JsonValueKind.String when long.TryParse(p.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long v) => v,
            _ => null,
        };
    }

    private static double? ReadDouble(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out JsonElement p))
            return null;
        return p.ValueKind switch
        {
            JsonValueKind.Number when p.TryGetDouble(out double v) => v,
            JsonValueKind.String when double.TryParse(p.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double v) => v,
            _ => null,
        };
    }

    private static DateTime? ReadUtc(JsonElement el, string name)
    {
        string? s = ReadString(el, name);
        // POTA spot times are UTC without an explicit offset.
        return s is not null
            && DateTime.TryParse(s, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime dt)
            ? dt
            : null;
    }
}

/// <summary>Thrown when a POTA response cannot be parsed into the expected shape.</summary>
public sealed class PotaFormatException : Exception
{
    public PotaFormatException(string message) : base(message) { }
    public PotaFormatException(string message, Exception inner) : base(message, inner) { }
}

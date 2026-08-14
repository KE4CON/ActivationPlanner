using System.Linq;
using System.Text.Json;

namespace ActivationPlanner.Services.Weather;

/// <summary>
/// Pure parsers for the US National Weather Service (api.weather.gov) JSON — kept separate from the
/// HTTP client so they are fully unit-testable. The NWS flow is two steps: a /points response points
/// at a forecast URL (and names the place), then the forecast response carries the periods.
/// </summary>
public static class WeatherJson
{
    /// <summary>From a /points/{lat},{lon} response, read the forecast URL and a friendly place name.</summary>
    public static (string ForecastUrl, string? LocationName) ParsePointReference(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using JsonDocument doc = ParseDocument(json);
        if (!doc.RootElement.TryGetProperty("properties", out JsonElement props))
            throw new WeatherFormatException("Weather point response had no 'properties'.");

        if (!props.TryGetProperty("forecast", out JsonElement forecastEl) || forecastEl.ValueKind != JsonValueKind.String)
            throw new WeatherFormatException("Weather point response had no forecast URL (location may be outside the US).");

        string forecastUrl = forecastEl.GetString()!;
        string? location = ReadRelativeLocation(props);
        return (forecastUrl, location);
    }

    /// <summary>From a forecast response, read the periods (soonest first).</summary>
    public static IReadOnlyList<WeatherPeriod> ParsePeriods(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using JsonDocument doc = ParseDocument(json);
        if (!doc.RootElement.TryGetProperty("properties", out JsonElement props)
            || !props.TryGetProperty("periods", out JsonElement periods)
            || periods.ValueKind != JsonValueKind.Array)
        {
            throw new WeatherFormatException("Weather forecast response had no 'periods' array.");
        }

        var list = new List<WeatherPeriod>();
        foreach (JsonElement p in periods.EnumerateArray())
        {
            list.Add(new WeatherPeriod
            {
                Name = ReadString(p, "name") ?? "—",
                IsDaytime = p.TryGetProperty("isDaytime", out JsonElement d) && d.ValueKind == JsonValueKind.True,
                Temperature = p.TryGetProperty("temperature", out JsonElement t) && t.TryGetInt32(out int temp) ? temp : 0,
                TemperatureUnit = ReadString(p, "temperatureUnit") ?? "F",
                WindSpeed = ReadString(p, "windSpeed"),
                WindDirection = ReadString(p, "windDirection"),
                ShortForecast = ReadString(p, "shortForecast"),
                DetailedForecast = ReadString(p, "detailedForecast"),
            });
        }
        return list;
    }

    /// <summary>From an /alerts/active response, read the active alerts (most severe first).</summary>
    public static IReadOnlyList<WeatherAlert> ParseAlerts(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        using JsonDocument doc = ParseDocument(json);
        if (!doc.RootElement.TryGetProperty("features", out JsonElement features)
            || features.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var list = new List<WeatherAlert>();
        foreach (JsonElement f in features.EnumerateArray())
        {
            if (!f.TryGetProperty("properties", out JsonElement p))
                continue;
            string? ev = ReadString(p, "event");
            if (ev is null)
                continue;
            list.Add(new WeatherAlert
            {
                Id = ReadString(f, "id") ?? ReadString(p, "id"),
                Event = ev,
                Headline = ReadString(p, "headline"),
                Severity = ReadString(p, "severity"),
                Urgency = ReadString(p, "urgency"),
                AreaDesc = ReadString(p, "areaDesc"),
                Description = ReadString(p, "description"),
                Instruction = ReadString(p, "instruction"),
                Expires = ReadString(p, "expires"),
            });
        }

        return list.OrderByDescending(a => a.SeverityRank).ToList();
    }

    private static string? ReadRelativeLocation(JsonElement props)
    {
        if (!props.TryGetProperty("relativeLocation", out JsonElement rel)
            || !rel.TryGetProperty("properties", out JsonElement relProps))
        {
            return null;
        }
        string? city = ReadString(relProps, "city");
        string? state = ReadString(relProps, "state");
        if (city is null)
            return state;
        return state is null ? city : $"{city}, {state}";
    }

    private static string? ReadString(JsonElement obj, string name) =>
        obj.TryGetProperty(name, out JsonElement el) && el.ValueKind == JsonValueKind.String
            ? el.GetString()
            : null;

    private static JsonDocument ParseDocument(string json)
    {
        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new WeatherFormatException("Weather response was not valid JSON.", ex);
        }
    }
}

namespace ActivationPlanner.Services.Weather;

/// <summary>One forecast period (e.g. "This Afternoon", "Tonight") from the US National Weather Service.</summary>
public sealed record WeatherPeriod
{
    public required string Name { get; init; }
    public bool IsDaytime { get; init; }
    public int Temperature { get; init; }
    public string TemperatureUnit { get; init; } = "F";
    public string? WindSpeed { get; init; }
    public string? WindDirection { get; init; }
    public string? ShortForecast { get; init; }
    public string? DetailedForecast { get; init; }

    /// <summary>Temperature formatted like "72°F".</summary>
    public string TemperatureText => $"{Temperature}°{TemperatureUnit}";

    /// <summary>Wind formatted like "NW 10 mph" (null when not reported).</summary>
    public string? WindText =>
        string.IsNullOrWhiteSpace(WindSpeed) ? null : $"{WindDirection} {WindSpeed}".Trim();
}

/// <summary>A short forecast for a location: the place name and the upcoming periods.</summary>
public sealed record WeatherForecast
{
    /// <summary>Human-friendly place, e.g. "Denver, CO" (null if the service didn't supply one).</summary>
    public string? LocationName { get; init; }

    /// <summary>Upcoming forecast periods, soonest first.</summary>
    public required IReadOnlyList<WeatherPeriod> Periods { get; init; }
}

/// <summary>An active NWS watch/warning/advisory for the area (e.g. "Severe Thunderstorm Warning").</summary>
public sealed record WeatherAlert
{
    /// <summary>Stable NWS identifier, used to tell a new alert from one already shown.</summary>
    public string? Id { get; init; }
    public required string Event { get; init; }
    public string? Headline { get; init; }
    /// <summary>Extreme / Severe / Moderate / Minor / Unknown.</summary>
    public string? Severity { get; init; }
    public string? Urgency { get; init; }
    public string? AreaDesc { get; init; }
    public string? Description { get; init; }
    public string? Instruction { get; init; }
    public string? Expires { get; init; }

    /// <summary>A key to dedupe by, falling back to event+area when no id is supplied.</summary>
    public string Key => Id ?? $"{Event}|{AreaDesc}";

    /// <summary>Numeric severity for sorting/coloring — higher is worse.</summary>
    public int SeverityRank => Severity switch
    {
        "Extreme" => 4,
        "Severe" => 3,
        "Moderate" => 2,
        "Minor" => 1,
        _ => 0,
    };
}

/// <summary>Thrown when the weather service cannot be reached or returns an error status.</summary>
public sealed class WeatherUnavailableException : Exception
{
    public WeatherUnavailableException(string message) : base(message) { }
    public WeatherUnavailableException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Thrown when the weather service returns content that cannot be parsed.</summary>
public sealed class WeatherFormatException : Exception
{
    public WeatherFormatException(string message) : base(message) { }
    public WeatherFormatException(string message, Exception inner) : base(message, inner) { }
}

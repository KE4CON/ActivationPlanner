using ActivationPlanner.Services.Weather;

namespace ActivationPlanner.Services.Tests.Weather;

public sealed class WeatherJsonTests
{
    [Fact]
    public void Parses_forecast_url_and_place_from_a_points_response()
    {
        const string json = """
            {
              "properties": {
                "forecast": "https://api.weather.gov/gridpoints/BOU/62,61/forecast",
                "relativeLocation": { "properties": { "city": "Denver", "state": "CO" } }
              }
            }
            """;

        var (url, place) = WeatherJson.ParsePointReference(json);

        Assert.Equal("https://api.weather.gov/gridpoints/BOU/62,61/forecast", url);
        Assert.Equal("Denver, CO", place);
    }

    [Fact]
    public void Missing_forecast_url_throws_format_exception()
    {
        // A non-US point returns properties without a forecast URL.
        Assert.Throws<WeatherFormatException>(() =>
            WeatherJson.ParsePointReference("""{ "properties": { } }"""));
    }

    [Fact]
    public void Parses_periods_from_a_forecast_response()
    {
        const string json = """
            {
              "properties": {
                "periods": [
                  { "name": "This Afternoon", "isDaytime": true, "temperature": 72, "temperatureUnit": "F",
                    "windSpeed": "10 mph", "windDirection": "NW", "shortForecast": "Sunny",
                    "detailedForecast": "Sunny, with a high near 72." },
                  { "name": "Tonight", "isDaytime": false, "temperature": 48, "temperatureUnit": "F",
                    "windSpeed": "5 mph", "windDirection": "N", "shortForecast": "Clear" }
                ]
              }
            }
            """;

        var periods = WeatherJson.ParsePeriods(json);

        Assert.Equal(2, periods.Count);
        Assert.Equal("This Afternoon", periods[0].Name);
        Assert.True(periods[0].IsDaytime);
        Assert.Equal("72°F", periods[0].TemperatureText);
        Assert.Equal("NW 10 mph", periods[0].WindText);
        Assert.Equal("Sunny", periods[0].ShortForecast);
        Assert.False(periods[1].IsDaytime);
        Assert.Equal("48°F", periods[1].TemperatureText);
    }

    [Fact]
    public void Missing_periods_throws_format_exception()
    {
        Assert.Throws<WeatherFormatException>(() =>
            WeatherJson.ParsePeriods("""{ "properties": { } }"""));
    }

    [Fact]
    public void Parses_active_alerts_most_severe_first()
    {
        const string json = """
            {
              "features": [
                { "properties": { "event": "Flood Watch", "severity": "Moderate", "areaDesc": "Cheyenne Co.",
                                   "headline": "Flood Watch until 8 PM", "description": "Rain.", "instruction": "Avoid low areas.",
                                   "expires": "2026-08-14T20:00:00-06:00" } },
                { "properties": { "event": "Severe Thunderstorm Warning", "severity": "Severe", "areaDesc": "Kit Carson Co.",
                                   "headline": "SVR until 5 PM", "description": "Storms.", "instruction": "Take shelter." } }
              ]
            }
            """;

        var alerts = WeatherJson.ParseAlerts(json);

        Assert.Equal(2, alerts.Count);
        Assert.Equal("Severe Thunderstorm Warning", alerts[0].Event); // Severe sorts above Moderate
        Assert.Equal("Take shelter.", alerts[0].Instruction);
        Assert.Equal("Flood Watch", alerts[1].Event);
    }

    [Fact]
    public void No_features_means_no_alerts()
    {
        Assert.Empty(WeatherJson.ParseAlerts("""{ "features": [] }"""));
    }
}

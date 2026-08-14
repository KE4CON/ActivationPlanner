using System.Globalization;

namespace ActivationPlanner.Services.Weather;

/// <summary>
/// Fetches a short forecast for a location from the US National Weather Service (api.weather.gov) —
/// free, unauthenticated (a descriptive User-Agent is required by NWS). Two GETs: /points to find the
/// forecast URL and place name, then the forecast itself. US locations only; elsewhere the /points
/// call fails and surfaces as a clear message. JSON parsing is delegated to <see cref="WeatherJson"/>.
/// Layer-3 service.
/// </summary>
public sealed class WeatherClient
{
    private const string PointsUrl = "https://api.weather.gov/points/{0},{1}";

    // NWS asks callers to identify themselves with a contact; the project repo serves as that.
    private const string UserAgent = "ActivationPlanner/1.0 (https://github.com/KE4CON/ActivationPlanner)";

    private readonly HttpClient _http;

    public WeatherClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    /// <summary>Fetch the forecast for a latitude/longitude.</summary>
    /// <exception cref="WeatherUnavailableException">The service could not be reached / errored.</exception>
    /// <exception cref="WeatherFormatException">The service returned an unexpected shape.</exception>
    public async Task<WeatherForecast> GetForecastAsync(double latitude, double longitude, CancellationToken ct = default)
    {
        string pointsUrl = string.Format(
            CultureInfo.InvariantCulture, PointsUrl, Math.Round(latitude, 4), Math.Round(longitude, 4));

        string pointJson = await GetAsync(pointsUrl, ct).ConfigureAwait(false);
        (string forecastUrl, string? location) = WeatherJson.ParsePointReference(pointJson);

        string forecastJson = await GetAsync(forecastUrl, ct).ConfigureAwait(false);
        IReadOnlyList<WeatherPeriod> periods = WeatherJson.ParsePeriods(forecastJson);

        return new WeatherForecast { LocationName = location, Periods = periods };
    }

    /// <summary>Fetch active NWS watches/warnings/advisories for a latitude/longitude (empty if none).</summary>
    /// <exception cref="WeatherUnavailableException">The service could not be reached / errored.</exception>
    public async Task<IReadOnlyList<WeatherAlert>> GetAlertsAsync(double latitude, double longitude, CancellationToken ct = default)
    {
        string url = string.Format(
            CultureInfo.InvariantCulture,
            "https://api.weather.gov/alerts/active?point={0},{1}", Math.Round(latitude, 4), Math.Round(longitude, 4));

        string json = await GetAsync(url, ct).ConfigureAwait(false);
        return WeatherJson.ParseAlerts(json);
    }

    private async Task<string> GetAsync(string url, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd(UserAgent);
            request.Headers.Accept.ParseAdd("application/geo+json");
            using HttpResponseMessage response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new WeatherUnavailableException(
                    $"Weather service returned HTTP {(int)response.StatusCode}. (US locations only.)");
            return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }
        catch (WeatherUnavailableException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new WeatherUnavailableException("Could not reach the weather service. Check your network connection.", ex);
        }
    }
}

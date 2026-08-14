namespace ActivationPlanner.Services.SpaceWeather;

/// <summary>
/// Fetches current solar / space-weather indices from the public N0NBH (hamqsl.com) feed — the
/// community-standard source, plain unauthenticated HTTP. XML parsing is delegated to the pure
/// <see cref="SpaceWeatherXml"/> so it stays testable. Layer-3 service.
/// </summary>
public sealed class SpaceWeatherClient
{
    /// <summary>The N0NBH solar XML feed.</summary>
    public const string FeedUrl = "https://www.hamqsl.com/solarxml.php";

    private readonly HttpClient _http;

    public SpaceWeatherClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    /// <summary>Fetch the current solar indices.</summary>
    /// <exception cref="SpaceWeatherUnavailableException">The feed could not be reached.</exception>
    /// <exception cref="SpaceWeatherFormatException">The feed returned an unexpected shape.</exception>
    public async Task<SolarConditions> GetCurrentAsync(CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, FeedUrl);
            request.Headers.UserAgent.ParseAdd("ActivationPlanner/1.0");
            using HttpResponseMessage response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new SpaceWeatherUnavailableException($"Solar-data service returned HTTP {(int)response.StatusCode}.");

            string xml = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return SpaceWeatherXml.Parse(xml);
        }
        catch (SpaceWeatherUnavailableException)
        {
            throw;
        }
        catch (SpaceWeatherFormatException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SpaceWeatherUnavailableException("Could not reach the solar-data service. Check your network connection.", ex);
        }
    }
}

/// <summary>Thrown when the solar-data feed cannot be reached or returns an error status.</summary>
public sealed class SpaceWeatherUnavailableException : Exception
{
    public SpaceWeatherUnavailableException(string message) : base(message) { }
    public SpaceWeatherUnavailableException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>Thrown when the solar-data feed returns content that cannot be parsed.</summary>
public sealed class SpaceWeatherFormatException : Exception
{
    public SpaceWeatherFormatException(string message) : base(message) { }
    public SpaceWeatherFormatException(string message, Exception inner) : base(message, inner) { }
}

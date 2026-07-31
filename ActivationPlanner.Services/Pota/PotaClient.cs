namespace ActivationPlanner.Services.Pota;

/// <summary>
/// Read-only client for POTA's public, unauthenticated data (Item #6 (a)): current activator
/// spots and park details. Plain HTTP GETs — no login, no credentials. The auth-sensitive log
/// upload for award credit is a separate, out-of-scope operation and is not here; self-spotting
/// is a separate, deliberately-gated component (<see cref="PotaSelfSpotter"/>).
/// <para>Layer-3 service. Network I/O via BCL <see cref="HttpClient"/>; JSON parsing is delegated
/// to the pure <see cref="PotaJson"/> helpers so it stays testable.</para>
/// </summary>
public sealed class PotaClient
{
    /// <summary>Base address of the POTA public API.</summary>
    public const string BaseUrl = "https://api.pota.app";

    private readonly HttpClient _http;

    public PotaClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
    }

    /// <summary>Fetch all current activator spots.</summary>
    /// <exception cref="PotaUnavailableException">The feed could not be reached.</exception>
    /// <exception cref="PotaFormatException">The feed returned an unexpected shape.</exception>
    public async Task<IReadOnlyList<PotaSpot>> GetActivatorSpotsAsync(CancellationToken ct = default)
    {
        string json = await GetAsync($"{BaseUrl}/spot/activator", ct).ConfigureAwait(false);
        return PotaJson.ParseSpots(json);
    }

    /// <summary>Fetch details for a park reference (e.g. "US-4534"), or null if not found.</summary>
    /// <exception cref="PotaUnavailableException">The service could not be reached.</exception>
    public async Task<PotaPark?> GetParkAsync(string reference, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reference);

        HttpResponseMessage response;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}/park/{Uri.EscapeDataString(reference)}");
            request.Headers.UserAgent.ParseAdd("ActivationPlanner/1.0");
            response = await _http.SendAsync(request, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new PotaUnavailableException("Could not reach POTA. Check your network connection.", ex);
        }

        using (response)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
            if (!response.IsSuccessStatusCode)
                throw new PotaUnavailableException($"POTA returned HTTP {(int)response.StatusCode}.");

            string json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return PotaJson.ParsePark(json);
        }
    }

    private async Task<string> GetAsync(string url, CancellationToken ct)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd("ActivationPlanner/1.0");
            using HttpResponseMessage response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new PotaUnavailableException($"POTA returned HTTP {(int)response.StatusCode}.");
            return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }
        catch (PotaUnavailableException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new PotaUnavailableException("Could not reach POTA. Check your network connection.", ex);
        }
    }
}

/// <summary>Thrown when POTA's service cannot be reached or returns an error status.</summary>
public sealed class PotaUnavailableException : Exception
{
    public PotaUnavailableException(string message) : base(message) { }
    public PotaUnavailableException(string message, Exception inner) : base(message, inner) { }
}

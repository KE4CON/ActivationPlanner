using System.Globalization;
using System.Text.Json;
using ActivationPlanner.PropagationModel.Geo;

namespace ActivationPlanner.Services.Location;

/// <summary>
/// Resolves an approximate (city-level) location from the caller's public IP via a no-key
/// geo-IP HTTP service. Cross-platform and dependency-free (BCL <see cref="HttpClient"/> only).
/// The lookup runs only when explicitly requested — the caller's IP is sent to the geo-IP
/// service solely at that moment — and returns a coarse fix, which is sufficient for HF
/// propagation planning.
/// </summary>
public sealed class GeoIpLocationProvider : ILocationProvider
{
    /// <summary>Default no-key HTTPS endpoint returning JSON with latitude/longitude and place fields.</summary>
    public const string DefaultEndpoint = "https://ipapi.co/json/";

    private readonly HttpClient _http;
    private readonly string _endpoint;

    public GeoIpLocationProvider(HttpClient http, string? endpoint = null)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _endpoint = string.IsNullOrWhiteSpace(endpoint) ? DefaultEndpoint : endpoint;
    }

    /// <inheritdoc />
    public string SourceLabel => "Network (geo-IP)";

    /// <inheritdoc />
    public async Task<LocationFix> GetCurrentAsync(CancellationToken ct = default)
    {
        string json;
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, _endpoint);
            // Some geo-IP services reject requests without a User-Agent.
            request.Headers.UserAgent.ParseAdd("ActivationPlanner/1.0");

            using HttpResponseMessage response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new LocationUnavailableException(
                    $"Geo-IP lookup failed with HTTP {(int)response.StatusCode}.");

            json = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }
        catch (LocationUnavailableException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new LocationUnavailableException(
                "Could not reach the geo-IP service. Check your network connection.", ex);
        }

        return GeoIpLocation.Parse(json, SourceLabel);
    }
}

/// <summary>Pure parsing of a geo-IP JSON response into a <see cref="LocationFix"/> (kept separate so it is unit-testable).</summary>
public static class GeoIpLocation
{
    /// <summary>Parse a geo-IP JSON body. Handles ipapi.co-style fields.</summary>
    /// <exception cref="LocationUnavailableException">The body is missing coordinates or reports an error.</exception>
    public static LocationFix Parse(string json, string sourceLabel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new LocationUnavailableException("Geo-IP response was not valid JSON.", ex);
        }

        using (doc)
        {
            JsonElement root = doc.RootElement;

            // ipapi.co reports failures as {"error": true, "reason": "..."}.
            if (root.TryGetProperty("error", out JsonElement error)
                && error.ValueKind == JsonValueKind.True)
            {
                string reason = root.TryGetProperty("reason", out JsonElement r) ? r.GetString() ?? "" : "";
                throw new LocationUnavailableException($"Geo-IP service returned an error. {reason}".Trim());
            }

            if (!TryReadCoordinate(root, "latitude", out double lat)
                || !TryReadCoordinate(root, "longitude", out double lon))
            {
                throw new LocationUnavailableException("Geo-IP response did not include coordinates.");
            }

            GeoLocation location;
            try
            {
                location = new GeoLocation(lat, lon);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new LocationUnavailableException("Geo-IP coordinates were out of range.", ex);
            }

            return new LocationFix
            {
                Location = location,
                SourceLabel = sourceLabel,
                PlaceName = BuildPlaceName(root),
                IsApproximate = true,
            };
        }
    }

    private static bool TryReadCoordinate(JsonElement root, string name, out double value)
    {
        value = 0;
        if (!root.TryGetProperty(name, out JsonElement el))
            return false;

        return el.ValueKind switch
        {
            JsonValueKind.Number => el.TryGetDouble(out value),
            JsonValueKind.String => double.TryParse(
                el.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out value),
            _ => false,
        };
    }

    private static string? BuildPlaceName(JsonElement root)
    {
        string? city = ReadString(root, "city");
        string? region = ReadString(root, "region");
        string? country = ReadString(root, "country_name") ?? ReadString(root, "country");

        var parts = new[] { city, region, country }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();
        return parts.Length > 0 ? string.Join(", ", parts) : null;
    }

    private static string? ReadString(JsonElement root, string name) =>
        root.TryGetProperty(name, out JsonElement el) && el.ValueKind == JsonValueKind.String
            ? el.GetString()
            : null;
}

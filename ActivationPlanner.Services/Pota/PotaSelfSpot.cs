using System.Globalization;
using System.Text.Json;

namespace ActivationPlanner.Services.Pota;

/// <summary>
/// A self-spot the operator would post for their own activation. By definition the spotter is
/// the activator (POTA treats a self-spot as spotter == activator).
/// </summary>
public sealed record PotaSelfSpotRequest
{
    /// <summary>The activator's callsign (also the spotter for a self-spot).</summary>
    public required string Activator { get; init; }

    /// <summary>Park reference being activated, e.g. "US-4534".</summary>
    public required string Reference { get; init; }

    /// <summary>Operating frequency in kHz.</summary>
    public required double FrequencyKhz { get; init; }

    /// <summary>Operating mode, e.g. "SSB".</summary>
    public required string Mode { get; init; }

    /// <summary>Optional spotter comment.</summary>
    public string? Comments { get; init; }
}

/// <summary>
/// Self-spotting (Item #6 (b)) — <b>built but intentionally not enabled</b>. POTA's public spot
/// endpoint accepts a self-spot with no authentication, but CLAUDE.md requires direct confirmation
/// from POTA that third-party automated self-spotting is acceptable <i>before</i> this ships. Until
/// then <see cref="SubmitAsync"/> refuses to send. The request-body construction is fully
/// implemented and tested so that, once confirmation is obtained, enabling it is a one-line change
/// (and no UI currently wires this in).
/// </summary>
public sealed class PotaSelfSpotter
{
    /// <summary>POST target for a self-spot (spotter == activator).</summary>
    public const string SpotUrl = PotaClient.BaseUrl + "/spot/";

    /// <summary>Source tag included in the spot body.</summary>
    public const string Source = "Activation Planner";

    private readonly HttpClient _http;
    private readonly bool _enabled;

    /// <param name="http">HTTP client used only if/when self-spotting is enabled.</param>
    /// <param name="enabled">
    /// Must be explicitly true to permit sending. Defaults to false and stays false in the shipped
    /// app until POTA confirms third-party automated self-spotting is acceptable.
    /// </param>
    public PotaSelfSpotter(HttpClient http, bool enabled = false)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _enabled = enabled;
    }

    /// <summary>Whether sending is permitted. False in the shipped configuration.</summary>
    public bool IsEnabled => _enabled;

    /// <summary>
    /// Submit a self-spot — only when explicitly enabled. Throws
    /// <see cref="PotaSelfSpotDisabledException"/> otherwise; nothing is sent.
    /// </summary>
    public async Task SubmitAsync(PotaSelfSpotRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_enabled)
            throw new PotaSelfSpotDisabledException(
                "Self-spotting is disabled pending direct confirmation from POTA that third-party " +
                "automated self-spotting is acceptable.");

        string body = BuildBody(request);
        using var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        using var message = new HttpRequestMessage(HttpMethod.Post, SpotUrl) { Content = content };
        message.Headers.UserAgent.ParseAdd("ActivationPlanner/1.0");

        using HttpResponseMessage response = await _http.SendAsync(message, ct).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new PotaUnavailableException($"POTA self-spot POST returned HTTP {(int)response.StatusCode}.");
    }

    /// <summary>
    /// Build the JSON body for a self-spot. Pure and testable; the spotter is always set to the
    /// activator so the post is unambiguously a self-spot.
    /// </summary>
    public static string BuildBody(PotaSelfSpotRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Activator);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Reference);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Mode);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(request.FrequencyKhz);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("activator", request.Activator);
            writer.WriteString("spotter", request.Activator); // self-spot: spotter == activator
            writer.WriteString("frequency", request.FrequencyKhz.ToString("0.0", CultureInfo.InvariantCulture));
            writer.WriteString("reference", request.Reference);
            writer.WriteString("mode", request.Mode);
            writer.WriteString("source", Source);
            writer.WriteString("comments", request.Comments ?? string.Empty);
            writer.WriteEndObject();
        }
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }
}

/// <summary>Thrown when a self-spot submission is attempted while the feature is disabled.</summary>
public sealed class PotaSelfSpotDisabledException : Exception
{
    public PotaSelfSpotDisabledException(string message) : base(message) { }
}

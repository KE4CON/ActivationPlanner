using ActivationPlanner.PropagationModel.Bands;

namespace ActivationPlanner.Services.Pota;

/// <summary>
/// A current POTA activator spot from the public (unauthenticated) spot feed. Fields mirror the
/// api.pota.app response, normalized: frequency is exposed in both kHz and MHz, and mapped to a
/// <see cref="HamBand"/> so spots can be correlated with the band plan.
/// </summary>
public sealed record PotaSpot
{
    /// <summary>POTA spot id.</summary>
    public required long SpotId { get; init; }

    /// <summary>Activator callsign.</summary>
    public required string Activator { get; init; }

    /// <summary>Spot frequency in kHz (as POTA reports it).</summary>
    public required double FrequencyKhz { get; init; }

    /// <summary>Frequency in MHz.</summary>
    public double FrequencyMhz => FrequencyKhz / 1000.0;

    /// <summary>The HF band the spot falls in, or null for out-of-HF (e.g. VHF) frequencies.</summary>
    public HamBand? Band => HamBands.BandForFrequencyMhz(FrequencyMhz);

    /// <summary>Operating mode, e.g. "SSB", "CW", "FT8".</summary>
    public string? Mode { get; init; }

    /// <summary>Park reference, e.g. "US-4534".</summary>
    public required string Reference { get; init; }

    /// <summary>Park name, if the feed included it.</summary>
    public string? ParkName { get; init; }

    /// <summary>Callsign of whoever posted the spot (equals the activator for a self-spot).</summary>
    public string? Spotter { get; init; }

    /// <summary>Spotter comment.</summary>
    public string? Comments { get; init; }

    /// <summary>Spot time (UTC).</summary>
    public DateTime? SpotTimeUtc { get; init; }

    /// <summary>Location descriptor, e.g. "US-WY".</summary>
    public string? LocationDesc { get; init; }

    /// <summary>Maidenhead grid (6- or 4-character), if provided.</summary>
    public string? Grid { get; init; }

    /// <summary>Park latitude, if provided.</summary>
    public double? Latitude { get; init; }

    /// <summary>Park longitude, if provided.</summary>
    public double? Longitude { get; init; }

    /// <summary>True when the activator posted this spot themselves.</summary>
    public bool IsSelfSpot =>
        Spotter is not null && string.Equals(Spotter, Activator, StringComparison.OrdinalIgnoreCase);
}

/// <summary>Details for a single POTA park (reference), from the public park endpoint.</summary>
public sealed record PotaPark
{
    /// <summary>POTA numeric park id.</summary>
    public required int ParkId { get; init; }

    /// <summary>Park reference, e.g. "US-4534".</summary>
    public required string Reference { get; init; }

    /// <summary>Park name.</summary>
    public required string Name { get; init; }

    /// <summary>Latitude, if known.</summary>
    public double? Latitude { get; init; }

    /// <summary>Longitude, if known.</summary>
    public double? Longitude { get; init; }

    /// <summary>Maidenhead grid, if known.</summary>
    public string? Grid { get; init; }

    /// <summary>Park type description, e.g. "National Forest".</summary>
    public string? ParkType { get; init; }

    /// <summary>Whether the park is currently active for POTA.</summary>
    public bool Active { get; init; }

    /// <summary>Location descriptor, e.g. "US-WY".</summary>
    public string? LocationDesc { get; init; }

    /// <summary>Location name, e.g. "Wyoming".</summary>
    public string? LocationName { get; init; }

    /// <summary>DXCC entity name, e.g. "United States of America".</summary>
    public string? EntityName { get; init; }

    /// <summary>Official park website, if any.</summary>
    public string? Website { get; init; }

    /// <summary>Activation comments/notes, if any.</summary>
    public string? Comments { get; init; }
}

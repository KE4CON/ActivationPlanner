namespace ActivationPlanner.PropagationModel.Geo;

/// <summary>
/// A single, on-demand location reading — the operator's current position from whatever source
/// resolved it (network geo-IP today; a real GPS provider could plug in behind the same seam).
/// Refresh-on-demand only; the app never tracks continuously (CLAUDE.md location rule), so this
/// is a snapshot, not a stream.
/// </summary>
public sealed record LocationFix
{
    /// <summary>The resolved position.</summary>
    public required GeoLocation Location { get; init; }

    /// <summary>Where the fix came from, for display, e.g. "Network (geo-IP)".</summary>
    public required string SourceLabel { get; init; }

    /// <summary>Human-readable place, e.g. "Denver, Colorado, US", if the source provided one.</summary>
    public string? PlaceName { get; init; }

    /// <summary>
    /// True when the fix is coarse (e.g. city-level geo-IP) rather than precise. Fine for HF
    /// propagation planning, but surfaced so the operator knows to refine it if they need to.
    /// </summary>
    public bool IsApproximate { get; init; }
}

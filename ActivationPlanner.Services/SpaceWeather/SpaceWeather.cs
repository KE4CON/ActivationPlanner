namespace ActivationPlanner.Services.SpaceWeather;

/// <summary>
/// Current solar / space-weather indices. Feeds the planner's VOACAP sunspot input so predictions
/// use real conditions instead of a typed guess, and gives the operator quick band context.
/// </summary>
public sealed record SolarConditions
{
    /// <summary>Sunspot number (SSN) — the value VOACAP takes.</summary>
    public int? SunspotNumber { get; init; }

    /// <summary>10.7 cm solar flux index (SFI) — higher favors the high bands.</summary>
    public int? SolarFluxIndex { get; init; }

    /// <summary>A-index (daily geomagnetic activity).</summary>
    public int? AIndex { get; init; }

    /// <summary>K-index (near-real-time geomagnetic activity; high = disturbed/absorbing).</summary>
    public int? KIndex { get; init; }

    /// <summary>Feed's own "last updated" text, shown as-is.</summary>
    public string? UpdatedText { get; init; }

    /// <summary>True when a usable sunspot number was reported.</summary>
    public bool HasSunspotNumber => SunspotNumber is > 0;
}

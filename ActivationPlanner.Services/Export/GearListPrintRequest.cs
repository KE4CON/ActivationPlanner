using System.Collections.Generic;

namespace ActivationPlanner.Services.Export;

/// <summary>
/// A single line on a printed gear pack list — the item's name, its display group (Radio, Power,
/// Antennas, …) and whether it's flagged essential. Printed with a tick box for field use.
/// </summary>
public sealed record GearPrintItem
{
    public required string Name { get; init; }
    public required string Group { get; init; }
    public bool Essential { get; init; }
}

/// <summary>
/// Request to print the operator's tailored gear list. Carries only the items the operator has
/// actually selected on the Operation &amp; gear list screen (checked), grouped for a clean packing
/// sheet. Session-local working data — nothing here is persisted (stateless-replanning rule).
/// </summary>
public sealed record GearListPrintRequest
{
    /// <summary>Sheet title (e.g. "POTA kit — packing list").</summary>
    public required string Title { get; init; }

    /// <summary>Optional context line under the title (operation type, date).</summary>
    public string? Subtitle { get; init; }

    /// <summary>Optional one-line packing guidance for the operation.</summary>
    public string? PackingTip { get; init; }

    /// <summary>The selected items to print, in display order.</summary>
    public required IReadOnlyList<GearPrintItem> Items { get; init; }
}

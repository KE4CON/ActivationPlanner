using System.Collections.Generic;
using System.Linq;

namespace ActivationPlanner.PropagationModel.Gear;

/// <summary>
/// The operator's complete owned-gear inventory — the unit that is persisted and
/// re-loaded. Immutable snapshot; mutations elsewhere produce a new instance.
/// Non-antenna gear lives in <see cref="Items"/> (categorized by
/// <see cref="GearCategory"/>); antennas live in <see cref="Antennas"/> because
/// they carry a richer physical shape.
/// </summary>
public sealed record GearInventory
{
    /// <summary>Non-antenna gear (radios, power, digital interfaces, EMCOMM items).</summary>
    public IReadOnlyList<GearItem> Items { get; init; } = [];

    /// <summary>Owned antennas.</summary>
    public IReadOnlyList<AntennaProfile> Antennas { get; init; } = [];

    /// <summary>An empty inventory — the starting point before first-use setup.</summary>
    public static GearInventory Empty { get; } = new();

    /// <summary>True when the operator owns nothing at all (drives first-run wizard).</summary>
    public bool IsEmpty => Items.Count == 0 && Antennas.Count == 0;

    /// <summary>All non-antenna gear in a given category.</summary>
    public IEnumerable<GearItem> ItemsIn(GearCategory category) =>
        Items.Where(i => i.Category == category);
}

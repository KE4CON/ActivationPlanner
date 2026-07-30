namespace ActivationPlanner.PropagationModel.Gear;

/// <summary>
/// A single non-antenna piece of owned gear (radio, power source, digital
/// interface, EMCOMM item, …). Immutable — edits produce a new instance via
/// <c>with</c>. <see cref="Id"/> is stable across edits so a row can be updated
/// or removed by identity.
/// </summary>
public sealed record GearItem
{
    /// <summary>Stable identity, assigned once when the item is first created.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Which wizard/inventory category this item belongs to.</summary>
    public required GearCategory Category { get; init; }

    /// <summary>Operator-facing name (e.g. "IC-705", "Bioenno 6Ah LiFePO4").</summary>
    public required string Name { get; init; }

    /// <summary>Optional free-text notes.</summary>
    public string? Notes { get; init; }
}

namespace ActivationPlanner.PropagationModel.Gear;

/// <summary>
/// An owned antenna and the physical parameters needed to eventually model it
/// (Item #9). Immutable — edits produce a new instance via <c>with</c>.
/// <para>
/// Lengths are stored in <b>feet</b>. Wavelength-relative comparisons (the
/// Option A/B trigger math in Phase 3) convert these per evaluated band; nothing
/// in this record does propagation math — it only holds the operator's inputs.
/// </para>
/// </summary>
public sealed record AntennaProfile
{
    /// <summary>Stable identity, assigned once when the antenna is first created.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Operator-facing name (e.g. "Chelegance MC-750", "40m EFHW").</summary>
    public required string Name { get; init; }

    /// <summary>Physical antenna family.</summary>
    public required AntennaCategory Category { get; init; }

    /// <summary>Where/how the feedline connects — required for accurate modeling.</summary>
    public required FeedPointType FeedPoint { get; init; }

    /// <summary>Overall physical length of the radiator, in feet.</summary>
    public required double LengthFeet { get; init; }

    /// <summary>
    /// Height of the antenna above ground, in feet. For a vertical this is the
    /// base height; for a horizontal dipole it is the height of the wire.
    /// </summary>
    public required double HeightFeet { get; init; }

    /// <summary>
    /// Radial count — verticals only (operator-entered per Item #9).
    /// Null for antenna types where radials do not apply.
    /// </summary>
    public int? RadialCount { get; init; }

    /// <summary>Radial length in feet — verticals only. Null when not applicable.</summary>
    public double? RadialLengthFeet { get; init; }

    /// <summary>
    /// Height of the radials above ground, in feet — verticals only. Null (or 0) means the radials
    /// lie on the ground (the classic ground-mounted vertical). A positive value models an
    /// <b>elevated radial</b> / raised-counterpoise vertical: the radials — and the feed point where
    /// they meet the radiator — sit at this height, which lowers the take-off angle and cuts ground
    /// loss (why a few elevated radials rival dozens of on-ground ones). A few feet is typical.
    /// </summary>
    public double? RadialHeightFeet { get; init; }

    /// <summary>Optional free-text notes.</summary>
    public string? Notes { get; init; }

    /// <summary>
    /// True for families where radial count/length are meaningful operator inputs.
    /// Used by the UI to show/hide the radial fields.
    /// </summary>
    public bool UsesRadials => Category is AntennaCategory.Vertical or AntennaCategory.Whip;
}

namespace ActivationPlanner.PropagationModel.Missions;

/// <summary>
/// Standing metadata for a <see cref="MissionType"/>: how to present it, the default
/// propagation framing it implies, and a short note explaining that framing. Immutable
/// reference data — the built-in set lives in <see cref="MissionProfiles"/>.
/// </summary>
public sealed record MissionProfile
{
    /// <summary>The mission this profile describes.</summary>
    public required MissionType Type { get; init; }

    /// <summary>Operator-facing name, e.g. "POTA".</summary>
    public required string DisplayName { get; init; }

    /// <summary>One-line description of the mission.</summary>
    public required string Description { get; init; }

    /// <summary>Default propagation framing this mission implies (operator-overridable).</summary>
    public required PropagationFraming Framing { get; init; }

    /// <summary>Short explanation of what the framing means for the plan.</summary>
    public required string FramingNote { get; init; }
}

/// <summary>
/// The built-in catalog of <see cref="MissionProfile"/>s, one per <see cref="MissionType"/>.
/// </summary>
public static class MissionProfiles
{
    private const string DxNote =
        "DX / point-to-point framing: the prediction targets a distant receive point. " +
        "Enter the DX or target location on the planning screen.";

    private const string NvisNote =
        "Regional / NVIS framing: the receive point represents a nearby served agency or net " +
        "control station for short-to-medium-haul coverage. Enter a near-in target location.";

    /// <summary>All mission profiles, in the order they should be offered.</summary>
    public static IReadOnlyList<MissionProfile> All { get; } =
    [
        new()
        {
            Type = MissionType.Pota,
            DisplayName = "POTA",
            Description = "Parks On The Air — portable, hunter-facing operating.",
            Framing = PropagationFraming.DxPointToPoint,
            FramingNote = DxNote,
        },
        new()
        {
            Type = MissionType.Sota,
            DisplayName = "SOTA",
            Description = "Summits On The Air — weight-critical summit operating.",
            Framing = PropagationFraming.DxPointToPoint,
            FramingNote = DxNote,
        },
        new()
        {
            Type = MissionType.FieldDay,
            DisplayName = "Field Day",
            Description = "Multi-band, longer-duration operating with more support gear.",
            Framing = PropagationFraming.DxPointToPoint,
            FramingNote = DxNote,
        },
        new()
        {
            Type = MissionType.Emcomm,
            DisplayName = "EMCOMM",
            Description = "Emergency communications — regional nets and a load-bearing go-kit.",
            Framing = PropagationFraming.RegionalNvis,
            FramingNote = NvisNote,
        },
        new()
        {
            Type = MissionType.General,
            DisplayName = "General",
            Description = "Casual operating with no mission-specific emphasis.",
            Framing = PropagationFraming.DxPointToPoint,
            FramingNote = DxNote,
        },
    ];

    /// <summary>The profile for <paramref name="type"/>.</summary>
    public static MissionProfile For(MissionType type) =>
        All.First(p => p.Type == type);
}

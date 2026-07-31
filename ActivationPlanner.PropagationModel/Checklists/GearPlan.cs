using System.Collections.Generic;
using System.Linq;
using ActivationPlanner.PropagationModel.Missions;

namespace ActivationPlanner.PropagationModel.Checklists;

/// <summary>Where a gear-plan entry comes from.</summary>
public enum GearPlanSource
{
    /// <summary>A specific item the operator actually owns (a real radio, antenna, battery, …).</summary>
    OwnedGear,

    /// <summary>A personal item to remember that isn't tracked in gear inventory (logs, first aid, ID).</summary>
    Reminder,

    /// <summary>The mission typically needs this but the operator owns nothing for it — secondary.</summary>
    Acquire,
}

/// <summary>
/// One line of a tailored gear plan: a named item, the group it displays under, and whether it is
/// owned gear, a personal reminder, or a "consider acquiring" gap. Working data — check-off and
/// edits live in the UI (stateless-replanning rule).
/// </summary>
public sealed record GearPlanEntry
{
    /// <summary>Item text — a real owned item's name, a reminder, or an acquire role.</summary>
    public required string Name { get; init; }

    /// <summary>Display group heading (e.g. "Radio", "Antennas", "Power", "Safety").</summary>
    public required string Group { get; init; }

    /// <summary>Owned gear / reminder / acquire.</summary>
    public required GearPlanSource Source { get; init; }

    /// <summary>True when essential for the mission.</summary>
    public bool Essential { get; init; }
}

/// <summary>
/// A packing plan tailored to a mission, built from the operator's owned inventory plus the
/// mission's template: the actual owned items to pack, personal reminders, and a clearly separated
/// "consider acquiring" list for mission needs the operator owns nothing for (CLAUDE.md gear rule).
/// A starting point the operator then fine-tunes; session-local, not persisted.
/// </summary>
public sealed record GearPlan
{
    /// <summary>Mission this plan was built for.</summary>
    public required MissionType MissionType { get; init; }

    /// <summary>Underlying template name (e.g. "SOTA kit").</summary>
    public required string TemplateName { get; init; }

    /// <summary>One-line packing guidance for this mission.</summary>
    public required string PackingTip { get; init; }

    /// <summary>All entries, in display order.</summary>
    public required IReadOnlyList<GearPlanEntry> Entries { get; init; }

    /// <summary>Owned gear + reminders — the things to physically pack.</summary>
    public IEnumerable<GearPlanEntry> Pack =>
        Entries.Where(e => e.Source is GearPlanSource.OwnedGear or GearPlanSource.Reminder);

    /// <summary>Secondary "consider acquiring" gaps.</summary>
    public IEnumerable<GearPlanEntry> Acquire =>
        Entries.Where(e => e.Source == GearPlanSource.Acquire);
}

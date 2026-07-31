using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.PropagationModel.Missions;

namespace ActivationPlanner.PropagationModel.Checklists;

/// <summary>
/// Checklist grouping, scaled down from FieldCommand IMS's preflight categories to one
/// operator's personal go-kit (Item #2).
/// </summary>
public enum ChecklistCategory
{
    /// <summary>Batteries, chargers, solar, DC cabling, fuses.</summary>
    Power,

    /// <summary>Radio-support gear beyond the primary rig: spare antenna, coax, mic, digital interface.</summary>
    Communications,

    /// <summary>Documentation &amp; traffic handling: logs, ICS/NTS forms, notepad.</summary>
    Documentation,

    /// <summary>Personal safety &amp; comfort: first aid, water, weather protection, light.</summary>
    Safety,

    /// <summary>Identification &amp; coordination: license copy, ID, emergency contacts.</summary>
    Identification,
}

/// <summary>
/// How a checklist item relates to the operator's owned inventory. Keeps the CLAUDE.md rule
/// crisp: "pack this" (owned or a personal-item reminder) is never mixed with "consider
/// acquiring" (a mission need the operator doesn't own).
/// </summary>
public enum ChecklistItemStatus
{
    /// <summary>Matched to owned inventory — pack it.</summary>
    Owned,

    /// <summary>A personal item not tracked in gear inventory (first aid, license copy) — a pack reminder.</summary>
    Reminder,

    /// <summary>The mission typically needs this but it isn't owned — a secondary "consider acquiring" note.</summary>
    Acquire,
}

/// <summary>
/// One line in a checklist <see cref="ChecklistTemplate"/>: what it is, whether it's essential,
/// and how it maps to inventory so the <c>ChecklistService</c> can classify it owned vs.
/// consider-acquiring. Items with no gear mapping are personal-item reminders.
/// </summary>
public sealed record ChecklistTemplateItem
{
    /// <summary>Which checklist category this item belongs to.</summary>
    public required ChecklistCategory Category { get; init; }

    /// <summary>Operator-facing item text, e.g. "Spare battery".</summary>
    public required string Name { get; init; }

    /// <summary>True when the item is essential for this mission (drives a pre-finalize warning).</summary>
    public bool Essential { get; init; }

    /// <summary>
    /// Inventory gear category this item is satisfied by, if any. When set, ownership is
    /// decided by whether the operator has any gear in this category.
    /// </summary>
    public GearCategory? GearCategory { get; init; }

    /// <summary>True when the item is satisfied by owning any antenna (e.g. "backup antenna").</summary>
    public bool IsAntenna { get; init; }

    /// <summary>True when this item maps to trackable inventory (a gear category or an antenna).</summary>
    public bool MapsToInventory => GearCategory is not null || IsAntenna;
}

/// <summary>
/// A standing, reusable loadout for a mission type (Reference §7): the starting point a
/// specific activation's <see cref="ChecklistInstance"/> is built from. Immutable reference
/// data — the built-in set lives in <see cref="ChecklistTemplates"/>.
/// </summary>
public sealed record ChecklistTemplate
{
    /// <summary>The mission this template serves.</summary>
    public required MissionType MissionType { get; init; }

    /// <summary>Template name, e.g. "POTA kit".</summary>
    public required string Name { get; init; }

    /// <summary>The template's items, in display order.</summary>
    public required IReadOnlyList<ChecklistTemplateItem> Items { get; init; }
}

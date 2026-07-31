using System.Collections.Generic;
using System.Linq;
using ActivationPlanner.PropagationModel.Missions;

namespace ActivationPlanner.PropagationModel.Checklists;

/// <summary>
/// One item in a built <see cref="ChecklistInstance"/>: a template item resolved against the
/// operator's inventory into an owned / reminder / acquire <see cref="ChecklistItemStatus"/>.
/// Check-off state is session-local and lives in the UI, not here (stateless-replanning rule).
/// </summary>
public sealed record ChecklistInstanceItem
{
    /// <summary>Checklist category.</summary>
    public required ChecklistCategory Category { get; init; }

    /// <summary>Item text.</summary>
    public required string Name { get; init; }

    /// <summary>True when essential for the mission.</summary>
    public required bool Essential { get; init; }

    /// <summary>Owned / reminder (pack this) vs. acquire (secondary).</summary>
    public required ChecklistItemStatus Status { get; init; }

    /// <summary>True for items the operator should physically pack (owned gear or a reminder).</summary>
    public bool IsPackable => Status is ChecklistItemStatus.Owned or ChecklistItemStatus.Reminder;
}

/// <summary>
/// A specific activation's checklist, built from a mission template and the operator's owned
/// inventory. "Pack this" (owned + reminders) and "consider acquiring" are kept strictly
/// separate (CLAUDE.md gear rule). Session-local working data; not persisted.
/// </summary>
public sealed record ChecklistInstance
{
    /// <summary>The mission this instance was built for.</summary>
    public required MissionType MissionType { get; init; }

    /// <summary>The template it started from.</summary>
    public required string TemplateName { get; init; }

    /// <summary>All resolved items, in template order.</summary>
    public required IReadOnlyList<ChecklistInstanceItem> Items { get; init; }

    /// <summary>Items to physically pack (owned gear + personal reminders).</summary>
    public IEnumerable<ChecklistInstanceItem> PackItems => Items.Where(i => i.IsPackable);

    /// <summary>Secondary "consider acquiring" items — mission needs the operator doesn't own.</summary>
    public IEnumerable<ChecklistInstanceItem> AcquireItems =>
        Items.Where(i => i.Status == ChecklistItemStatus.Acquire);

    /// <summary>Count of essential items that must be packed.</summary>
    public int EssentialPackCount => PackItems.Count(i => i.Essential);
}

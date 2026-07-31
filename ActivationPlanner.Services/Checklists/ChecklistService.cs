using ActivationPlanner.PropagationModel.Checklists;
using ActivationPlanner.PropagationModel.Missions;
using Inventory = ActivationPlanner.PropagationModel.Gear.GearInventory;

namespace ActivationPlanner.Services.Checklists;

/// <summary>
/// Template-vs-instance checklist logic (Reference §7). Builds a specific activation's
/// <see cref="ChecklistInstance"/> from a mission's standing <see cref="ChecklistTemplate"/>,
/// resolving each item against the operator's owned inventory: items backed by owned gear
/// become "pack this", the same items when not owned become secondary "consider acquiring"
/// notes, and personal items not tracked in inventory are pack reminders. The two tiers are
/// never mixed (CLAUDE.md gear-suggestion rule).
/// <para>Layer-3 service: consumes PropagationModel only. No UI, no propagation math.</para>
/// </summary>
public sealed class ChecklistService
{
    /// <summary>
    /// Build the checklist for <paramref name="missionType"/>, classifying each template item
    /// against <paramref name="inventory"/>. Item order matches the template.
    /// </summary>
    public ChecklistInstance Build(MissionType missionType, Inventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        ChecklistTemplate template = ChecklistTemplates.For(missionType);

        var items = template.Items
            .Select(t => new ChecklistInstanceItem
            {
                Category = t.Category,
                Name = t.Name,
                Essential = t.Essential,
                Status = Classify(t, inventory),
            })
            .ToList();

        return new ChecklistInstance
        {
            MissionType = missionType,
            TemplateName = template.Name,
            Items = items,
        };
    }

    // Owned-first, acquire-second. Items with no inventory mapping are personal-item reminders.
    private static ChecklistItemStatus Classify(ChecklistTemplateItem item, Inventory inventory)
    {
        if (item.IsAntenna)
            return inventory.Antennas.Count > 0 ? ChecklistItemStatus.Owned : ChecklistItemStatus.Acquire;

        if (item.GearCategory is { } category)
            return inventory.ItemsIn(category).Any() ? ChecklistItemStatus.Owned : ChecklistItemStatus.Acquire;

        return ChecklistItemStatus.Reminder;
    }
}

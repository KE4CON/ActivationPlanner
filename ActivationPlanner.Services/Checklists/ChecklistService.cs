using ActivationPlanner.PropagationModel.Checklists;
using ActivationPlanner.PropagationModel.Gear;
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

    // ---- Tailored gear plan (specific owned items, not just category presence) ----

    // Owned-gear display order: the main rig and antenna first, then support gear.
    private static readonly GearCategory[] GearOrder =
        [GearCategory.Radio, GearCategory.Power, GearCategory.DigitalInterface,
         GearCategory.Computer, GearCategory.Emcomm, GearCategory.Other];

    private static readonly IReadOnlyDictionary<MissionType, string> PackingTips =
        new Dictionary<MissionType, string>
        {
            [MissionType.Pota] = "Balanced portable kit — a quick-deploy antenna, a mid-size battery, and a logging device.",
            [MissionType.Sota] = "Pack light — QRP radio, a wire antenna, a small battery. Leave generators and heavy power at home.",
            [MissionType.FieldDay] = "Higher capacity — 100 W radio, a generator or large power station, and a logging computer.",
            [MissionType.Emcomm] = "Resilience — NVIS/regional antenna, a digital/TNC interface, redundant power, and a rugged computer.",
            [MissionType.General] = "Your full kit — trim it to whatever this outing actually needs.",
        };

    /// <summary>
    /// Build a tailored packing plan for <paramref name="missionType"/> from the operator's owned
    /// <paramref name="inventory"/>: the actual owned items to pack (by name), the mission's personal
    /// reminders, and a separate "consider acquiring" list for mission needs nothing is owned for.
    /// A starting point the operator edits — the UI adds check-off / add / remove on top.
    /// </summary>
    public GearPlan BuildGearPlan(MissionType missionType, Inventory inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        ChecklistTemplate template = ChecklistTemplates.For(missionType);

        var entries = new List<GearPlanEntry>();

        // 1) Owned gear — the real items. Radios first, then antennas, then support gear.
        foreach (GearItem radio in inventory.ItemsIn(GearCategory.Radio))
            entries.Add(OwnedEntry(radio.Name, GroupLabel(GearCategory.Radio)));

        foreach (AntennaProfile antenna in inventory.Antennas)
            entries.Add(OwnedEntry(antenna.Name, "Antennas"));

        foreach (GearCategory category in GearOrder.Where(c => c != GearCategory.Radio))
            foreach (GearItem item in inventory.ItemsIn(category))
                entries.Add(OwnedEntry(item.Name, GroupLabel(category)));

        // 2) Personal reminders — template items that don't map to trackable inventory.
        foreach (ChecklistTemplateItem t in template.Items.Where(t => !t.MapsToInventory))
            entries.Add(new GearPlanEntry
            {
                Name = t.Name,
                Group = t.Category.ToString(),
                Source = GearPlanSource.Reminder,
                Essential = t.Essential,
            });

        // 3) Consider acquiring — mission roles the operator owns nothing for (deduped by name).
        var seen = new HashSet<string>();
        foreach (ChecklistTemplateItem t in template.Items.Where(t => t.MapsToInventory))
        {
            bool owned = t.IsAntenna
                ? inventory.Antennas.Count > 0
                : t.GearCategory is { } c && inventory.ItemsIn(c).Any();
            if (owned || !seen.Add(t.Name))
                continue;

            entries.Add(new GearPlanEntry
            {
                Name = t.Name,
                Group = t.IsAntenna ? "Antennas" : GroupLabel(t.GearCategory!.Value),
                Source = GearPlanSource.Acquire,
                Essential = t.Essential,
            });
        }

        return new GearPlan
        {
            MissionType = missionType,
            TemplateName = template.Name,
            PackingTip = PackingTips[missionType],
            Entries = entries,
        };
    }

    private static GearPlanEntry OwnedEntry(string name, string group) =>
        new() { Name = name, Group = group, Source = GearPlanSource.OwnedGear };

    private static string GroupLabel(GearCategory category) => category switch
    {
        GearCategory.Radio => "Radio",
        GearCategory.Power => "Power",
        GearCategory.DigitalInterface => "Digital Interface",
        GearCategory.Computer => "Computer",
        GearCategory.Emcomm => "EMCOMM",
        _ => "Accessories",
    };
}

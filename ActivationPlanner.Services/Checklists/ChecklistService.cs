using System.Text.RegularExpressions;
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

        // "Specialty" categories this operation calls for beyond the shared base kit that every
        // operation (including General) carries. Owned gear in these is flagged as suggested — e.g.
        // a logging computer or a digital interface for Field Day, EMCOMM gear for EMCOMM.
        var specialty = TemplateCategories(missionType);
        specialty.ExceptWith(TemplateCategories(MissionType.General));

        // 1) Owned gear — everything the operator owns, nothing hidden. Items well suited to the
        //    operation are flagged Recommended (a "Suggested" badge) and sorted first in their group;
        //    the list stays fully editable, so the operator trims or adds from this starting point.
        //    Radios: QRP for SOTA, 100 W for Field Day; other operations don't discriminate on power.
        foreach (GearItem radio in inventory.ItemsIn(GearCategory.Radio)
                     .OrderByDescending(r => RadioSuitsMission(missionType, r)))
            entries.Add(OwnedEntry(radio.Name, GroupLabel(GearCategory.Radio),
                                   RadioSuitsMission(missionType, radio)));

        foreach (AntennaProfile antenna in inventory.Antennas
                     .OrderByDescending(a => AntennaSuitsMission(missionType, a)))
            entries.Add(OwnedEntry(antenna.Name, "Antennas", AntennaSuitsMission(missionType, antenna)));

        foreach (GearCategory category in GearOrder.Where(c => c != GearCategory.Radio))
            foreach (GearItem item in inventory.ItemsIn(category))
                entries.Add(OwnedEntry(item.Name, GroupLabel(category), specialty.Contains(category)));

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

    private static GearPlanEntry OwnedEntry(string name, string group, bool recommended = false) =>
        new() { Name = name, Group = group, Source = GearPlanSource.OwnedGear, Recommended = recommended };

    // ---- radio power-class matching (drives per-mission radio suggestions) ----

    // QRP is conventionally ≤ ~10 W (covers the IC-705's 10 W); Field Day wants the 100 W class.
    private const int QrpWattsCeiling = 10;
    private const int FieldDayWattsFloor = 50;

    // Radios came from the preset catalog, so their notes reliably carry the rating ("100 W", "5 W",
    // "QRP"). Parse the first wattage figure; fall back to a "QRP" mention when no number is present.
    private static readonly Regex WattsPattern = new(@"(\d+)\s*[wW]\b", RegexOptions.Compiled);

    private static int? ExtractWatts(GearItem radio)
    {
        if (radio.Notes is not { } notes)
            return null;
        Match m = WattsPattern.Match(notes);
        return m.Success && int.TryParse(m.Groups[1].Value, out int w) ? w : null;
    }

    private static bool RadioSuitsMission(MissionType mission, GearItem radio)
    {
        int? watts = ExtractWatts(radio);
        bool qrp = watts is <= QrpWattsCeiling
                   || (watts is null && radio.Notes?.Contains("QRP", StringComparison.OrdinalIgnoreCase) == true);
        bool fullPower = watts is >= FieldDayWattsFloor;

        return mission switch
        {
            MissionType.Sota => qrp,          // pack light — QRP only
            MissionType.FieldDay => fullPower, // higher capacity — 100 W class
            _ => false,                        // POTA / EMCOMM / General don't discriminate on power
        };
    }

    // Antenna suitability by operation, driven by physical family (Item #9 category). Weight and
    // per-band NVIS height aren't modeled here, so this is a family-level heuristic — a starting
    // suggestion the operator edits, never a hard filter (nothing is hidden).
    //   SOTA / POTA : light, quick-deploy portable — whips, end-fed wires, verticals.
    //   Field Day   : full-size wire performers — dipoles and end-fed half-waves.
    //   EMCOMM      : regional / NVIS high-angle — the NVIS crossed dipole and (low) dipoles.
    private static bool AntennaSuitsMission(MissionType mission, AntennaProfile antenna) =>
        mission switch
        {
            MissionType.Sota or MissionType.Pota =>
                antenna.Category is AntennaCategory.Whip
                    or AntennaCategory.EndFedHalfWave
                    or AntennaCategory.Vertical,
            MissionType.FieldDay =>
                antenna.Category is AntennaCategory.Dipole or AntennaCategory.EndFedHalfWave,
            MissionType.Emcomm =>
                antenna.Category is AntennaCategory.NvisCrossedDipole or AntennaCategory.Dipole,
            _ => false, // General — no operation-specific emphasis
        };

    private static HashSet<GearCategory> TemplateCategories(MissionType mission) =>
        ChecklistTemplates.For(mission).Items
            .Where(t => t.GearCategory is not null)
            .Select(t => t.GearCategory!.Value)
            .ToHashSet();

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

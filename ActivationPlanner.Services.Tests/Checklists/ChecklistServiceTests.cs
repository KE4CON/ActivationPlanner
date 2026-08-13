using System.Linq;
using ActivationPlanner.PropagationModel.Checklists;
using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.PropagationModel.Missions;
using ActivationPlanner.Services.Checklists;
using Xunit;
using Inventory = ActivationPlanner.PropagationModel.Gear.GearInventory;

namespace ActivationPlanner.Services.Tests.Checklists;

public sealed class ChecklistServiceTests
{
    private static GearItem Item(GearCategory category, string name) =>
        new() { Category = category, Name = name };

    private static AntennaProfile Antenna() => new()
    {
        Name = "40m EFHW", Category = AntennaCategory.EndFedHalfWave, FeedPoint = FeedPointType.EndFedHalfWave,
        LengthFeet = 66, HeightFeet = 30,
    };

    [Fact]
    public void Owned_gear_category_becomes_pack_this()
    {
        var inventory = new Inventory { Items = [Item(GearCategory.Power, "Bioenno 6Ah")] };

        var checklist = new ChecklistService().Build(MissionType.Pota, inventory);

        var spareBattery = checklist.Items.Single(i => i.Name == "Spare battery");
        Assert.Equal(ChecklistItemStatus.Owned, spareBattery.Status);
        Assert.True(spareBattery.IsPackable);
        Assert.Contains(spareBattery, checklist.PackItems);
        Assert.DoesNotContain(spareBattery, checklist.AcquireItems);
    }

    [Fact]
    public void Unowned_gear_category_becomes_consider_acquiring()
    {
        // Empty inventory: the spare battery maps to Power gear the operator doesn't own.
        var checklist = new ChecklistService().Build(MissionType.Pota, Inventory.Empty);

        var spareBattery = checklist.Items.Single(i => i.Name == "Spare battery");
        Assert.Equal(ChecklistItemStatus.Acquire, spareBattery.Status);
        Assert.False(spareBattery.IsPackable);
        Assert.Contains(spareBattery, checklist.AcquireItems);
        Assert.DoesNotContain(spareBattery, checklist.PackItems);
    }

    [Fact]
    public void Antenna_backed_item_tracks_owned_antennas()
    {
        var withAntenna = new ChecklistService().Build(
            MissionType.Pota, new Inventory { Antennas = [Antenna()] });
        var without = new ChecklistService().Build(MissionType.Pota, Inventory.Empty);

        Assert.Equal(ChecklistItemStatus.Owned,
            withAntenna.Items.Single(i => i.Name == "Backup / spare antenna").Status);
        Assert.Equal(ChecklistItemStatus.Acquire,
            without.Items.Single(i => i.Name == "Backup / spare antenna").Status);
    }

    [Fact]
    public void Personal_items_without_inventory_mapping_are_reminders()
    {
        // First aid kit isn't tracked in gear inventory — always a pack reminder, never "acquire".
        var checklist = new ChecklistService().Build(MissionType.Emcomm, Inventory.Empty);

        var firstAid = checklist.Items.Single(i => i.Name == "First aid kit");
        Assert.Equal(ChecklistItemStatus.Reminder, firstAid.Status);
        Assert.True(firstAid.IsPackable);
        Assert.Contains(firstAid, checklist.PackItems);
    }

    [Fact]
    public void Pack_and_acquire_tiers_never_overlap()
    {
        var inventory = new Inventory { Items = [Item(GearCategory.Power, "Battery")] };
        var checklist = new ChecklistService().Build(MissionType.FieldDay, inventory);

        Assert.Empty(checklist.PackItems.Intersect(checklist.AcquireItems));
        Assert.All(checklist.PackItems, i => Assert.NotEqual(ChecklistItemStatus.Acquire, i.Status));
        Assert.All(checklist.AcquireItems, i => Assert.False(i.IsPackable));
    }

    [Fact]
    public void Emcomm_template_marks_ics_forms_essential()
    {
        var checklist = new ChecklistService().Build(MissionType.Emcomm, Inventory.Empty);

        var icsForms = checklist.Items.Single(i => i.Name == "ICS-213 / 214 forms");
        Assert.True(icsForms.Essential);
    }

    [Fact]
    public void Each_mission_selects_its_own_template()
    {
        var svc = new ChecklistService();

        Assert.Equal("POTA kit", svc.Build(MissionType.Pota, Inventory.Empty).TemplateName);
        Assert.Equal("EMCOMM go-kit", svc.Build(MissionType.Emcomm, Inventory.Empty).TemplateName);

        // EMCOMM adds a digital-interface requirement the general kit doesn't have.
        var general = svc.Build(MissionType.General, Inventory.Empty);
        Assert.DoesNotContain(general.Items, i => i.Name.Contains("Digital-mode interface"));
    }

    [Fact]
    public void Build_rejects_null_inventory()
    {
        Assert.Throws<System.ArgumentNullException>(
            () => new ChecklistService().Build(MissionType.Pota, null!));
    }

    // ---- BuildGearPlan (tailored, specific-item plan) ----

    [Fact]
    public void Gear_plan_lists_the_operators_actual_owned_items_by_name()
    {
        var inventory = new Inventory
        {
            Items = [Item(GearCategory.Radio, "Icom IC-705"), Item(GearCategory.Power, "Bioenno BLF-1220A")],
            Antennas = [Antenna()],
        };

        var plan = new ChecklistService().BuildGearPlan(MissionType.Pota, inventory);

        Assert.Contains(plan.Pack, e => e.Name == "Icom IC-705" && e.Group == "Radio" && e.Source == GearPlanSource.OwnedGear);
        Assert.Contains(plan.Pack, e => e.Name == "Bioenno BLF-1220A" && e.Group == "Power");
        Assert.Contains(plan.Pack, e => e.Name == "40m EFHW" && e.Group == "Antennas");
    }

    [Fact]
    public void Gear_plan_keeps_personal_reminders_in_the_pack_list()
    {
        var plan = new ChecklistService().BuildGearPlan(MissionType.Pota, Inventory.Empty);
        Assert.Contains(plan.Pack, e => e.Name == "First aid kit" && e.Source == GearPlanSource.Reminder);
    }

    [Fact]
    public void Gear_plan_flags_unowned_mission_needs_as_acquire_only()
    {
        // EMCOMM needs a digital interface; empty inventory -> it must be an acquire gap, never packed.
        var plan = new ChecklistService().BuildGearPlan(MissionType.Emcomm, Inventory.Empty);

        Assert.Contains(plan.Acquire, e => e.Name.Contains("Digital-mode interface"));
        Assert.DoesNotContain(plan.Pack, e => e.Name.Contains("Digital-mode interface"));
        Assert.Empty(plan.Pack.Intersect(plan.Acquire));
    }

    [Fact]
    public void Gear_plan_does_not_duplicate_an_owned_role_into_acquire()
    {
        // Owning a digital interface means it shows as owned gear, not as an acquire gap.
        var inventory = new Inventory { Items = [Item(GearCategory.DigitalInterface, "Digirig Mobile")] };
        var plan = new ChecklistService().BuildGearPlan(MissionType.Emcomm, inventory);

        Assert.Contains(plan.Pack, e => e.Name == "Digirig Mobile" && e.Group == "Digital Interface");
        Assert.DoesNotContain(plan.Acquire, e => e.Name.Contains("Digital-mode interface"));
    }

    private static GearItem Radio(string name, string notes) =>
        new() { Category = GearCategory.Radio, Name = name, Notes = notes };

    [Fact]
    public void Sota_suggests_qrp_radios_field_day_suggests_100w_nothing_hidden()
    {
        var inventory = new Inventory
        {
            Items =
            [
                Radio("Icom IC-705", "HF / VHF / UHF, 10 W. QRP portable."),
                Radio("Yaesu FT-891", "HF + 6 m, 100 W."),
            ],
        };
        var svc = new ChecklistService();

        var sota = svc.BuildGearPlan(MissionType.Sota, inventory);
        // Both radios still present (nothing hidden), but only the QRP one is suggested.
        Assert.Contains(sota.Pack, e => e.Name == "Icom IC-705");
        Assert.Contains(sota.Pack, e => e.Name == "Yaesu FT-891");
        Assert.True(sota.Pack.Single(e => e.Name == "Icom IC-705").Recommended);
        Assert.False(sota.Pack.Single(e => e.Name == "Yaesu FT-891").Recommended);

        var fieldDay = svc.BuildGearPlan(MissionType.FieldDay, inventory);
        Assert.False(fieldDay.Pack.Single(e => e.Name == "Icom IC-705").Recommended);
        Assert.True(fieldDay.Pack.Single(e => e.Name == "Yaesu FT-891").Recommended);
    }

    [Fact]
    public void Suggested_radios_sort_ahead_of_the_rest()
    {
        // 100 W radio listed first in inventory, but SOTA should surface the QRP one first.
        var inventory = new Inventory
        {
            Items =
            [
                Radio("Yaesu FT-891", "100 W."),
                Radio("QRP Labs QMX+", "5 W multi-band kit."),
            ],
        };

        var radios = new ChecklistService().BuildGearPlan(MissionType.Sota, inventory)
            .Pack.Where(e => e.Group == "Radio").ToList();

        Assert.Equal("QRP Labs QMX+", radios[0].Name);
    }

    [Fact]
    public void Computer_and_interface_are_suggested_for_field_day_but_not_hidden_on_sota()
    {
        var inventory = new Inventory
        {
            Items = [Item(GearCategory.Computer, "Panasonic Toughbook")],
        };
        var svc = new ChecklistService();

        var fieldDay = svc.BuildGearPlan(MissionType.FieldDay, inventory);
        Assert.True(fieldDay.Pack.Single(e => e.Name == "Panasonic Toughbook").Recommended);

        var sota = svc.BuildGearPlan(MissionType.Sota, inventory);
        // Still packed (nothing hidden), just not suggested for a light SOTA hike.
        Assert.Contains(sota.Pack, e => e.Name == "Panasonic Toughbook");
        Assert.False(sota.Pack.Single(e => e.Name == "Panasonic Toughbook").Recommended);
    }

    [Fact]
    public void Owned_gear_is_never_dropped_when_switching_operations()
    {
        var inventory = new Inventory
        {
            Items =
            [
                Radio("Icom IC-705", "10 W QRP."),
                Item(GearCategory.Power, "Bioenno 30Ah"),
                Item(GearCategory.Computer, "Toughbook"),
                Item(GearCategory.Emcomm, "Go-kit binder"),
            ],
            Antennas = [Antenna()],
        };
        var svc = new ChecklistService();

        foreach (var mission in new[] { MissionType.Pota, MissionType.Sota, MissionType.FieldDay, MissionType.Emcomm, MissionType.General })
        {
            var packed = svc.BuildGearPlan(mission, inventory).Pack.Select(e => e.Name).ToHashSet();
            Assert.Contains("Icom IC-705", packed);
            Assert.Contains("Bioenno 30Ah", packed);
            Assert.Contains("Toughbook", packed);
            Assert.Contains("Go-kit binder", packed);
            Assert.Contains("40m EFHW", packed);
        }
    }

    private static AntennaProfile Ant(string name, AntennaCategory category, FeedPointType feed) => new()
    {
        Name = name, Category = category, FeedPoint = feed, LengthFeet = 30, HeightFeet = 15,
    };

    [Fact]
    public void Emcomm_suggests_the_nvis_antenna_sota_does_not_nothing_hidden()
    {
        var inventory = new Inventory
        {
            Antennas =
            [
                Ant("Chameleon NVIS (4-wire)", AntennaCategory.NvisCrossedDipole, FeedPointType.CenterFed),
                Ant("Chelegance MC-750", AntennaCategory.Vertical, FeedPointType.BaseFed),
            ],
        };
        var svc = new ChecklistService();

        var emcomm = svc.BuildGearPlan(MissionType.Emcomm, inventory);
        Assert.True(emcomm.Pack.Single(e => e.Name == "Chameleon NVIS (4-wire)").Recommended);
        Assert.False(emcomm.Pack.Single(e => e.Name == "Chelegance MC-750").Recommended);

        var sota = svc.BuildGearPlan(MissionType.Sota, inventory);
        // NVIS not suggested for SOTA, but still present (nothing hidden).
        Assert.Contains(sota.Pack, e => e.Name == "Chameleon NVIS (4-wire)");
        Assert.False(sota.Pack.Single(e => e.Name == "Chameleon NVIS (4-wire)").Recommended);
        // Compact vertical is a light portable — suggested for SOTA.
        Assert.True(sota.Pack.Single(e => e.Name == "Chelegance MC-750").Recommended);
    }

    [Fact]
    public void Field_day_suggests_a_dipole_over_a_whip()
    {
        var inventory = new Inventory
        {
            Antennas =
            [
                Ant("Portable whip", AntennaCategory.Whip, FeedPointType.BaseFed),
                Ant("40m dipole", AntennaCategory.Dipole, FeedPointType.CenterFed),
            ],
        };

        var fieldDay = new ChecklistService().BuildGearPlan(MissionType.FieldDay, inventory);
        Assert.True(fieldDay.Pack.Single(e => e.Name == "40m dipole").Recommended);
        Assert.False(fieldDay.Pack.Single(e => e.Name == "Portable whip").Recommended);
    }

    [Fact]
    public void Suggested_antennas_sort_ahead_of_the_rest()
    {
        var inventory = new Inventory
        {
            Antennas =
            [
                Ant("40m dipole", AntennaCategory.Dipole, FeedPointType.CenterFed),   // not SOTA-suited
                Ant("End-fed wire", AntennaCategory.EndFedHalfWave, FeedPointType.EndFedHalfWave), // SOTA-suited
            ],
        };

        var antennas = new ChecklistService().BuildGearPlan(MissionType.Sota, inventory)
            .Pack.Where(e => e.Group == "Antennas").ToList();

        Assert.Equal("End-fed wire", antennas[0].Name);
    }

    [Fact]
    public void Gear_plan_carries_a_mission_packing_tip()
    {
        var sota = new ChecklistService().BuildGearPlan(MissionType.Sota, Inventory.Empty);
        Assert.False(string.IsNullOrWhiteSpace(sota.PackingTip));
        Assert.Contains("light", sota.PackingTip, System.StringComparison.OrdinalIgnoreCase);
    }
}

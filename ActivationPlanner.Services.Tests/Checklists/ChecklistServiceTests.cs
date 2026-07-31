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
}

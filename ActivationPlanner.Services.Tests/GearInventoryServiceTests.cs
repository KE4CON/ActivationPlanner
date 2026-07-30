using System;
using System.Linq;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.Services.GearInventory;
using Xunit;
using Inventory = ActivationPlanner.PropagationModel.Gear.GearInventory;

namespace ActivationPlanner.Services.Tests;

public class GearInventoryServiceTests
{
    private static GearItem Radio(string name = "IC-705") =>
        new() { Category = GearCategory.Radio, Name = name };

    private static AntennaProfile Vertical(string name = "MC-750") => new()
    {
        Name = name,
        Category = AntennaCategory.Vertical,
        FeedPoint = FeedPointType.BaseFed,
        LengthFeet = 10.5,
        HeightFeet = 0,
        RadialCount = 4,
        RadialLengthFeet = 13.0,
    };

    [Fact]
    public async Task Fresh_service_reports_first_run()
    {
        var svc = new GearInventoryService(new InMemoryGearStore());
        await svc.LoadAsync();

        Assert.True(svc.IsFirstRun);
        Assert.True(svc.Current.IsEmpty);
    }

    [Fact]
    public async Task AddItem_persists_and_updates_current()
    {
        var store = new InMemoryGearStore();
        var svc = new GearInventoryService(store);

        var radio = Radio();
        await svc.AddItemAsync(radio);

        Assert.False(svc.IsFirstRun);
        Assert.Single(svc.Current.Items);
        Assert.Equal(radio.Id, svc.Current.Items[0].Id);
        Assert.Equal(1, store.SaveCount);
        Assert.Single(store.Saved.Items); // wrote through to the store
    }

    [Fact]
    public async Task UpdateItem_replaces_matching_row_by_id()
    {
        var store = new InMemoryGearStore();
        var svc = new GearInventoryService(store);
        var radio = Radio("KX2");
        await svc.AddItemAsync(radio);

        await svc.UpdateItemAsync(radio with { Name = "KX3" });

        var only = Assert.Single(svc.Current.Items);
        Assert.Equal(radio.Id, only.Id);
        Assert.Equal("KX3", only.Name);
    }

    [Fact]
    public async Task UpdateItem_unknown_id_throws()
    {
        var svc = new GearInventoryService(new InMemoryGearStore());
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.UpdateItemAsync(Radio()));
    }

    [Fact]
    public async Task RemoveItem_drops_only_the_target()
    {
        var svc = new GearInventoryService(new InMemoryGearStore());
        var keep = Radio("keep");
        var drop = Radio("drop");
        await svc.AddItemAsync(keep);
        await svc.AddItemAsync(drop);

        await svc.RemoveItemAsync(drop.Id);

        var only = Assert.Single(svc.Current.Items);
        Assert.Equal(keep.Id, only.Id);
    }

    [Fact]
    public async Task Antenna_crud_round_trips()
    {
        var svc = new GearInventoryService(new InMemoryGearStore());
        var ant = Vertical();

        await svc.AddAntennaAsync(ant);
        Assert.Single(svc.Current.Antennas);

        await svc.UpdateAntennaAsync(ant with { HeightFeet = 6 });
        Assert.Equal(6, svc.Current.Antennas[0].HeightFeet);

        await svc.RemoveAntennaAsync(ant.Id);
        Assert.Empty(svc.Current.Antennas);
    }

    [Fact]
    public async Task Replace_commits_whole_inventory_in_one_save()
    {
        var store = new InMemoryGearStore();
        var svc = new GearInventoryService(store);

        var built = new Inventory
        {
            Items = [Radio(), new GearItem { Category = GearCategory.Power, Name = "6Ah LiFePO4" }],
            Antennas = [Vertical()],
        };

        await svc.ReplaceAsync(built);

        Assert.Equal(2, svc.Current.Items.Count);
        Assert.Single(svc.Current.Antennas);
        Assert.Equal(1, store.SaveCount); // single commit, not one-per-item
    }

    [Fact]
    public async Task Load_reads_existing_inventory_from_store()
    {
        var seeded = new Inventory { Items = [Radio("seeded")] };
        var svc = new GearInventoryService(new InMemoryGearStore(seeded));

        await svc.LoadAsync();

        Assert.False(svc.IsFirstRun);
        Assert.Equal("seeded", svc.Current.Items[0].Name);
    }
}

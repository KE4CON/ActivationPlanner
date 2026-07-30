using System;
using System.IO;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.Services.GearInventory;
using Xunit;
using Inventory = ActivationPlanner.PropagationModel.Gear.GearInventory;

namespace ActivationPlanner.Services.Tests;

public class JsonGearStoreTests : IDisposable
{
    private readonly string _dir;
    private readonly string _file;

    public JsonGearStoreTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "ActivationPlannerTests", Guid.NewGuid().ToString("N"));
        _file = Path.Combine(_dir, "gear-inventory.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
            Directory.Delete(_dir, recursive: true);
    }

    [Fact]
    public async Task Load_returns_empty_when_file_absent()
    {
        var store = new JsonGearStore(_file);
        var loaded = await store.LoadAsync();
        Assert.True(loaded.IsEmpty);
    }

    [Fact]
    public async Task Save_then_load_round_trips_all_fields()
    {
        var store = new JsonGearStore(_file);
        var original = new Inventory
        {
            Items =
            [
                new GearItem { Category = GearCategory.Radio, Name = "IC-705", Notes = "primary" },
                new GearItem { Category = GearCategory.Power, Name = "Bioenno 6Ah" },
            ],
            Antennas =
            [
                new AntennaProfile
                {
                    Name = "MC-750",
                    Category = AntennaCategory.Vertical,
                    FeedPoint = FeedPointType.BaseFed,
                    LengthFeet = 10.5,
                    HeightFeet = 0,
                    RadialCount = 4,
                    RadialLengthFeet = 13.0,
                },
                new AntennaProfile
                {
                    Name = "40m EFHW",
                    Category = AntennaCategory.EndFedHalfWave,
                    FeedPoint = FeedPointType.EndFedHalfWave,
                    LengthFeet = 66.0,
                    HeightFeet = 30.0,
                },
            ],
        };

        await store.SaveAsync(original);
        var loaded = await store.LoadAsync();

        Assert.Equal(2, loaded.Items.Count);
        Assert.Equal(original.Items[0].Id, loaded.Items[0].Id);
        Assert.Equal("primary", loaded.Items[0].Notes);

        Assert.Equal(2, loaded.Antennas.Count);
        var vertical = loaded.Antennas[0];
        Assert.Equal(AntennaCategory.Vertical, vertical.Category);
        Assert.Equal(FeedPointType.BaseFed, vertical.FeedPoint);
        Assert.Equal(10.5, vertical.LengthFeet);
        Assert.Equal(4, vertical.RadialCount);
        Assert.Equal(13.0, vertical.RadialLengthFeet);

        var efhw = loaded.Antennas[1];
        Assert.Null(efhw.RadialCount); // nullable radial fields survive as null
    }

    [Fact]
    public async Task Enums_are_written_as_readable_strings()
    {
        var store = new JsonGearStore(_file);
        await store.SaveAsync(new Inventory
        {
            Items = [new GearItem { Category = GearCategory.DigitalInterface, Name = "Digirig" }],
        });

        var json = await File.ReadAllTextAsync(_file);
        Assert.Contains("DigitalInterface", json);
        Assert.DoesNotContain("\"Category\": 2", json); // not the numeric enum value
    }

    [Fact]
    public async Task Save_overwrites_previous_contents()
    {
        var store = new JsonGearStore(_file);
        await store.SaveAsync(new Inventory { Items = [new GearItem { Category = GearCategory.Radio, Name = "first" }] });
        await store.SaveAsync(new Inventory { Items = [new GearItem { Category = GearCategory.Radio, Name = "second" }] });

        var loaded = await store.LoadAsync();
        Assert.Single(loaded.Items);
        Assert.Equal("second", loaded.Items[0].Name);
    }

    [Fact]
    public async Task Save_creates_missing_directory()
    {
        Assert.False(Directory.Exists(_dir));
        var store = new JsonGearStore(_file);

        await store.SaveAsync(Inventory.Empty);

        Assert.True(File.Exists(_file));
    }
}

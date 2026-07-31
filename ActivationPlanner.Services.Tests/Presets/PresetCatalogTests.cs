using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.Services.Presets;

namespace ActivationPlanner.Services.Tests.Presets;

public sealed class PresetCatalogTests
{
    [Fact]
    public void Default_catalog_loads_the_bundled_presets()
    {
        var catalog = PresetCatalog.Default;
        Assert.NotEmpty(catalog.Antennas);
        Assert.NotEmpty(catalog.Radios);
    }

    [Fact]
    public void Nvis_preset_is_a_measured_crossed_dipole_with_real_dimensions()
    {
        var nvis = PresetCatalog.Default.Antennas.Single(a => a.Id == "chameleon-nvis-4wire");
        Assert.Equal(AntennaCategory.NvisCrossedDipole, nvis.Category);
        Assert.Equal(ModelingConfidence.Measured, nvis.ModelingConfidence);
        Assert.True(nvis.LengthFeet > 0 && nvis.HeightFeet > 0);
        Assert.Equal("Chameleon NVIS (4-wire)", nvis.DisplayName);
    }

    [Fact]
    public void Loaded_vertical_preset_is_flagged_approximate()
    {
        // A modular/loaded vertical cannot be modeled exactly from published specs — must be flagged.
        var mpas = PresetCatalog.Default.Antennas.Single(a => a.Id == "chameleon-mpas-2");
        Assert.Equal(AntennaCategory.Vertical, mpas.Category);
        Assert.Equal(ModelingConfidence.Approximate, mpas.ModelingConfidence);
    }

    [Fact]
    public void Radio_presets_carry_bands_and_power()
    {
        var ic7300 = PresetCatalog.Default.Radios.Single(r => r.Id == "icom-ic-7300");
        Assert.Equal("Icom IC-7300", ic7300.DisplayName);
        Assert.Equal(100, ic7300.PowerWatts);
        Assert.False(string.IsNullOrWhiteSpace(ic7300.Bands));
    }

    [Fact]
    public void Every_preset_cites_a_source_and_names_a_manufacturer()
    {
        Assert.All(PresetCatalog.Default.Antennas, a =>
        {
            Assert.False(string.IsNullOrWhiteSpace(a.Manufacturer));
            Assert.False(string.IsNullOrWhiteSpace(a.Source));
        });
    }
}

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
    public void Gear_presets_cover_power_and_digital_interfaces()
    {
        var gear = PresetCatalog.Default.Gear;
        Assert.Contains(gear, g => g.Category == GearCategory.Power);
        Assert.Contains(gear, g => g.Category == GearCategory.DigitalInterface);
        Assert.All(gear, g => Assert.False(string.IsNullOrWhiteSpace(g.DisplayName)));
    }

    [Fact]
    public void Every_preset_cites_a_source_and_names_a_manufacturer()
    {
        Assert.All(PresetCatalog.Default.Antennas, a =>
        {
            Assert.False(string.IsNullOrWhiteSpace(a.Manufacturer));
            // Branded models must cite a product source; generic/homebrew entries have no product
            // page (a plain wire dipole isn't a "model"), so a null source is valid for those.
            bool generic = a.Manufacturer.Contains("Homebrew") || a.Manufacturer.Contains("Generic");
            if (!generic)
                Assert.False(string.IsNullOrWhiteSpace(a.Source), $"{a.Id} is a branded preset but cites no source");
        });
    }

    [Fact]
    public void Antenna_preset_ids_are_unique_and_carry_valid_geometry()
    {
        var antennas = PresetCatalog.Default.Antennas;

        // No duplicate IDs (a dup would let two models collide in the picker / lookups).
        var ids = antennas.Select(a => a.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());

        // Length must not be negative and the display name must be non-empty. Length 0 is a valid,
        // intentional sentinel meaning "model a resonant length for each band" (the app's leave-0
        // rule) — used by loaded/broadband and generic-homebrew presets whose exact length varies.
        Assert.All(antennas, a =>
        {
            Assert.True(a.LengthFeet >= 0, $"{a.Id} has a negative length");
            Assert.False(string.IsNullOrWhiteSpace(a.DisplayName));
        });
    }

    [Fact]
    public void Catalog_covers_the_expanded_portable_brand_lineup()
    {
        var antennas = PresetCatalog.Default.Antennas;

        // Spot-check models from the expansion across several manufacturers.
        Assert.Contains(antennas, a => a.Id == "chameleon-tdl");          // Chameleon Tactical Delta Loop
        Assert.Contains(antennas, a => a.Id == "elecraft-ax1");            // Elecraft AX1
        Assert.Contains(antennas, a => a.Id == "sotabeams-band-hopper-iii"); // SOTAbeams linked dipole
        Assert.Contains(antennas, a => a.Id == "lnr-ef-mtr");             // LNR/Par EndFedz

        // Resonant wire antennas model accurately; loaded/broadband ones are flagged approximate.
        Assert.Equal(ModelingConfidence.Measured,
            antennas.Single(a => a.Id == "lnr-ef-mtr").ModelingConfidence);
        Assert.Equal(ModelingConfidence.Approximate,
            antennas.Single(a => a.Id == "elecraft-ax1").ModelingConfidence);
    }
}

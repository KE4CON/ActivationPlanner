using ActivationPlanner.PropagationModel.Antennas;
using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.Services.Antennas;

namespace ActivationPlanner.Services.Tests.Antennas;

/// <summary>
/// Covers the Option A/B trigger rules (CLAUDE.md "Key Domain Rules"). Antenna dimensions
/// are derived from the target wavelength fraction via the same conversion the evaluator
/// uses, so boundary cases land exactly where intended.
/// </summary>
public sealed class AntennaModelingEvaluatorTests
{
    private readonly AntennaModelingEvaluator _eval = new();

    /// <summary>Feet for a dimension of <paramref name="wavelengths"/> at <paramref name="freq"/> MHz.</summary>
    private static double Feet(double wavelengths, double freq) =>
        wavelengths * Wavelength.Metres(freq) / Wavelength.MetresPerFoot;

    private static AntennaProfile Antenna(
        AntennaCategory category, FeedPointType feed, double lengthFeet, double heightFeet) => new()
        {
            Name = $"{category} test",
            Category = category,
            FeedPoint = feed,
            LengthFeet = lengthFeet,
            HeightFeet = heightFeet,
        };

    // ---- verticals ----

    [Fact]
    public void Ground_mounted_quarter_wave_vertical_matches_library()
    {
        const double f = 14.1;
        var antenna = Antenna(AntennaCategory.Vertical, FeedPointType.BaseFed,
            lengthFeet: Feet(0.25, f), heightFeet: 0.0);

        var d = _eval.Evaluate(antenna, f);

        Assert.Equal(AntennaModelingOption.LibraryMatch, d.Option);
        Assert.NotNull(d.LibraryMatch);
    }

    [Fact]
    public void Elevated_vertical_in_distortion_zone_requires_custom_modeling()
    {
        const double f = 14.1;
        // Base height 0.5λ is squarely inside the 0.25λ–1.25λ zone; length still matches a model.
        var antenna = Antenna(AntennaCategory.Vertical, FeedPointType.BaseFed,
            lengthFeet: Feet(0.25, f), heightFeet: Feet(0.5, f));

        var d = _eval.Evaluate(antenna, f);

        Assert.Equal(AntennaModelingOption.CustomModeling, d.Option);
        Assert.Contains("distortion zone", d.Reason);
    }

    [Fact]
    public void Vertical_distortion_zone_lower_boundary_is_inclusive()
    {
        const double f = 14.1;
        var atBoundary = Antenna(AntennaCategory.Vertical, FeedPointType.BaseFed,
            lengthFeet: Feet(0.25, f), heightFeet: Feet(0.25, f));
        var justBelow = Antenna(AntennaCategory.Vertical, FeedPointType.BaseFed,
            lengthFeet: Feet(0.25, f), heightFeet: Feet(0.24, f));

        Assert.Equal(AntennaModelingOption.CustomModeling, _eval.Evaluate(atBoundary, f).Option);
        Assert.Equal(AntennaModelingOption.LibraryMatch, _eval.Evaluate(justBelow, f).Option);
    }

    // ---- dipoles ----

    [Fact]
    public void Dipole_at_a_library_height_matches()
    {
        const double f = 14.1;
        var antenna = Antenna(AntennaCategory.Dipole, FeedPointType.CenterFed,
            lengthFeet: Feet(0.5, f), heightFeet: Feet(0.5, f)); // exactly the 0.5λ model

        var d = _eval.Evaluate(antenna, f);

        Assert.Equal(AntennaModelingOption.LibraryMatch, d.Option);
        Assert.Equal(0.5, d.LibraryMatch!.AssumedHeightWavelengths);
    }

    [Fact]
    public void Dipole_within_height_tolerance_matches()
    {
        const double f = 14.1;
        // 0.28λ is 0.03λ from the 0.25λ model — inside the provisional 0.05λ tolerance.
        var antenna = Antenna(AntennaCategory.Dipole, FeedPointType.CenterFed,
            lengthFeet: Feet(0.5, f), heightFeet: Feet(0.28, f));

        var d = _eval.Evaluate(antenna, f);

        Assert.Equal(AntennaModelingOption.LibraryMatch, d.Option);
        Assert.True(d.HeightDeltaWavelengths <= AntennaModelingThresholds.DipoleHeightDeltaWavelengths);
    }

    [Fact]
    public void Dipole_too_far_from_any_library_height_requires_custom_modeling()
    {
        const double f = 14.1;
        // 0.35λ is 0.10λ from the nearest model (0.25λ) — beyond the 0.05λ tolerance.
        var antenna = Antenna(AntennaCategory.Dipole, FeedPointType.CenterFed,
            lengthFeet: Feet(0.5, f), heightFeet: Feet(0.35, f));

        var d = _eval.Evaluate(antenna, f);

        Assert.Equal(AntennaModelingOption.CustomModeling, d.Option);
        Assert.Contains("Height", d.Reason);
    }

    // ---- end-fed half-wave: the per-band electrical-length case ----

    [Fact]
    public void Efhw_on_its_design_band_matches_but_off_band_requires_custom_modeling()
    {
        // A 40m EFHW: physically a half wave at 7.1 MHz, strung at 0.25λ (its 40m height).
        double lengthFeet = Feet(0.5, 7.1);
        double heightFeet = Feet(0.25, 7.1);
        var efhw = Antenna(AntennaCategory.EndFedHalfWave, FeedPointType.EndFedHalfWave, lengthFeet, heightFeet);

        // On 40m it is a half-wave at its modeled height -> library match.
        var onForty = _eval.Evaluate(efhw, 7.1);
        Assert.Equal(AntennaModelingOption.LibraryMatch, onForty.Option);

        // On 20m the same wire is ~1.0λ long — a different antenna electrically -> custom modeling.
        var onTwenty = _eval.Evaluate(efhw, 14.1);
        Assert.Equal(AntennaModelingOption.CustomModeling, onTwenty.Option);
        Assert.Contains("length", onTwenty.Reason);
    }

    // ---- no library model ----

    [Fact]
    public void Magnetic_loop_always_requires_custom_modeling()
    {
        var loop = Antenna(AntennaCategory.MagneticLoop, FeedPointType.Other, lengthFeet: 3.0, heightFeet: 5.0);
        var d = _eval.Evaluate(loop, 14.1);
        Assert.Equal(AntennaModelingOption.CustomModeling, d.Option);
    }

    [Fact]
    public void Other_category_requires_custom_modeling()
    {
        var other = Antenna(AntennaCategory.Other, FeedPointType.Other, lengthFeet: 20.0, heightFeet: 10.0);
        var d = _eval.Evaluate(other, 14.1);
        Assert.Equal(AntennaModelingOption.CustomModeling, d.Option);
    }

    [Fact]
    public void Nvis_crossed_dipole_always_requires_custom_modeling()
    {
        // No community-library equivalent for a crossed NVIS antenna; it must go through NEC.
        var nvis = Antenna(AntennaCategory.NvisCrossedDipole, FeedPointType.CenterFed, lengthFeet: 45.0, heightFeet: 15.0);
        var d = _eval.Evaluate(nvis, 5.35);
        Assert.Equal(AntennaModelingOption.CustomModeling, d.Option);
        Assert.Contains("NVIS", d.Reason);
    }

    // ---- per-band batch ----

    [Fact]
    public void EvaluateBands_returns_a_decision_per_band()
    {
        const double f = 14.1;
        var dipole = Antenna(AntennaCategory.Dipole, FeedPointType.CenterFed, Feet(0.5, f), Feet(0.5, f));

        var decisions = _eval.EvaluateBands(dipole, [HamBand.M40, HamBand.M20, HamBand.M10]);

        Assert.Equal(3, decisions.Count);
        Assert.All(decisions, d => Assert.Equal(dipole.Id, d.AntennaId));
    }
}

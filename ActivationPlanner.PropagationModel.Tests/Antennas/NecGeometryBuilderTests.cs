using ActivationPlanner.PropagationModel.Antennas;
using ActivationPlanner.PropagationModel.Gear;

namespace ActivationPlanner.PropagationModel.Tests.Antennas;

public sealed class NecGeometryBuilderTests
{
    private readonly NecGeometryBuilder _builder = new();

    private static double Feet(double wavelengths, double freq) =>
        wavelengths * Wavelength.Metres(freq) / Wavelength.MetresPerFoot;

    private static AntennaProfile Antenna(
        AntennaCategory category, FeedPointType feed, double lengthFeet, double heightFeet,
        int? radialCount = null, double? radialFeet = null) => new()
        {
            Name = $"{category}",
            Category = category,
            FeedPoint = feed,
            LengthFeet = lengthFeet,
            HeightFeet = heightFeet,
            RadialCount = radialCount,
            RadialLengthFeet = radialFeet,
        };

    [Fact]
    public void Dipole_is_center_fed_on_an_odd_middle_segment()
    {
        const double f = 14.1;
        var dipole = Antenna(AntennaCategory.Dipole, FeedPointType.CenterFed, Feet(0.5, f), Feet(0.5, f));

        var deck = _builder.Build(dipole, f);

        var wire = Assert.Single(deck.Wires);
        Assert.Equal(1, wire.Tag);
        Assert.True(wire.Segments % 2 == 1, "center-fed dipole needs an odd segment count");
        // Fed on the middle segment of its single wire.
        Assert.Equal((wire.Segments + 1) / 2, deck.Excitation.Segment);
        // Wire is centered on the origin along X at the antenna height.
        Assert.Equal(-wire.X2, wire.X1, precision: 6);
        Assert.True(deck.RadiationPattern.PhiStartDeg == 90); // broadside cut
    }

    [Fact]
    public void End_fed_half_wave_is_fed_at_the_first_segment()
    {
        const double f = 7.1;
        var efhw = Antenna(AntennaCategory.EndFedHalfWave, FeedPointType.EndFedHalfWave, Feet(0.5, f), Feet(0.25, f));

        var deck = _builder.Build(efhw, f);

        Assert.Equal(1, deck.Excitation.Segment);
        var wire = Assert.Single(deck.Wires);
        Assert.Equal(0, wire.X1, precision: 6); // starts at the origin (fed end)
    }

    [Fact]
    public void Vertical_radiator_is_upright_and_base_fed()
    {
        const double f = 14.1;
        var vertical = Antenna(AntennaCategory.Vertical, FeedPointType.BaseFed, Feet(0.25, f), heightFeet: 0);

        var deck = _builder.Build(vertical, f);

        var radiator = deck.Wires[0];
        Assert.Equal(radiator.X1, radiator.X2, precision: 6); // vertical: same X
        Assert.Equal(radiator.Y1, radiator.Y2, precision: 6); // vertical: same Y
        Assert.True(radiator.Z2 > radiator.Z1);               // goes up
        Assert.Equal(1, deck.Excitation.Segment);             // base fed
        Assert.NotNull(deck.Ground);
    }

    [Fact]
    public void Vertical_with_radials_emits_a_wire_per_radial()
    {
        const double f = 14.1;
        var vertical = Antenna(AntennaCategory.Vertical, FeedPointType.BaseFed, Feet(0.25, f), 0,
            radialCount: 4, radialFeet: Feet(0.25, f));

        var deck = _builder.Build(vertical, f);

        Assert.Equal(1 + 4, deck.Wires.Count); // radiator + 4 radials
        Assert.All(deck.Wires.Skip(1), r => Assert.Equal(0.0, r.Z1, precision: 6)); // radials at base height 0
    }

    [Fact]
    public void Magnetic_loop_is_not_auto_generated()
    {
        var loop = Antenna(AntennaCategory.MagneticLoop, FeedPointType.Other, lengthFeet: 3, heightFeet: 5);
        Assert.Throws<NotSupportedException>(() => _builder.Build(loop, 14.1));
    }

    [Fact]
    public void Segment_count_scales_with_electrical_length()
    {
        // A full-wave wire should get more segments than a short one.
        var shortWire = Antenna(AntennaCategory.EndFedHalfWave, FeedPointType.EndFedHalfWave, Feet(0.1, 14.1), Feet(0.25, 14.1));
        var longWire = Antenna(AntennaCategory.EndFedHalfWave, FeedPointType.EndFedHalfWave, Feet(2.0, 14.1), Feet(0.25, 14.1));
        Assert.True(_builder.Build(longWire, 14.1).Wires[0].Segments >
                    _builder.Build(shortWire, 14.1).Wires[0].Segments);
    }
}

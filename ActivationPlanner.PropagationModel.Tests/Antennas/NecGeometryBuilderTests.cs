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
        int? radialCount = null, double? radialFeet = null, double? radialHeightFeet = null) => new()
        {
            Name = $"{category}",
            Category = category,
            FeedPoint = feed,
            LengthFeet = lengthFeet,
            HeightFeet = heightFeet,
            RadialCount = radialCount,
            RadialLengthFeet = radialFeet,
            RadialHeightFeet = radialHeightFeet,
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
        // Full upper-hemisphere sweep (multiple azimuths) so the 3D far-field surface has an az x el grid.
        Assert.True(deck.RadiationPattern.PhiCount > 1, "expected a multi-azimuth far-field sweep");
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
        // Base height is 0, but radials are horizontal wires — NEC rejects them in the ground plane,
        // so the whole assembly is lifted to the ground clearance. Radiator base and radials share
        // that height (electrically connected at the feed).
        Assert.Equal(NecGeometryBuilder.GroundClearanceMetres, deck.Wires[0].Z1, precision: 6);
        Assert.All(deck.Wires.Skip(1), r =>
            Assert.Equal(NecGeometryBuilder.GroundClearanceMetres, r.Z1, precision: 6));
    }

    [Fact]
    public void Elevated_radials_lift_the_feed_and_radials_to_the_stated_height()
    {
        const double f = 14.1;
        // A ground-spiked whip is entered with a 0 base height, but the radials are raised ~3 ft on
        // stakes. The radial height should drive the assembly: the feed and every radial sit at that
        // height (electrically connected there), not on the ground.
        const double radialHeightFt = 3.0;
        var vertical = Antenna(AntennaCategory.Vertical, FeedPointType.BaseFed, Feet(0.25, f), heightFeet: 0,
            radialCount: 4, radialFeet: Feet(0.25, f), radialHeightFeet: radialHeightFt);

        var deck = _builder.Build(vertical, f);

        double expectedZ = Wavelength.FeetToMetres(radialHeightFt);
        Assert.Equal(1 + 4, deck.Wires.Count);              // radiator + 4 radials
        Assert.Equal(expectedZ, deck.Wires[0].Z1, precision: 6); // radiator base at the radial height
        Assert.True(deck.Wires[0].Z2 > deck.Wires[0].Z1);        // radiator still rises from the feed
        Assert.All(deck.Wires.Skip(1), r => Assert.Equal(expectedZ, r.Z1, precision: 6)); // radials at that height
    }

    [Fact]
    public void On_ground_radials_ignore_a_zero_radial_height()
    {
        const double f = 14.1;
        // Radial height 0 (or null) keeps today's behavior: the assembly is only nudged to the
        // ground clearance so the horizontal radials clear the ground plane.
        var vertical = Antenna(AntennaCategory.Vertical, FeedPointType.BaseFed, Feet(0.25, f), heightFeet: 0,
            radialCount: 4, radialFeet: Feet(0.25, f), radialHeightFeet: 0);

        var deck = _builder.Build(vertical, f);

        Assert.All(deck.Wires, w =>
            Assert.Equal(NecGeometryBuilder.GroundClearanceMetres, w.Z1, precision: 6));
    }

    [Fact]
    public void Ground_mounted_dipole_is_lifted_clear_of_the_ground_plane()
    {
        // A dipole entered with zero height would lie in the ground plane (NEC "GEOMETRY DATA
        // ERROR"); it is nudged up to the ground clearance instead.
        var dipole = Antenna(AntennaCategory.Dipole, FeedPointType.CenterFed, Feet(0.5, 14.1), heightFeet: 0);
        var deck = _builder.Build(dipole, 14.1);
        Assert.Equal(NecGeometryBuilder.GroundClearanceMetres, deck.Wires[0].Z1, precision: 6);
    }

    [Fact]
    public void Zero_length_antenna_is_rejected_with_a_clear_error()
    {
        // A radiator with no length is a degenerate zero-length wire; fail loudly rather than
        // emitting a deck nec2c rejects with an opaque exit code. (The modeler substitutes a
        // resonant length before this point; this guards direct/degenerate calls.)
        var vertical = Antenna(AntennaCategory.Vertical, FeedPointType.BaseFed, lengthFeet: 0, heightFeet: 25);
        Assert.Throws<ArgumentException>(() => _builder.Build(vertical, 14.1));
    }

    [Fact]
    public void Nvis_crossed_dipole_has_four_legs_meeting_at_an_apex_feed()
    {
        const double f = 5.35;
        var nvis = Antenna(AntennaCategory.NvisCrossedDipole, FeedPointType.CenterFed,
            lengthFeet: 45, heightFeet: 15);

        var deck = _builder.Build(nvis, f);

        // Four legs, one per wire.
        Assert.Equal(4, deck.Wires.Count);

        // All four share the apex as their first endpoint (the common feed point), at the mast top.
        double apexZ = Wavelength.FeetToMetres(15);
        Assert.All(deck.Wires, w =>
        {
            Assert.Equal(0.0, w.X1, precision: 6);
            Assert.Equal(0.0, w.Y1, precision: 6);
            Assert.Equal(apexZ, w.Z1, precision: 6);
        });

        // Legs run along +X, -X, +Y, -Y and slope downward (tip below the apex).
        Assert.All(deck.Wires, w => Assert.True(w.Z2 < w.Z1, "each leg should slope down from the apex"));
        Assert.Contains(deck.Wires, w => w.X2 > 0 && Math.Abs(w.Y2) < 1e-6);
        Assert.Contains(deck.Wires, w => w.X2 < 0 && Math.Abs(w.Y2) < 1e-6);
        Assert.Contains(deck.Wires, w => w.Y2 > 0 && Math.Abs(w.X2) < 1e-6);
        Assert.Contains(deck.Wires, w => w.Y2 < 0 && Math.Abs(w.X2) < 1e-6);

        // Fed at the apex (segment 1 of the first leg), over ground.
        Assert.Equal(1, deck.Excitation.Tag);
        Assert.Equal(1, deck.Excitation.Segment);
        Assert.NotNull(deck.Ground);
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

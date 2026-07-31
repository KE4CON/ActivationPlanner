using ActivationPlanner.PropagationModel.Antennas;
using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.ProcessEngine.Nec;

namespace ActivationPlanner.PropagationModel.Tests.Antennas;

public sealed class NecAntennaModelerTests
{
    private sealed class FakeNecRunner(NecRawResult result) : INecRunner
    {
        public NecGeometryInput? ReceivedGeometry { get; private set; }

        public Task<NecRawResult> RunAsync(
            NecGeometryInput geometry, string? runDirectory = null, CancellationToken ct = default)
        {
            ReceivedGeometry = geometry;
            return Task.FromResult(result);
        }
    }

    private static NecRawResult Raw() => new()
    {
        Impedance = new NecImpedance(72.5, 8.1),
        Pattern =
        [
            new NecRadiationSample(90, 0, -20, -20, -20), // horizon
            new NecRadiationSample(65, 0, -14, 6.1, 6.2), // peak (elevation 25)
            new NecRadiationSample(30, 0, -16, 3.7, 3.8),
            new NecRadiationSample(0, 0, -999, -999, -999), // zenith
        ],
    };

    private static AntennaProfile Dipole() => new()
    {
        Name = "20m dipole",
        Category = AntennaCategory.Dipole,
        FeedPoint = FeedPointType.CenterFed,
        LengthFeet = 33,
        HeightFeet = 33,
    };

    [Fact]
    public async Task Maps_peak_gain_and_takeoff_angle()
    {
        var modeler = new NecAntennaModeler(new FakeNecRunner(Raw()));
        AntennaPattern pattern = await modeler.ModelAsync(Dipole(), 14.1);

        Assert.Equal(6.2, pattern.PeakGainDbi, precision: 2);
        Assert.Equal(25.0, pattern.TakeoffAngleDeg, precision: 2); // 90 - 65
        Assert.Equal(14.1, pattern.FrequencyMhz);
    }

    [Fact]
    public async Task Carries_feed_point_impedance()
    {
        var modeler = new NecAntennaModeler(new FakeNecRunner(Raw()));
        var pattern = await modeler.ModelAsync(Dipole(), 14.1);
        Assert.Equal(72.5, pattern.FeedpointResistanceOhms);
        Assert.Equal(8.1, pattern.FeedpointReactanceOhms);
    }

    [Fact]
    public async Task Elevation_samples_are_ascending_and_converted_from_theta()
    {
        var modeler = new NecAntennaModeler(new FakeNecRunner(Raw()));
        var pattern = await modeler.ModelAsync(Dipole(), 14.1);

        Assert.Equal(4, pattern.Elevation.Count);
        Assert.Equal(0, pattern.Elevation[0].ElevationAngleDeg);   // from theta 90
        Assert.Equal(90, pattern.Elevation[^1].ElevationAngleDeg); // from theta 0
    }

    [Fact]
    public async Task Passes_generated_geometry_to_the_runner()
    {
        var runner = new FakeNecRunner(Raw());
        var modeler = new NecAntennaModeler(runner);
        await modeler.ModelAsync(Dipole(), 14.1);

        Assert.NotNull(runner.ReceivedGeometry);
        Assert.Equal(14.1, runner.ReceivedGeometry!.FrequencyMhz);
        Assert.Single(runner.ReceivedGeometry.Wires);
    }

    [Fact]
    public async Task Fully_specified_antenna_carries_no_estimate_note()
    {
        var modeler = new NecAntennaModeler(new FakeNecRunner(Raw()));
        var pattern = await modeler.ModelAsync(Dipole(), 14.1); // Dipole() has a real length
        Assert.Null(pattern.EstimateNote);
    }

    [Fact]
    public async Task Nvis_crossed_dipole_models_four_legs_and_estimates_missing_leg_length()
    {
        var nvis = new AntennaProfile
        {
            Name = "Chameleon NVIS",
            Category = AntennaCategory.NvisCrossedDipole,
            FeedPoint = FeedPointType.CenterFed,
            LengthFeet = 0, // leg length not entered
            HeightFeet = 15,
        };

        var runner = new FakeNecRunner(Raw());
        var pattern = await new NecAntennaModeler(runner).ModelAsync(nvis, 5.35);

        Assert.NotNull(pattern.EstimateNote);
        Assert.Contains("quarter-wave", pattern.EstimateNote);
        Assert.Equal(4, runner.ReceivedGeometry!.Wires.Count); // two crossed dipoles = four legs
    }

    [Fact]
    public async Task Substitutes_a_resonant_quarter_wave_when_a_verticals_length_is_missing()
    {
        // A loaded/modular vertical (e.g. Chameleon MPAS) often has no single electrical length the
        // operator would enter. Rather than emit a degenerate zero-length wire, the modeler fills in
        // a resonant quarter-wave and flags the result as an estimate.
        var vertical = new AntennaProfile
        {
            Name = "Chameleon MPAS",
            Category = AntennaCategory.Vertical,
            FeedPoint = FeedPointType.BaseFed,
            LengthFeet = 0,
            HeightFeet = 25,
        };
        const double f = 14.1;

        var runner = new FakeNecRunner(Raw());
        var pattern = await new NecAntennaModeler(runner).ModelAsync(vertical, f);

        Assert.NotNull(pattern.EstimateNote);
        Assert.Contains("quarter-wave", pattern.EstimateNote);

        // The radiator wire should span a quarter wavelength, and clear the ground plane.
        NecWire radiator = runner.ReceivedGeometry!.Wires[0];
        double modeledLengthM = radiator.Z2 - radiator.Z1;
        Assert.Equal(0.25 * Wavelength.Metres(f), modeledLengthM, precision: 3);
        Assert.True(radiator.Z1 > 0);
    }
}

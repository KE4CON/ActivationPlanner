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
}

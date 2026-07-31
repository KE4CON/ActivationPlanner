using ActivationPlanner.PropagationModel.Antennas;
using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.Services.Antennas;
using ActivationPlanner.Services.Planning;

namespace ActivationPlanner.Services.Tests.Planning;

public sealed class PlanningServiceTests
{
    /// <summary>A predictor returning canned band predictions so the planning logic is tested in isolation.</summary>
    private sealed class StubPredictor(CircuitPrediction result) : IPropagationPredictor
    {
        public CircuitQuery? ReceivedQuery { get; private set; }

        public Task<CircuitPrediction> PredictAsync(CircuitQuery query, CancellationToken ct = default)
        {
            ReceivedQuery = query;
            return Task.FromResult(result);
        }
    }

    private static CircuitQuery Query() => new()
    {
        Transmitter = new GeoLocation(34.05, -84.30),
        Receiver = new GeoLocation(40.00, -105.00),
        Month = 6,
        Year = 2026,
        SunspotNumber = 70,
        Bands = [HamBand.M40, HamBand.M20, HamBand.M10],
    };

    private static BandPrediction Band(HamBand band, double avgRel)
    {
        double freq = HamBands.RepresentativeFrequencyMhz(band);
        return new BandPrediction
        {
            Band = band,
            FrequencyMhz = freq,
            Hours =
            [
                new BandHourSample { HourUtc = 1, Reliability = avgRel, MufMhz = 15 },
                new BandHourSample { HourUtc = 2, Reliability = avgRel, MufMhz = 15 },
            ],
        };
    }

    private static CircuitPrediction Prediction() => new()
    {
        DistanceKm = 2000,
        HoursUtc = [1, 2],
        Bands =
        [
            Band(HamBand.M40, 0.30),
            Band(HamBand.M20, 0.85),
            Band(HamBand.M10, 0.55),
        ],
    };

    private static double Feet(double wavelengths, double freq) =>
        wavelengths * Wavelength.Metres(freq) / Wavelength.MetresPerFoot;

    [Fact]
    public async Task Ranks_bands_best_reliability_first()
    {
        var svc = new PlanningService(new StubPredictor(Prediction()));
        var plan = await svc.PlanAsync(Query(), []);

        Assert.Equal(HamBand.M20, plan.Bands[0].Band); // 0.85
        Assert.Equal(HamBand.M10, plan.Bands[1].Band); // 0.55
        Assert.Equal(HamBand.M40, plan.Bands[2].Band); // 0.30
    }

    [Fact]
    public async Task Each_band_evaluates_every_owned_antenna()
    {
        var dipole = new AntennaProfile
        {
            Name = "20m dipole", Category = AntennaCategory.Dipole, FeedPoint = FeedPointType.CenterFed,
            LengthFeet = Feet(0.5, 14.1), HeightFeet = Feet(0.5, 14.1),
        };
        var loop = new AntennaProfile
        {
            Name = "Mag loop", Category = AntennaCategory.MagneticLoop, FeedPoint = FeedPointType.Other,
            LengthFeet = 3, HeightFeet = 5,
        };

        var svc = new PlanningService(new StubPredictor(Prediction()));
        var plan = await svc.PlanAsync(Query(), [dipole, loop]);

        var m20 = plan.Bands.Single(b => b.Band == HamBand.M20);
        Assert.Equal(2, m20.OwnedAntennas.Count);
        // On 20m the dipole is library-ready; the loop always needs custom modeling.
        var dipoleSuggestion = m20.OwnedAntennas.Single(s => s.Antenna.Name == "20m dipole");
        var loopSuggestion = m20.OwnedAntennas.Single(s => s.Antenna.Name == "Mag loop");
        Assert.True(dipoleSuggestion.IsLibraryReady);
        Assert.False(loopSuggestion.IsLibraryReady);
    }

    [Fact]
    public async Task Library_ready_antennas_are_listed_before_those_needing_modeling()
    {
        var dipole = new AntennaProfile
        {
            Name = "20m dipole", Category = AntennaCategory.Dipole, FeedPoint = FeedPointType.CenterFed,
            LengthFeet = Feet(0.5, 14.1), HeightFeet = Feet(0.5, 14.1),
        };
        var loop = new AntennaProfile
        {
            Name = "Mag loop", Category = AntennaCategory.MagneticLoop, FeedPoint = FeedPointType.Other,
            LengthFeet = 3, HeightFeet = 5,
        };

        var svc = new PlanningService(new StubPredictor(Prediction()));
        // Pass loop first to prove ordering is by readiness, not input order.
        var plan = await svc.PlanAsync(Query(), [loop, dipole]);

        var m20 = plan.Bands.Single(b => b.Band == HamBand.M20);
        Assert.True(m20.OwnedAntennas[0].IsLibraryReady);
        Assert.Equal("20m dipole", m20.OwnedAntennas[0].Antenna.Name);
    }

    [Fact]
    public async Task Carries_hours_distance_and_forwards_query()
    {
        var predictor = new StubPredictor(Prediction());
        var svc = new PlanningService(predictor);
        var query = Query();

        var plan = await svc.PlanAsync(query, []);

        Assert.Equal([1, 2], plan.HoursUtc);
        Assert.Equal(2000, plan.DistanceKm);
        Assert.False(plan.HasOwnedAntennas);
        Assert.Same(query, predictor.ReceivedQuery);
    }
}

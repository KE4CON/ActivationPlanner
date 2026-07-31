using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.ProcessEngine.Voacap;

namespace ActivationPlanner.PropagationModel.Tests.Voacap;

/// <summary>Verifies the engine composes the runner and mapper correctly, via a fake runner.</summary>
public sealed class VoacapPropagationEngineTests
{
    /// <summary>A fake runner: records the deck it was given and returns a canned raw result.</summary>
    private sealed class FakeRunner : IVoacapRunner
    {
        public VoacapDeckInput? ReceivedDeck { get; private set; }

        public Task<VoacapRawPrediction> RunAsync(
            VoacapDeckInput deck, string? runDirectory = null, CancellationToken ct = default)
        {
            ReceivedDeck = deck;
            var raw = new VoacapRawPrediction
            {
                Hours =
                [
                    new VoacapHourBlock
                    {
                        HourUtc = 1.0,
                        MufMhz = 15.0,
                        Samples =
                        [
                            new VoacapFrequencySample
                            {
                                FrequencyMhz = 14.1, Reliability = 0.77, Snr = 22,
                                RawRow = new Dictionary<string, string?>(),
                            },
                        ],
                    },
                ],
            };
            return Task.FromResult(raw);
        }
    }

    private static CircuitQuery Query() => new()
    {
        Transmitter = new GeoLocation(34.05, -84.30),
        Receiver = new GeoLocation(40.00, -105.00),
        Month = 6,
        Year = 2026,
        SunspotNumber = 70,
        Bands = [HamBand.M20],
    };

    [Fact]
    public async Task PredictAsync_renders_deck_from_query_and_maps_result()
    {
        var runner = new FakeRunner();
        var engine = new VoacapPropagationEngine(runner);

        CircuitPrediction prediction = await engine.PredictAsync(Query());

        // The deck the engine handed the runner reflects the query.
        Assert.NotNull(runner.ReceivedDeck);
        Assert.Equal(2026, runner.ReceivedDeck!.Year);
        Assert.Equal([14.100], runner.ReceivedDeck.FrequenciesMhz);

        // The raw result was mapped back onto the requested band.
        var m20 = Assert.Single(prediction.Bands);
        Assert.Equal(HamBand.M20, m20.Band);
        Assert.Equal(0.77, m20.Hours.Single(h => h.HourUtc == 1).Reliability);
    }

    [Fact]
    public async Task PredictAsync_forwards_cancellation_token()
    {
        var engine = new VoacapPropagationEngine(new ThrowIfCancelledRunner());
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(() => engine.PredictAsync(Query(), cts.Token));
    }

    private sealed class ThrowIfCancelledRunner : IVoacapRunner
    {
        public Task<VoacapRawPrediction> RunAsync(
            VoacapDeckInput deck, string? runDirectory = null, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(new VoacapRawPrediction { Hours = [] });
        }
    }
}

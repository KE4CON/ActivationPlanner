using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.ProcessEngine.Voacap;

namespace ActivationPlanner.PropagationModel.Tests.Voacap;

/// <summary>Tests the domain ↔ VOACAP-wire translation in both directions.</summary>
public sealed class VoacapCircuitMapperTests
{
    private static CircuitQuery SampleQuery(params HamBand[] bands) => new()
    {
        Transmitter = new GeoLocation(34.05, -84.30),   // near Atlanta
        Receiver = new GeoLocation(40.00, -105.00),     // Colorado
        Month = 6,
        Year = 2026,
        SunspotNumber = 70,
        Bands = bands.Length > 0 ? bands : [HamBand.M40, HamBand.M20, HamBand.M15],
        TransmitPowerWatts = 100,
        Noise = NoiseEnvironment.Residential,
    };

    // ---- forward: CircuitQuery -> VoacapDeckInput ----

    [Fact]
    public void ToDeck_maps_coordinates_year_month_and_ssn()
    {
        var deck = new VoacapCircuitMapper().ToDeck(SampleQuery());
        Assert.Equal(34.05, deck.TxLatitudeDeg);
        Assert.Equal(-84.30, deck.TxLongitudeDeg);
        Assert.Equal(40.00, deck.RxLatitudeDeg);
        Assert.Equal(-105.00, deck.RxLongitudeDeg);
        Assert.Equal(2026, deck.Year);
        Assert.Equal(6.0, deck.MonthValue);
        Assert.Equal(70, deck.SunspotNumber);
    }

    [Fact]
    public void ToDeck_converts_watts_to_kilowatts_on_tx_antenna()
    {
        var deck = new VoacapCircuitMapper().ToDeck(SampleQuery() with { TransmitPowerWatts = 100 });
        Assert.Equal(0.1, deck.TxAntenna.PowerKw, precision: 6);
    }

    [Fact]
    public void ToDeck_maps_noise_environment_to_dbw()
    {
        var deck = new VoacapCircuitMapper().ToDeck(SampleQuery() with { Noise = NoiseEnvironment.Remote });
        Assert.Equal(164.0, deck.NoiseDbw);
    }

    [Fact]
    public void ToDeck_maps_bands_to_representative_frequencies_in_order()
    {
        var deck = new VoacapCircuitMapper().ToDeck(SampleQuery(HamBand.M40, HamBand.M20, HamBand.M10));
        Assert.Equal(new[] { 7.100, 14.100, 28.300 }, deck.FrequenciesMhz);
    }

    [Fact]
    public void ToDeck_rejects_empty_band_list()
    {
        var mapper = new VoacapCircuitMapper();
        Assert.Throws<ArgumentException>(() => mapper.ToDeck(SampleQuery() with { Bands = [] }));
    }

    // ---- reverse: VoacapRawPrediction -> CircuitPrediction ----

    private static VoacapRawPrediction SyntheticRaw()
    {
        // Two hours, three bands (40m/20m/15m at their representative frequencies).
        VoacapFrequencySample S(double f, double rel, double snr) => new()
        {
            FrequencyMhz = f,
            Reliability = rel,
            Snr = snr,
            Mode = "1F2",
            RawRow = new Dictionary<string, string?>(),
        };

        return new VoacapRawPrediction
        {
            SunspotNumber = 70,
            TransmitterLabel = "HOME",
            ReceiverLabel = "TARGET",
            Hours =
            [
                new VoacapHourBlock
                {
                    HourUtc = 1.0, MufMhz = 12.0,
                    Samples = [S(7.1, 0.90, 30), S(14.1, 0.20, 5), S(21.1, 0.00, -10)],
                },
                new VoacapHourBlock
                {
                    HourUtc = 2.0, MufMhz = 18.0,
                    Samples = [S(7.1, 0.85, 28), S(14.1, 0.80, 25), S(21.1, 0.10, 2)],
                },
            ],
        };
    }

    [Fact]
    public void ToPrediction_associates_each_band_with_its_frequency_samples()
    {
        var query = SampleQuery(HamBand.M40, HamBand.M20, HamBand.M15);
        var prediction = new VoacapCircuitMapper().ToPrediction(query, SyntheticRaw());

        Assert.Equal(3, prediction.Bands.Count);
        var m20 = prediction.Bands.Single(b => b.Band == HamBand.M20);
        Assert.Equal(14.1, m20.FrequencyMhz);
        Assert.Equal(0.20, m20.Hours.Single(h => h.HourUtc == 1).Reliability);
        Assert.Equal(0.80, m20.Hours.Single(h => h.HourUtc == 2).Reliability);
    }

    [Fact]
    public void ToPrediction_flags_bands_above_the_hourly_muf()
    {
        var query = SampleQuery(HamBand.M40, HamBand.M20, HamBand.M15);
        var prediction = new VoacapCircuitMapper().ToPrediction(query, SyntheticRaw());

        // Hour 1 MUF = 12.0: 15m (21.1) is above the MUF, 40m (7.1) is not.
        var m15Hour1 = prediction.Bands.Single(b => b.Band == HamBand.M15).Hours.Single(h => h.HourUtc == 1);
        var m40Hour1 = prediction.Bands.Single(b => b.Band == HamBand.M40).Hours.Single(h => h.HourUtc == 1);
        Assert.True(m15Hour1.IsAboveMuf);
        Assert.False(m40Hour1.IsAboveMuf);
    }

    [Fact]
    public void BandPrediction_aggregates_best_and_average_reliability()
    {
        var query = SampleQuery(HamBand.M40, HamBand.M20, HamBand.M15);
        var prediction = new VoacapCircuitMapper().ToPrediction(query, SyntheticRaw());

        var m40 = prediction.Bands.Single(b => b.Band == HamBand.M40);
        Assert.Equal(0.90, m40.BestReliability);
        Assert.Equal(1, m40.BestHourUtc);
        Assert.Equal(0.875, m40.AverageReliability, precision: 6);
    }

    [Fact]
    public void RankByAverageReliability_orders_best_band_first()
    {
        var query = SampleQuery(HamBand.M40, HamBand.M20, HamBand.M15);
        var prediction = new VoacapCircuitMapper().ToPrediction(query, SyntheticRaw());

        var ranked = prediction.RankByAverageReliability();
        Assert.Equal(HamBand.M40, ranked[0].Band); // 0.875 avg beats 20m (0.50) and 15m (0.05)
    }

    [Fact]
    public void ToPrediction_carries_distance_and_labels()
    {
        var query = SampleQuery();
        var prediction = new VoacapCircuitMapper().ToPrediction(query, SyntheticRaw());
        Assert.Equal("HOME", prediction.TransmitterLabel);
        Assert.Equal("TARGET", prediction.ReceiverLabel);
        Assert.True(prediction.DistanceKm > 1900 && prediction.DistanceKm < 2100); // ~2000 km ATL->CO
    }
}

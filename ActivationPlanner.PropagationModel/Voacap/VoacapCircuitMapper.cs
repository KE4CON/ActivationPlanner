using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.ProcessEngine.Voacap;

namespace ActivationPlanner.PropagationModel.Voacap;

/// <summary>
/// Translates between the planner domain (<see cref="CircuitQuery"/> /
/// <see cref="CircuitPrediction"/>) and the ProcessEngine's VOACAP wire types
/// (<see cref="VoacapDeckInput"/> / <see cref="VoacapRawPrediction"/>).
/// <para>
/// This is where domain concepts become VOACAP parameters and back: bands become
/// representative frequencies, watts become kW, a noise environment becomes a dBW figure,
/// and VOACAP's per-hour frequency samples are re-associated with the bands that produced
/// them. Pure and deterministic.
/// </para>
/// </summary>
public sealed class VoacapCircuitMapper
{
    /// <summary>Phase 2 uses an isotropic pattern at both ends; owned-antenna modeling is Phase 3.</summary>
    private const string DefaultAntennaFile = "default/isotrope";

    /// <summary>Tolerance for matching VOACAP's rounded output frequency back to a band, MHz.</summary>
    private const double FrequencyMatchToleranceMhz = 0.25;

    /// <summary>Build the VOACAP input deck for a query.</summary>
    public VoacapDeckInput ToDeck(CircuitQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.Bands.Count == 0)
            throw new ArgumentException("At least one band must be requested.", nameof(query));

        double powerKw = query.TransmitPowerWatts / 1000.0;
        var txAntenna = new VoacapAntenna(DefaultAntennaFile, BearingDeg: 0.0, PowerKw: powerKw);
        var rxAntenna = new VoacapAntenna(DefaultAntennaFile, BearingDeg: 0.0, PowerKw: 0.0);

        return new VoacapDeckInput
        {
            TxLatitudeDeg = query.Transmitter.LatitudeDeg,
            TxLongitudeDeg = query.Transmitter.LongitudeDeg,
            RxLatitudeDeg = query.Receiver.LatitudeDeg,
            RxLongitudeDeg = query.Receiver.LongitudeDeg,
            Path = query.UseLongPath ? VoacapPath.Long : VoacapPath.Short,
            StartHourUtc = query.StartHourUtc,
            StopHourUtc = query.StopHourUtc,
            HourIncrement = query.HourIncrement,
            Year = query.Year,
            MonthValue = query.Month, // whole-month value, e.g. 6 -> 6.00
            SunspotNumber = query.SunspotNumber,
            NoiseDbw = query.Noise.NoiseDbw(),
            RequiredReliabilityPercent = query.RequiredReliabilityPercent,
            RequiredSnrDb = query.RequiredSnrDb,
            TxAntenna = txAntenna,
            RxAntenna = rxAntenna,
            FrequenciesMhz = query.Bands.Select(HamBands.RepresentativeFrequencyMhz).ToList(),
        };
    }

    /// <summary>Re-associate a parsed VOACAP run with the bands and hours the query asked for.</summary>
    public CircuitPrediction ToPrediction(CircuitQuery query, VoacapRawPrediction raw)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(raw);

        var hoursUtc = raw.Hours.Select(h => (int)Math.Round(h.HourUtc)).OrderBy(h => h).ToList();

        var bands = new List<BandPrediction>(query.Bands.Count);
        foreach (HamBand band in query.Bands)
        {
            double freq = HamBands.RepresentativeFrequencyMhz(band);
            var samples = new List<BandHourSample>(raw.Hours.Count);

            foreach (VoacapHourBlock block in raw.Hours)
            {
                VoacapFrequencySample? match = NearestSample(block, freq);
                samples.Add(new BandHourSample
                {
                    HourUtc = (int)Math.Round(block.HourUtc),
                    Reliability = match?.Reliability,
                    Snr = match?.Snr,
                    SignalPowerDbw = match?.SignalPowerDbw,
                    Mode = match?.Mode,
                    MufMhz = block.MufMhz,
                    IsAboveMuf = freq > block.MufMhz,
                });
            }

            bands.Add(new BandPrediction
            {
                Band = band,
                FrequencyMhz = freq,
                Hours = samples.OrderBy(s => s.HourUtc).ToList(),
            });
        }

        return new CircuitPrediction
        {
            Bands = bands,
            HoursUtc = hoursUtc,
            DistanceKm = query.DistanceKm,
            TransmitterLabel = raw.TransmitterLabel,
            ReceiverLabel = raw.ReceiverLabel,
        };
    }

    /// <summary>The evaluated sample in the block whose frequency is closest to <paramref name="freqMhz"/>.</summary>
    private static VoacapFrequencySample? NearestSample(VoacapHourBlock block, double freqMhz)
    {
        VoacapFrequencySample? best = null;
        double bestDelta = FrequencyMatchToleranceMhz;
        foreach (VoacapFrequencySample s in block.Samples)
        {
            double delta = Math.Abs(s.FrequencyMhz - freqMhz);
            if (delta <= bestDelta)
            {
                bestDelta = delta;
                best = s;
            }
        }
        return best;
    }
}

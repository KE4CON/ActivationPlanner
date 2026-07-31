using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Voacap;

namespace ActivationPlanner.UI.Sample;

/// <summary>
/// A stand-in <see cref="IPropagationPredictor"/> that fabricates a plausible day/night
/// propagation pattern so the planning screen is usable before the operator has installed
/// and configured VOACAP. It performs NO real prediction — the UI flags this as sample data.
/// Once a VOACAP install is configured (a later settings feature), the composition root wires
/// the real <see cref="VoacapPropagationEngine"/> here instead.
/// </summary>
public sealed class SamplePropagationPredictor : IPropagationPredictor
{
    /// <summary>True — lets the UI show a clear "sample data" banner.</summary>
    public bool IsSample => true;

    public Task<CircuitPrediction> PredictAsync(CircuitQuery query, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        int[] hours = Hours(query).ToArray();
        var bands = query.Bands
            .Select(band => BuildBand(band, hours))
            .ToList();

        var prediction = new CircuitPrediction
        {
            Bands = bands,
            HoursUtc = hours,
            DistanceKm = query.DistanceKm,
            TransmitterLabel = "Your station",
            ReceiverLabel = "Target",
        };
        return Task.FromResult(prediction);
    }

    private static IEnumerable<int> Hours(CircuitQuery query)
    {
        int step = Math.Max(1, query.HourIncrement);
        for (int h = query.StartHourUtc; h <= query.StopHourUtc; h += step)
            yield return h;
    }

    private static BandPrediction BuildBand(HamBand band, IReadOnlyList<int> hours)
    {
        double freq = HamBands.RepresentativeFrequencyMhz(band);
        var samples = hours
            .Select(h => Sample(freq, h))
            .ToList();

        return new BandPrediction
        {
            Band = band,
            FrequencyMhz = freq,
            Hours = samples,
        };
    }

    private static BandHourSample Sample(double freq, int hour)
    {
        double muf = Muf(hour);
        double reliability = Reliability(freq, hour, muf);
        return new BandHourSample
        {
            HourUtc = hour,
            Reliability = reliability,
            Snr = Math.Round(reliability * 40 - 5, 1),
            MufMhz = Math.Round(muf, 1),
            Mode = "1F2",
            IsAboveMuf = freq > muf,
        };
    }

    // Sample MUF: low overnight, peaks mid-afternoon (UTC as a stand-in for local time).
    private static double Muf(int hour) =>
        7.0 + 18.0 * (0.5 * (1 + Math.Cos((hour - 14) / 24.0 * 2 * Math.PI)));

    private static double Reliability(double freq, int hour, double muf)
    {
        // Each band peaks at a different hour: low bands overnight, high bands midday.
        double peakHour = 2.0 + (freq - 3.5) / (30.0 - 3.5) * (14.0 - 2.0);
        double dist = CircularHourDistance(hour, peakHour);
        double proximity = Math.Max(0, 1 - dist / 8.0);

        double mufFactor = freq <= muf ? 1.0 : Math.Max(0, 1 - (freq - muf) / 10.0);
        return Math.Clamp(proximity * mufFactor, 0, 1);
    }

    private static double CircularHourDistance(double a, double b)
    {
        double d = Math.Abs(a - b) % 24.0;
        return Math.Min(d, 24.0 - d);
    }
}

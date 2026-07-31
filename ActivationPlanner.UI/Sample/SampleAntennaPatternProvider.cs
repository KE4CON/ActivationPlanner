using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Antennas;
using ActivationPlanner.PropagationModel.Gear;

namespace ActivationPlanner.UI.Sample;

/// <summary>
/// A stand-in <see cref="IAntennaPatternSource"/> that produces a representative elevation-plane
/// pattern until NEC2++ is configured. It is <b>not</b> a full model, but it is tied to the
/// antenna's real height: horizontal antennas show the ground-reflection lobing (higher antenna →
/// lower take-off angle), and verticals show a low-angle pattern — so the plot is genuinely
/// informative even in sample mode. The UI flags it as a modeled/representative pattern.
/// </summary>
public sealed class SampleAntennaPatternProvider : IAntennaPatternSource
{
    /// <summary>True — lets the UI show a "representative pattern" note.</summary>
    public bool IsSample => true;

    public Task<AntennaPattern> GetPatternAsync(AntennaProfile antenna, double frequencyMhz, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(antenna);

        bool vertical = antenna.Category is AntennaCategory.Vertical or AntennaCategory.Whip;
        bool nvis = antenna.Category is AntennaCategory.NvisCrossedDipole;
        double heightWl = Math.Max(0.05, Wavelength.InWavelengths(antenna.HeightFeet, frequencyMhz));
        double peakGainDbi = RepresentativePeakGain(antenna.Category);

        // Field magnitude vs elevation.
        var fields = new List<(double Elev, double Field)>();
        double maxField = 0;
        double peakElev = 0;
        for (double elev = 0; elev <= 90; elev += 2)
        {
            double theta = elev * Math.PI / 180.0;
            double field = nvis
                ? Math.Pow(Math.Sin(theta), 0.7)                    // overhead lobe (NVIS "cloud-warmer")
                : vertical
                    ? Math.Cos(theta)                               // low-angle radiator
                    : Math.Abs(Math.Sin(2 * Math.PI * heightWl * Math.Sin(theta))); // ground lobing
            fields.Add((elev, field));
            if (field > maxField)
            {
                maxField = field;
                peakElev = elev;
            }
        }

        if (maxField <= 0) maxField = 1;

        var samples = new List<AntennaPatternSample>(fields.Count);
        foreach (var (elev, field) in fields)
        {
            double rel = Math.Max(field / maxField, 1e-3);
            double gainDbi = peakGainDbi + 20 * Math.Log10(rel); // relative dB down from the peak
            samples.Add(new AntennaPatternSample(elev, gainDbi));
        }

        var pattern = new AntennaPattern
        {
            FrequencyMhz = frequencyMhz,
            PeakGainDbi = peakGainDbi,
            TakeoffAngleDeg = peakElev,
            FeedpointResistanceOhms = null,
            FeedpointReactanceOhms = null,
            Elevation = samples,
        };
        return Task.FromResult(pattern);
    }

    private static double RepresentativePeakGain(AntennaCategory category) => category switch
    {
        AntennaCategory.Dipole => 7.0,
        AntennaCategory.EndFedHalfWave => 5.5,
        AntennaCategory.Vertical => 1.5,
        AntennaCategory.Whip => 0.5,
        AntennaCategory.MagneticLoop => 2.0,
        AntennaCategory.NvisCrossedDipole => 2.0, // low over ground; the value is the high angle, not raw gain
        _ => 4.0,
    };
}

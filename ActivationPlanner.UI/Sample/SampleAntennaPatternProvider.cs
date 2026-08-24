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

        // Full azimuth x elevation grid for the 3D surface. Verticals/NVIS/loops are treated as
        // azimuthally symmetric (the elevation cut spun around); horizontal antennas get a soft
        // broadside figure-8 (max across the wire, shallow nulls along it) so the shape reads as a
        // dipole. Broadside (az = 90) equals the 2D cut, keeping the two views consistent.
        bool horizontal = !vertical && !nvis && antenna.Category is not AntennaCategory.MagneticLoop;
        var grid = new List<AntennaPatternGridSample>();
        double gridMax = 1e-6;
        var gridField = new List<(double Az, double Elev, double Field)>();
        for (double az = 0; az <= 360; az += 15)
        {
            double azFactor = horizontal ? 0.3 + 0.7 * Math.Abs(Math.Sin(az * Math.PI / 180.0)) : 1.0;
            foreach (var (elev, field) in fields)
            {
                if (((int)elev) % 5 != 0) continue; // coarser elevation for the grid
                double f = field * azFactor;
                gridField.Add((az, elev, f));
                if (f > gridMax) gridMax = f;
            }
        }
        foreach (var (az, elev, f) in gridField)
        {
            double rel = Math.Max(f / gridMax, 1e-3);
            grid.Add(new AntennaPatternGridSample(az, elev, peakGainDbi + 20 * Math.Log10(rel)));
        }

        var pattern = new AntennaPattern
        {
            FrequencyMhz = frequencyMhz,
            PeakGainDbi = peakGainDbi,
            TakeoffAngleDeg = peakElev,
            FeedpointResistanceOhms = null,
            FeedpointReactanceOhms = null,
            Elevation = samples,
            Grid = grid,
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

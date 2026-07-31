using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.ProcessEngine.Nec;

namespace ActivationPlanner.PropagationModel.Antennas;

/// <summary>
/// Builds a NEC2 geometry deck from an owned <see cref="AntennaProfile"/> at a given frequency —
/// the input side of Option B custom modeling. Translates the operator's physical description
/// (category, length, height, feed point, radials) into wires, a feed point, a ground model, and
/// an elevation-cut radiation-pattern request oriented to capture the antenna's main lobe.
/// <para>
/// Lengths convert feet → metres. Segment counts scale with electrical length (≈20 per
/// wavelength), and center-fed antennas get an odd segment count so the feed sits on the middle
/// segment. Magnetic loops and unclassified designs are not auto-generated (they need
/// hand-authored geometry) and throw <see cref="NotSupportedException"/>.
/// </para>
/// </summary>
public sealed class NecGeometryBuilder
{
    /// <summary>Default modeled wire radius (1 mm) — AntennaProfile does not carry conductor gauge.</summary>
    public const double DefaultWireRadiusMetres = 0.001;

    private const int SegmentsPerWavelength = 20;
    private const int MinSegments = 11;

    /// <summary>Average ground (relative permittivity 13, conductivity 0.005 S/m) — a sane default.</summary>
    public static NecGround AverageGround { get; } = new(Type: 2, DielectricConstant: 13.0, ConductivitySm: 0.005);

    /// <summary>Build the NEC deck for <paramref name="antenna"/> at <paramref name="frequencyMhz"/>.</summary>
    /// <exception cref="NotSupportedException">Geometry cannot be auto-generated for this antenna category.</exception>
    public NecGeometryInput Build(AntennaProfile antenna, double frequencyMhz, NecGround? ground = null)
    {
        ArgumentNullException.ThrowIfNull(antenna);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frequencyMhz);

        ground ??= AverageGround;
        double lengthWl = Wavelength.InWavelengths(antenna.LengthFeet, frequencyMhz);
        double lengthM = Wavelength.FeetToMetres(antenna.LengthFeet);
        double heightM = Wavelength.FeetToMetres(antenna.HeightFeet);

        return antenna.Category switch
        {
            AntennaCategory.Dipole => BuildHorizontal(antenna, frequencyMhz, ground, lengthM, heightM, lengthWl, centerFed: true),
            AntennaCategory.EndFedHalfWave => BuildHorizontal(antenna, frequencyMhz, ground, lengthM, heightM, lengthWl, centerFed: false),
            AntennaCategory.Vertical or AntennaCategory.Whip => BuildVertical(antenna, frequencyMhz, ground, lengthM, heightM, lengthWl),
            _ => throw new NotSupportedException(
                $"Automatic NEC geometry generation is not supported for a {antenna.Category} antenna; " +
                "it needs a hand-authored model."),
        };
    }

    private NecGeometryInput BuildHorizontal(
        AntennaProfile antenna, double freqMhz, NecGround ground,
        double lengthM, double heightM, double lengthWl, bool centerFed)
    {
        int segments = SegmentsFor(lengthWl, oddPreferred: centerFed);
        NecWire wire;
        int feedSegment;
        if (centerFed)
        {
            // Wire centered on the origin along X, fed at the middle segment.
            wire = new NecWire(1, segments, -lengthM / 2, 0, heightM, lengthM / 2, 0, heightM, DefaultWireRadiusMetres);
            feedSegment = (segments + 1) / 2;
        }
        else
        {
            // End-fed: wire runs from the origin, fed at the first segment.
            wire = new NecWire(1, segments, 0, 0, heightM, lengthM, 0, heightM, DefaultWireRadiusMetres);
            feedSegment = 1;
        }

        return new NecGeometryInput
        {
            Comments = [$"{antenna.Name} ({antenna.Category}) at {freqMhz:0.###} MHz"],
            Wires = [wire],
            FrequencyMhz = freqMhz,
            Ground = ground,
            Excitation = new NecExcitation(1, feedSegment),
            // Broadside elevation cut (phi=90) captures the main lobe of an X-oriented wire.
            RadiationPattern = new NecRadiationPattern(ThetaCount: 19, PhiCount: 1, ThetaStartDeg: 0, PhiStartDeg: 90, ThetaStepDeg: 5, PhiStepDeg: 0),
        };
    }

    private NecGeometryInput BuildVertical(
        AntennaProfile antenna, double freqMhz, NecGround ground,
        double lengthM, double heightM, double lengthWl)
    {
        int segments = SegmentsFor(lengthWl, oddPreferred: false);
        var wires = new List<NecWire>
        {
            // Vertical radiator from its base height upward, fed at the base segment.
            new(1, segments, 0, 0, heightM, 0, 0, heightM + lengthM, DefaultWireRadiusMetres),
        };
        wires.AddRange(BuildRadials(antenna, freqMhz, heightM));

        return new NecGeometryInput
        {
            Comments = [$"{antenna.Name} ({antenna.Category}) at {freqMhz:0.###} MHz"],
            Wires = wires,
            FrequencyMhz = freqMhz,
            Ground = ground,
            Excitation = new NecExcitation(1, 1),
            // Vertical is azimuthally symmetric; any phi cut represents the pattern.
            RadiationPattern = new NecRadiationPattern(ThetaCount: 19, PhiCount: 1, ThetaStartDeg: 0, PhiStartDeg: 0, ThetaStepDeg: 5, PhiStepDeg: 0),
        };
    }

    private static IEnumerable<NecWire> BuildRadials(AntennaProfile antenna, double freqMhz, double baseHeightM)
    {
        if (antenna.RadialCount is not { } count || count <= 0
            || antenna.RadialLengthFeet is not { } radialFeet || radialFeet <= 0)
        {
            yield break;
        }

        double radialM = Wavelength.FeetToMetres(radialFeet);
        double radialWl = Wavelength.InWavelengths(radialFeet, freqMhz);
        int radialSegs = SegmentsFor(radialWl, oddPreferred: false);

        for (int k = 0; k < count; k++)
        {
            double angle = 2 * Math.PI * k / count;
            double x = radialM * Math.Cos(angle);
            double y = radialM * Math.Sin(angle);
            // Radials fan out from the radiator base, sharing its base coordinate.
            yield return new NecWire(2 + k, radialSegs, 0, 0, baseHeightM, x, y, baseHeightM, DefaultWireRadiusMetres);
        }
    }

    private static int SegmentsFor(double lengthWl, bool oddPreferred)
    {
        int segments = Math.Max(MinSegments, (int)Math.Round(lengthWl * SegmentsPerWavelength));
        if (oddPreferred && segments % 2 == 0)
            segments++;
        return segments;
    }
}

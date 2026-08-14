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

    /// <summary>
    /// Minimum height above ground (5 cm) for any horizontal wire when a real ground is present.
    /// NEC-2 rejects a segment lying <i>in</i> the ground plane (z = 0) with a hard "GEOMETRY DATA
    /// ERROR" — so on-ground radials and zero-height dipoles get nudged up by this much. It is
    /// electrically negligible at HF (&lt;0.01λ) and only ever bites geometry that would otherwise
    /// be illegal. A vertical radiator's base endpoint may still sit at z = 0 (NEC images a
    /// monopole to ground), so this clearance applies to horizontal wires, not the radiator base.
    /// </summary>
    public const double GroundClearanceMetres = 0.05;

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

        // A radiator with no length produces a degenerate zero-length wire that NEC rejects. The
        // modeler substitutes a resonant length before we get here; guard so a direct/degenerate
        // call fails loudly with an actionable message instead of an opaque nec2c exit code.
        if (lengthM <= 0)
            throw new ArgumentException(
                "Antenna length must be greater than zero to generate geometry.", nameof(antenna));

        return antenna.Category switch
        {
            AntennaCategory.Dipole => BuildHorizontal(antenna, frequencyMhz, ground, lengthM, heightM, lengthWl, centerFed: true),
            AntennaCategory.EndFedHalfWave => BuildHorizontal(antenna, frequencyMhz, ground, lengthM, heightM, lengthWl, centerFed: false),
            AntennaCategory.Vertical or AntennaCategory.Whip => BuildVertical(antenna, frequencyMhz, ground, lengthM, heightM, lengthWl),
            AntennaCategory.NvisCrossedDipole => BuildNvisCrossedDipole(antenna, frequencyMhz, ground, lengthM, heightM, lengthWl),
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
        // Keep the horizontal wire out of the ground plane (NEC rejects z = 0). Only nudges a
        // zero/near-zero entered height; any real height passes through unchanged.
        double z = Math.Max(heightM, GroundClearanceMetres);
        NecWire wire;
        int feedSegment;
        if (centerFed)
        {
            // Wire centered on the origin along X, fed at the middle segment.
            wire = new NecWire(1, segments, -lengthM / 2, 0, z, lengthM / 2, 0, z, DefaultWireRadiusMetres);
            feedSegment = (segments + 1) / 2;
        }
        else
        {
            // End-fed: wire runs from the origin, fed at the first segment.
            wire = new NecWire(1, segments, 0, 0, z, lengthM, 0, z, DefaultWireRadiusMetres);
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

        // A bare monopole's base may sit at z = 0 (NEC images it to ground). But radials are
        // horizontal wires that would then lie in the ground plane — illegal. When radials are
        // present and the base is at/near ground, lift the whole assembly (base + radials) by the
        // ground clearance so they clear the plane and stay electrically connected at the feed.
        bool hasRadials = antenna.RadialCount is > 0 && antenna.RadialLengthFeet is > 0;

        // Elevated radials (a raised counterpoise): the radials and the feed point sit at a stated
        // height above ground, and the radiator rises from there. This is a distinct, common
        // portable configuration — it lowers the take-off angle and cuts ground loss versus
        // on-ground radials. When set, it drives the assembly base height (feed == radial ring).
        double? elevatedRadialM = hasRadials && antenna.RadialHeightFeet is { } rhFt && rhFt > 0
            ? Wavelength.FeetToMetres(rhFt)
            : null;
        double baseZ = elevatedRadialM is { } erM
            ? Math.Max(erM, GroundClearanceMetres)
            : hasRadials ? Math.Max(heightM, GroundClearanceMetres) : heightM;

        var wires = new List<NecWire>
        {
            // Vertical radiator from its base height upward, fed at the base segment.
            new(1, segments, 0, 0, baseZ, 0, 0, baseZ + lengthM, DefaultWireRadiusMetres),
        };
        wires.AddRange(BuildRadials(antenna, freqMhz, baseZ));

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

    private NecGeometryInput BuildNvisCrossedDipole(
        AntennaProfile antenna, double freqMhz, NecGround ground,
        double legLengthM, double apexHeightM, double legWl)
    {
        // Two dipoles crossed at 90°, apex-fed at the top of a short mast, each of the four legs
        // sloping down toward a ground stake (the AS-2259/GR geometry). The feed sits at the apex,
        // where all four legs meet; NEC drives the whole structure from a source on the first leg's
        // apex segment.
        double apexZ = Math.Max(apexHeightM, GroundClearanceMetres);
        double endZ = GroundClearanceMetres;
        double drop = apexZ - endZ;

        // Horizontal reach of each sloping leg (Pythagoras). If the leg is too short to reach the
        // ground from the apex, lay it out roughly horizontally at apex height instead of forcing a
        // degenerate near-vertical wire (which would make the four legs overlap).
        bool slopes = legLengthM > drop + 0.01;
        double horiz = slopes ? Math.Sqrt(legLengthM * legLengthM - drop * drop) : legLengthM;
        double tipZ = slopes ? endZ : apexZ;

        int segments = SegmentsFor(legWl, oddPreferred: false);
        var wires = new List<NecWire>
        {
            new(1, segments, 0, 0, apexZ,  horiz, 0, tipZ, DefaultWireRadiusMetres), // +X leg (fed)
            new(2, segments, 0, 0, apexZ, -horiz, 0, tipZ, DefaultWireRadiusMetres), // -X leg
            new(3, segments, 0, 0, apexZ, 0,  horiz, tipZ, DefaultWireRadiusMetres), // +Y leg
            new(4, segments, 0, 0, apexZ, 0, -horiz, tipZ, DefaultWireRadiusMetres), // -Y leg
        };

        return new NecGeometryInput
        {
            Comments = [$"{antenna.Name} (NVIS crossed dipole) at {freqMhz:0.###} MHz"],
            Wires = wires,
            FrequencyMhz = freqMhz,
            Ground = ground,
            // Fed at the apex, where the legs meet (segment 1 of the first leg).
            Excitation = new NecExcitation(1, 1),
            // Elevation cut; the crossed pair is near-symmetric in azimuth, so one phi captures the
            // NVIS high-angle lobe.
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

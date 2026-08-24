using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.ProcessEngine;
using ActivationPlanner.ProcessEngine.Nec;

namespace ActivationPlanner.PropagationModel.Antennas;

/// <summary>
/// The PropagationModel facade for Option B custom antenna modeling: takes an owned
/// <see cref="AntennaProfile"/> and a band frequency, runs it through NEC2 (via the mockable
/// <see cref="INecRunner"/>), and returns a domain <see cref="AntennaPattern"/>.
/// <para>
/// Layer 2 — composes the NEC runner (Layer 1) with geometry generation and result mapping. It
/// holds no process/file logic. This is what the Option A/B trigger (Phase 3) routes to when an
/// antenna needs custom modeling on a band; the resulting pattern (peak gain, take-off angle)
/// is what would feed a VOACAP antenna file in place of a library pattern.
/// </para>
/// </summary>
public sealed class NecAntennaModeler : IAntennaPatternSource
{
    private readonly INecRunner _runner;
    private readonly NecGeometryBuilder _builder;
    private readonly NecGround? _ground;

    public NecAntennaModeler(INecRunner runner, NecGeometryBuilder? builder = null, NecGround? ground = null)
    {
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        _builder = builder ?? new NecGeometryBuilder();
        _ground = ground;
    }

    /// <summary>
    /// Assemble a production modeler over a real nec2++/nec2c install from its executable path — a
    /// real <see cref="ProcessTransport"/> and <see cref="NecRunner"/> behind this facade. Lets the
    /// composition root (Layer 4) wire the real engine using only a path string, without naming any
    /// ProcessEngine (Layer 1) types itself.
    /// </summary>
    /// <param name="executablePath">Path to the nec2++ / nec2c executable.</param>
    public static NecAntennaModeler Create(string executablePath) =>
        new(new NecRunner(new ProcessTransport(), new NecRunnerOptions(executablePath)));

    /// <inheritdoc />
    public Task<AntennaPattern> GetPatternAsync(AntennaProfile antenna, double frequencyMhz, CancellationToken ct = default) =>
        ModelAsync(antenna, frequencyMhz, ct);

    /// <summary>Model <paramref name="antenna"/> at <paramref name="frequencyMhz"/> and return its pattern.</summary>
    /// <exception cref="NotSupportedException">Geometry cannot be auto-generated for this antenna.</exception>
    public async Task<AntennaPattern> ModelAsync(
        AntennaProfile antenna, double frequencyMhz, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(antenna);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frequencyMhz);

        // A resonant length is filled in when the operator left it blank (common for loaded/modular
        // verticals whose electrical length is not a fixed number). The pattern is then flagged as
        // an estimate so it is never read as an exact model of a specific radiator.
        (AntennaProfile modeled, string? estimateNote) = ResolveLength(antenna, frequencyMhz);

        NecGeometryInput geometry = _builder.Build(modeled, frequencyMhz, _ground);
        NecRawResult raw = await _runner.RunAsync(geometry, ct: ct).ConfigureAwait(false);
        return Map(raw, frequencyMhz, estimateNote);
    }

    /// <summary>
    /// If the antenna carries no usable length, substitute the resonant length for its category at
    /// this frequency (¼λ for a vertical/whip, ½λ for a dipole/end-fed half-wave) and describe the
    /// substitution. Antennas that already have a length pass through untouched with no note.
    /// </summary>
    private static (AntennaProfile Antenna, string? EstimateNote) ResolveLength(
        AntennaProfile antenna, double frequencyMhz)
    {
        if (antenna.LengthFeet > 0)
            return (antenna, null);

        double wavelengthFeet = Wavelength.Metres(frequencyMhz) / Wavelength.MetresPerFoot;
        (double fraction, string shape) = antenna.Category switch
        {
            AntennaCategory.Vertical or AntennaCategory.Whip => (0.25, "quarter-wave"),
            AntennaCategory.Dipole or AntennaCategory.EndFedHalfWave => (0.5, "half-wave"),
            // Each of the four legs is a quarter-wave, so a crossed pair forms two half-wave dipoles.
            AntennaCategory.NvisCrossedDipole => (0.25, "quarter-wave legs"),
            // Other categories have no simple resonant length; let the builder report what it does
            // and does not support rather than inventing geometry here.
            _ => (0.0, string.Empty),
        };

        if (fraction <= 0)
            return (antenna, null);

        double lengthFeet = fraction * wavelengthFeet;
        string note =
            $"Length not set — modeled as a resonant {shape} ({lengthFeet:0.#} ft) at {frequencyMhz:0.###} MHz.";
        return (antenna with { LengthFeet = lengthFeet }, note);
    }

    /// <summary>Map a raw NEC result into a domain pattern (theta-from-zenith → elevation).</summary>
    public static AntennaPattern Map(NecRawResult raw, double frequencyMhz, string? estimateNote = null)
    {
        ArgumentNullException.ThrowIfNull(raw);

        NecRadiationSample? peak = raw.PeakGain;

        // Full azimuth × elevation grid for the 3D surface — present only when the deck swept more
        // than one azimuth (a legacy single-cut result leaves Grid null and the 3D view falls back).
        int distinctPhi = raw.Pattern.Select(s => Math.Round(s.PhiDeg)).Distinct().Count();
        IReadOnlyList<AntennaPatternGridSample>? grid = distinctPhi > 1
            ? raw.Pattern
                .Select(s => new AntennaPatternGridSample(
                    AzimuthDeg: s.PhiDeg, ElevationAngleDeg: 90 - s.ThetaDeg, GainDbi: s.TotalGainDb))
                .ToList()
            : null;

        // The 2D elevation cut is taken at the peak-gain azimuth (the most meaningful slice); with a
        // single azimuth this is just the whole cut, matching the old behavior.
        double peakPhi = peak?.PhiDeg ?? 0;
        var elevation = raw.Pattern
            .Where(s => grid is null || Math.Abs(s.PhiDeg - peakPhi) < 0.5)
            .Select(s => new AntennaPatternSample(ElevationAngleDeg: 90 - s.ThetaDeg, GainDbi: s.TotalGainDb))
            .OrderBy(s => s.ElevationAngleDeg)
            .ToList();

        return new AntennaPattern
        {
            FrequencyMhz = frequencyMhz,
            PeakGainDbi = peak?.TotalGainDb ?? double.NegativeInfinity,
            TakeoffAngleDeg = peak is { } p ? 90 - p.ThetaDeg : 0,
            FeedpointResistanceOhms = raw.Impedance?.ResistanceOhms,
            FeedpointReactanceOhms = raw.Impedance?.ReactanceOhms,
            Elevation = elevation,
            Grid = grid,
            EstimateNote = estimateNote,
        };
    }
}

using ActivationPlanner.PropagationModel.Gear;
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
public sealed class NecAntennaModeler
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

    /// <summary>Model <paramref name="antenna"/> at <paramref name="frequencyMhz"/> and return its pattern.</summary>
    /// <exception cref="NotSupportedException">Geometry cannot be auto-generated for this antenna.</exception>
    public async Task<AntennaPattern> ModelAsync(
        AntennaProfile antenna, double frequencyMhz, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(antenna);
        NecGeometryInput geometry = _builder.Build(antenna, frequencyMhz, _ground);
        NecRawResult raw = await _runner.RunAsync(geometry, ct: ct).ConfigureAwait(false);
        return Map(raw, frequencyMhz);
    }

    /// <summary>Map a raw NEC result into a domain pattern (theta-from-zenith → elevation).</summary>
    public static AntennaPattern Map(NecRawResult raw, double frequencyMhz)
    {
        ArgumentNullException.ThrowIfNull(raw);

        var elevation = raw.Pattern
            .Select(s => new AntennaPatternSample(ElevationAngleDeg: 90 - s.ThetaDeg, GainDbi: s.TotalGainDb))
            .OrderBy(s => s.ElevationAngleDeg)
            .ToList();

        NecRadiationSample? peak = raw.PeakGain;

        return new AntennaPattern
        {
            FrequencyMhz = frequencyMhz,
            PeakGainDbi = peak?.TotalGainDb ?? double.NegativeInfinity,
            TakeoffAngleDeg = peak is { } p ? 90 - p.ThetaDeg : 0,
            FeedpointResistanceOhms = raw.Impedance?.ResistanceOhms,
            FeedpointReactanceOhms = raw.Impedance?.ReactanceOhms,
            Elevation = elevation,
        };
    }
}

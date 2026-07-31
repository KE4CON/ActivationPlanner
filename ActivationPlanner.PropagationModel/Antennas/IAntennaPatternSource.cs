using ActivationPlanner.PropagationModel.Gear;

namespace ActivationPlanner.PropagationModel.Antennas;

/// <summary>
/// Produces an antenna's radiation pattern for a band. Abstracts the NEC2-backed
/// <see cref="NecAntennaModeler"/> so the UI can plot patterns without referencing the shell-out —
/// and so a representative/offline source can stand in until NEC2++ is configured (mirrors the
/// propagation predictor pattern).
/// </summary>
public interface IAntennaPatternSource
{
    /// <summary>Get the modeled pattern for <paramref name="antenna"/> at <paramref name="frequencyMhz"/>.</summary>
    Task<AntennaPattern> GetPatternAsync(AntennaProfile antenna, double frequencyMhz, CancellationToken ct = default);
}

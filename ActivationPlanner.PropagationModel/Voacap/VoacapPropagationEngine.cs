using ActivationPlanner.ProcessEngine.Voacap;

namespace ActivationPlanner.PropagationModel.Voacap;

/// <summary>
/// The PropagationModel facade over the VOACAP shell-out: takes a planner
/// <see cref="CircuitQuery"/>, drives a prediction through the ProcessEngine runner, and
/// returns a domain <see cref="CircuitPrediction"/>.
/// <para>
/// This is Layer 2 — it composes the runner (Layer 1) with the mapper and exposes real
/// domain objects. It holds no process or file logic of its own; all of that lives behind
/// <see cref="IVoacapRunner"/>, which keeps this layer testable without a VOACAP install.
/// </para>
/// </summary>
public sealed class VoacapPropagationEngine : IPropagationPredictor
{
    private readonly IVoacapRunner _runner;
    private readonly VoacapCircuitMapper _mapper;

    public VoacapPropagationEngine(IVoacapRunner runner, VoacapCircuitMapper? mapper = null)
    {
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
        _mapper = mapper ?? new VoacapCircuitMapper();
    }

    /// <summary>Predict propagation for <paramref name="query"/> across its bands and hours.</summary>
    public async Task<CircuitPrediction> PredictAsync(CircuitQuery query, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        VoacapDeckInput deck = _mapper.ToDeck(query);
        VoacapRawPrediction raw = await _runner.RunAsync(deck, ct: ct).ConfigureAwait(false);
        return _mapper.ToPrediction(query, raw);
    }
}

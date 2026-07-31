namespace ActivationPlanner.PropagationModel.Voacap;

/// <summary>
/// Produces a propagation prediction for a circuit. Abstracts the VOACAP-backed
/// <see cref="VoacapPropagationEngine"/> so higher layers (Services, UI) can consume
/// predictions without referencing the ProcessEngine shell-out — and so a sample/offline
/// predictor can stand in until the user has configured their VOACAP install.
/// </summary>
public interface IPropagationPredictor
{
    /// <summary>Predict propagation for <paramref name="query"/> across its bands and hours.</summary>
    Task<CircuitPrediction> PredictAsync(CircuitQuery query, CancellationToken ct = default);
}

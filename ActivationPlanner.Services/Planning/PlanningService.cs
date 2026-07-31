using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.Services.Antennas;

namespace ActivationPlanner.Services.Planning;

/// <summary>
/// The core planning orchestration: runs a propagation prediction and matches each band
/// against the operator's owned antennas (Option A/B), producing a ranked
/// <see cref="SessionPlan"/> for the planning screen.
/// <para>
/// Layer-3 service. Depends only on the PropagationModel prediction abstraction
/// (<see cref="IPropagationPredictor"/>) and the peer antenna evaluator — no VOACAP/NEC2++
/// shell-out and no UI. The predictor may be the real VOACAP engine or a sample stand-in;
/// this service neither knows nor cares.
/// </para>
/// </summary>
public sealed class PlanningService
{
    private readonly IPropagationPredictor _predictor;
    private readonly AntennaModelingEvaluator _antennaEvaluator;

    public PlanningService(IPropagationPredictor predictor, AntennaModelingEvaluator? antennaEvaluator = null)
    {
        _predictor = predictor ?? throw new ArgumentNullException(nameof(predictor));
        _antennaEvaluator = antennaEvaluator ?? new AntennaModelingEvaluator();
    }

    /// <summary>
    /// Build a session plan for <paramref name="query"/>, matching each band against
    /// <paramref name="ownedAntennas"/>. Bands are returned ranked best-first; within a band,
    /// antennas are ordered library-ready first (owned-inventory only).
    /// </summary>
    public async Task<SessionPlan> PlanAsync(
        CircuitQuery query,
        IReadOnlyList<AntennaProfile> ownedAntennas,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(ownedAntennas);

        CircuitPrediction prediction = await _predictor.PredictAsync(query, ct).ConfigureAwait(false);

        var recommendations = new List<BandRecommendation>(prediction.Bands.Count);
        foreach (BandPrediction bandPrediction in prediction.Bands)
        {
            IReadOnlyList<AntennaSuggestion> suggestions = ownedAntennas
                .Select(a => new AntennaSuggestion
                {
                    Antenna = a,
                    Modeling = _antennaEvaluator.Evaluate(a, bandPrediction.FrequencyMhz),
                })
                // Owned antennas only; library-ready (Option A) shown before those needing modeling.
                .OrderByDescending(s => s.IsLibraryReady)
                .ThenBy(s => s.Antenna.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            recommendations.Add(new BandRecommendation
            {
                Band = bandPrediction.Band,
                FrequencyMhz = bandPrediction.FrequencyMhz,
                Prediction = bandPrediction,
                OwnedAntennas = suggestions,
            });
        }

        IReadOnlyList<BandRecommendation> ranked = recommendations
            .OrderByDescending(b => b.AverageReliability)
            .ThenBy(b => b.FrequencyMhz)
            .ToList();

        return new SessionPlan
        {
            Bands = ranked,
            HoursUtc = prediction.HoursUtc,
            DistanceKm = prediction.DistanceKm,
            TransmitterLabel = prediction.TransmitterLabel,
            ReceiverLabel = prediction.ReceiverLabel,
        };
    }
}

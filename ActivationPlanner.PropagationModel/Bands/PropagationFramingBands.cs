using ActivationPlanner.PropagationModel.Missions;

namespace ActivationPlanner.PropagationModel.Bands;

/// <summary>
/// The bands worth predicting for a given propagation framing. Regional/NVIS coverage lives on
/// the lower HF bands (near-vertical incidence), so that framing asks VOACAP about the low
/// bands; DX framing asks about the full HF set. This changes the <i>question</i> (which bands
/// are of interest), not the predicted answer for any band — the operator can still choose bands
/// explicitly.
/// </summary>
public static class PropagationFramingBands
{
    /// <summary>Low bands used for near-vertical incidence (regional / NVIS) coverage.</summary>
    public static IReadOnlyList<HamBand> Nvis { get; } =
        [HamBand.M80, HamBand.M60, HamBand.M40, HamBand.M30];

    /// <summary>Default band set for a framing: the NVIS low bands for regional, all HF for DX.</summary>
    public static IReadOnlyList<HamBand> For(PropagationFraming framing) => framing switch
    {
        PropagationFraming.RegionalNvis => Nvis,
        _ => HamBands.All,
    };
}

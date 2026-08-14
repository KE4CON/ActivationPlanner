using System.Collections.Generic;
using ActivationPlanner.PropagationModel.Bands;

namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// Band-plan reference tab: a plain-language guide to US amateur privileges (FCC Part 97) — which
/// license classes may operate where, and in what modes. Static reference data; nothing to fetch.
/// </summary>
public sealed class BandPlanViewModel : ViewModelBase
{
    public IReadOnlyList<BandPlanBand> Bands => UsBandPlan.Bands;
}

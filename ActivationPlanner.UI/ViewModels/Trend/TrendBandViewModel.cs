using System.Collections.Generic;

namespace ActivationPlanner.UI.ViewModels.Trend;

/// <summary>A band's reliability trend across the recent samples (oldest → newest). Immutable.</summary>
public sealed class TrendBandViewModel
{
    public TrendBandViewModel(string bandName, IReadOnlyList<TrendCellViewModel> cells, string latestLabel)
    {
        BandName = bandName;
        Cells = cells;
        LatestLabel = latestLabel;
    }

    public string BandName { get; }
    public IReadOnlyList<TrendCellViewModel> Cells { get; }

    /// <summary>The most recent reliability as a short label, e.g. "72%".</summary>
    public string LatestLabel { get; }
}

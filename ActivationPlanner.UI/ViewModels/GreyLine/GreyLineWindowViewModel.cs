using System.Collections.Generic;
using System.Linq;
using ActivationPlanner.Services.GreyLine;

namespace ActivationPlanner.UI.ViewModels.GreyLine;

/// <summary>One grey-line window (sunrise or sunset) with its ranked bands. Immutable.</summary>
public sealed class GreyLineWindowViewModel
{
    public GreyLineWindowViewModel(string title, GreyLineWindow window)
    {
        Title = title;
        Bands = window.Bands.Select(b => new GreyLineBandViewModel(b)).ToList();
        FavorableCount = Bands.Count(b => b.IsFavorable);
        Summary = FavorableCount > 0
            ? $"{FavorableCount} band(s) favorable"
            : "no strong openings at this hour";
    }

    public string Title { get; }
    public string Summary { get; }
    public int FavorableCount { get; }
    public IReadOnlyList<GreyLineBandViewModel> Bands { get; }
}

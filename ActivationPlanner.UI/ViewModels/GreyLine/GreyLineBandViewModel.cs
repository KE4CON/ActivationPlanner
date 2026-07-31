using System.Globalization;
using Avalonia.Media;
using ActivationPlanner.Services.GreyLine;
using ActivationPlanner.UI.ViewModels.Planning;

namespace ActivationPlanner.UI.ViewModels.GreyLine;

/// <summary>A band's reliability at a grey-line hour, for display. Immutable.</summary>
public sealed class GreyLineBandViewModel
{
    /// <summary>Reliability at/above this fraction is called out as a favorable grey-line opening.</summary>
    private const double FavorableThreshold = 0.5;

    public GreyLineBandViewModel(GreyLineBand band)
    {
        BandName = band.BandName;
        FrequencyLabel = band.FrequencyMhz.ToString("0.0##", CultureInfo.InvariantCulture) + " MHz";
        ReliabilityLabel = band.Reliability is { } r ? $"{r * 100:0}%" : "—";
        CellBrush = ReliabilityPalette.Brush(band.Reliability);
        IsFavorable = band.Reliability is { } rel && rel >= FavorableThreshold;
    }

    public string BandName { get; }
    public string FrequencyLabel { get; }
    public string ReliabilityLabel { get; }
    public IBrush CellBrush { get; }

    /// <summary>True when this band is worth working during the grey line (reliable enough).</summary>
    public bool IsFavorable { get; }
}

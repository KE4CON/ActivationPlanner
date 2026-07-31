using System.Globalization;
using Avalonia.Media;
using ActivationPlanner.UI.ViewModels.Planning;

namespace ActivationPlanner.UI.ViewModels.Trend;

/// <summary>One band's reliability at one sample time in the trend strip. Immutable.</summary>
public sealed class TrendCellViewModel
{
    public TrendCellViewModel(double? reliability, System.DateTime capturedAtUtc)
    {
        CellBrush = ReliabilityPalette.Brush(reliability);
        string time = capturedAtUtc.ToString("HH:mm", CultureInfo.InvariantCulture);
        Tooltip = reliability is { } r
            ? $"{time} UTC — {r * 100:0}% reliable"
            : $"{time} UTC — no prediction";
    }

    public IBrush CellBrush { get; }
    public string Tooltip { get; }
}

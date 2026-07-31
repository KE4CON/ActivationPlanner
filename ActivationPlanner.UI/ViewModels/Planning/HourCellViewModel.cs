using Avalonia.Media;

namespace ActivationPlanner.UI.ViewModels.Planning;

/// <summary>
/// One cell in a band's 24-hour reliability strip: a colour for the hour plus a tooltip
/// with the underlying figures. Immutable display data.
/// </summary>
public sealed class HourCellViewModel
{
    public HourCellViewModel(int hourUtc, double? reliability, double? mufMhz)
    {
        HourUtc = hourUtc;
        Reliability = reliability;
        CellBrush = ReliabilityPalette.Brush(reliability);
        Tooltip = reliability is { } r
            ? $"{hourUtc:00}:00 UTC — {r * 100:0}% reliable" + (mufMhz is { } m ? $", MUF {m:0.0} MHz" : "")
            : $"{hourUtc:00}:00 UTC — no prediction";
    }

    /// <summary>UTC hour, 0-24.</summary>
    public int HourUtc { get; }

    /// <summary>Reliability 0-1, or null if the band was not evaluated this hour.</summary>
    public double? Reliability { get; }

    /// <summary>Fill colour for the cell.</summary>
    public IBrush CellBrush { get; }

    /// <summary>Hover text with the hour's figures.</summary>
    public string Tooltip { get; }
}

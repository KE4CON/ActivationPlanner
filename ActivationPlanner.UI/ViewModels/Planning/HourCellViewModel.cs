using Avalonia.Media;

namespace ActivationPlanner.UI.ViewModels.Planning;

/// <summary>
/// One cell in a band's 24-hour reliability strip: a colour for the hour plus a tooltip
/// with the underlying figures. Immutable display data.
/// </summary>
public sealed class HourCellViewModel
{
    public HourCellViewModel(int hourUtc, double? reliability, double? mufMhz, bool isGreyLine = false)
    {
        HourUtc = hourUtc;
        Reliability = reliability;
        IsGreyLine = isGreyLine;
        CellBrush = ReliabilityPalette.Brush(reliability);
        Tooltip = reliability is { } r
            ? $"{hourUtc:00}:00 UTC — {r * 100:0}% reliable" + (mufMhz is { } m ? $", MUF {m:0.0} MHz" : "")
            : $"{hourUtc:00}:00 UTC — no prediction";
        if (isGreyLine)
            Tooltip += " • grey line";

        // Sparse ruler: label every 6th hour so the strip is readable without hovering.
        AxisLabel = hourUtc % 6 == 0 ? hourUtc.ToString("00") : string.Empty;
    }

    /// <summary>True when this hour falls in the sunrise/sunset grey-line window.</summary>
    public bool IsGreyLine { get; }

    /// <summary>Hour label shown under the strip at 6-hour intervals; empty otherwise.</summary>
    public string AxisLabel { get; }

    /// <summary>UTC hour, 0-24.</summary>
    public int HourUtc { get; }

    /// <summary>Reliability 0-1, or null if the band was not evaluated this hour.</summary>
    public double? Reliability { get; }

    /// <summary>Fill colour for the cell.</summary>
    public IBrush CellBrush { get; }

    /// <summary>Hover text with the hour's figures.</summary>
    public string Tooltip { get; }
}

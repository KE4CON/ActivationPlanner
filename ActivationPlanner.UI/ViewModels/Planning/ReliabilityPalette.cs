using System;
using Avalonia.Media;

namespace ActivationPlanner.UI.ViewModels.Planning;

/// <summary>
/// Maps a reliability figure (0-1) to a colour for the planning heatmap and badges:
/// red (poor) → amber (marginal) → green (good). Frequency slots VOACAP did not evaluate
/// map to a neutral "no data" grey.
/// </summary>
internal static class ReliabilityPalette
{
    private static readonly Color Poor = Color.FromRgb(0xC0, 0x3A, 0x3A);      // red
    private static readonly Color Marginal = Color.FromRgb(0xC9, 0x9A, 0x2B);  // amber
    private static readonly Color Good = Color.FromRgb(0x3A, 0xA0, 0x50);      // green
    private static readonly Color NoData = Color.FromRgb(0x45, 0x45, 0x48);    // grey

    /// <summary>A brush for a reliability value; grey when <paramref name="reliability"/> is null.</summary>
    public static IBrush Brush(double? reliability)
    {
        if (reliability is not { } r)
            return new SolidColorBrush(NoData);

        r = Math.Clamp(r, 0, 1);
        Color c = r < 0.5
            ? Lerp(Poor, Marginal, r / 0.5)
            : Lerp(Marginal, Good, (r - 0.5) / 0.5);
        return new SolidColorBrush(c);
    }

    private static Color Lerp(Color a, Color b, double t) => Color.FromRgb(
        (byte)Math.Round(a.R + (b.R - a.R) * t),
        (byte)Math.Round(a.G + (b.G - a.G) * t),
        (byte)Math.Round(a.B + (b.B - a.B) * t));
}

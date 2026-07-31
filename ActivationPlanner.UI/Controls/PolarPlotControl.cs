using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ActivationPlanner.PropagationModel.Antennas;

namespace ActivationPlanner.UI.Controls;

/// <summary>
/// Renders an antenna's elevation-plane radiation pattern as a polar "dome" plot (horizon to
/// horizon over the top), with dB rings, elevation-angle spokes, and the main-lobe filled. Radius
/// is a normalized dB scale (peak at the outer ring, down to −<see cref="DynamicRangeDb"/> at the
/// centre). Skia rendering — no external plotting/3D dependency.
/// </summary>
public sealed class PolarPlotControl : Control
{
    private const double DynamicRangeDb = 30.0;

    public static readonly StyledProperty<AntennaPattern?> PatternProperty =
        AvaloniaProperty.Register<PolarPlotControl, AntennaPattern?>(nameof(Pattern));

    private static readonly IBrush LobeFill = new SolidColorBrush(Color.FromArgb(0x55, 0x33, 0xD6, 0xFF));
    private static readonly Pen LobePen = new(new SolidColorBrush(Color.FromRgb(0x33, 0xD6, 0xFF)), 2)
    {
        LineJoin = PenLineJoin.Round,
    };
    private static readonly Pen GridPen = new(new SolidColorBrush(Color.FromArgb(0x66, 0x88, 0x88, 0x88)), 0.8);
    private static readonly IBrush LabelBrush = new SolidColorBrush(Color.FromArgb(0xC0, 0x99, 0x99, 0x99));
    private static readonly Typeface LabelFace = new("Inter");

    static PolarPlotControl()
    {
        AffectsRender<PolarPlotControl>(PatternProperty);
    }

    public AntennaPattern? Pattern
    {
        get => GetValue(PatternProperty);
        set => SetValue(PatternProperty, value);
    }

    public override void Render(DrawingContext context)
    {
        double w = Bounds.Width, h = Bounds.Height;
        if (w <= 0 || h <= 0)
            return;

        const double margin = 28;
        double cx = w / 2;
        double cy = h - margin;
        double radius = Math.Min(w / 2 - margin, h - 2 * margin);
        if (radius <= 0)
            return;

        Point Polar(double halfAngleDeg, double r)
        {
            double a = halfAngleDeg * Math.PI / 180.0;
            return new Point(cx + r * Math.Cos(a), cy - r * Math.Sin(a));
        }

        // dB rings (dome half-circles) + their labels.
        var pattern = Pattern;
        double peak = pattern?.PeakGainDbi ?? 0;
        foreach (double frac in new[] { 0.25, 0.5, 0.75, 1.0 })
            DrawArc(context, GridPen, cx, cy, radius * frac);

        // Elevation spokes at 0/30/60/90°.
        foreach (double elev in new double[] { 0, 30, 60, 90 })
        {
            context.DrawLine(GridPen, new Point(cx, cy), Polar(elev, radius));           // right side
            if (elev is not 90)
                context.DrawLine(GridPen, new Point(cx, cy), Polar(180 - elev, radius)); // left side
            DrawLabel(context, $"{elev:0}°", Polar(elev, radius + 12));
        }
        context.DrawLine(GridPen, new Point(cx - radius, cy), new Point(cx + radius, cy)); // horizon

        if (pattern is null || pattern.Elevation.Count == 0)
        {
            DrawLabel(context, "No pattern", new Point(cx - 24, cy - radius / 2));
            return;
        }

        // Build the lobe outline: right side elevation 0→90, then left side 90→0.
        double Norm(double gainDbi) => Math.Clamp((gainDbi - (peak - DynamicRangeDb)) / DynamicRangeDb, 0, 1);

        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            var samples = pattern.Elevation;
            ctx.BeginFigure(Polar(samples[0].ElevationAngleDeg, radius * Norm(samples[0].GainDbi)), isFilled: true);
            foreach (var s in samples)
                ctx.LineTo(Polar(s.ElevationAngleDeg, radius * Norm(s.GainDbi)));
            for (int i = samples.Count - 1; i >= 0; i--)
                ctx.LineTo(Polar(180 - samples[i].ElevationAngleDeg, radius * Norm(samples[i].GainDbi)));
            ctx.EndFigure(true);
        }
        context.DrawGeometry(LobeFill, LobePen, geo);

        // Peak gain / take-off annotation.
        DrawLabel(context, $"Peak {pattern.PeakGainDbi:0.0} dBi @ {pattern.TakeoffAngleDeg:0}° elevation",
            new Point(8, 8));
    }

    private static void DrawArc(DrawingContext context, IPen pen, double cx, double cy, double r)
    {
        // Approximate the dome half-circle (0°..180°) with a polyline.
        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            ctx.BeginFigure(new Point(cx + r, cy), isFilled: false);
            for (int deg = 5; deg <= 180; deg += 5)
            {
                double a = deg * Math.PI / 180.0;
                ctx.LineTo(new Point(cx + r * Math.Cos(a), cy - r * Math.Sin(a)));
            }
            ctx.EndFigure(false);
        }
        context.DrawGeometry(null, pen, geo);
    }

    private static void DrawLabel(DrawingContext context, string text, Point origin)
    {
        var formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            LabelFace, 11, LabelBrush);
        context.DrawText(formatted, origin);
    }
}

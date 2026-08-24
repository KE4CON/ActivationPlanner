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

    // EZNEC-style elevation plot: red pattern trace on a labeled polar grid (dB rings + angle spokes).
    private static readonly IBrush LobeFill = new SolidColorBrush(Color.FromArgb(0x38, 0xE0, 0x3A, 0x2A));
    private static readonly Pen LobePen = new(new SolidColorBrush(Color.FromRgb(0xD6, 0x2A, 0x1E)), 2.2)
    {
        LineJoin = PenLineJoin.Round,
    };
    private static readonly Pen OuterPen = new(new SolidColorBrush(Color.FromArgb(0xB0, 0x70, 0x78, 0x84)), 1.2);
    private static readonly Pen GridPen = new(new SolidColorBrush(Color.FromArgb(0x66, 0x80, 0x88, 0x92)), 0.8);
    private static readonly IBrush LabelBrush = new SolidColorBrush(Color.FromRgb(0x5A, 0x63, 0x6E));
    private static readonly IBrush DbLabelBrush = new SolidColorBrush(Color.FromRgb(0x5A, 0x63, 0x6E));
    // Take-off angle marker (the elevation of peak gain) — a distinct green radial line, EZNEC-style.
    private static readonly IBrush TakeoffBrush = new SolidColorBrush(Color.FromRgb(0x1B, 0x9E, 0x57));
    private static readonly Pen TakeoffPen = new(new SolidColorBrush(Color.FromRgb(0x1B, 0x9E, 0x57)), 1.8)
    {
        DashStyle = new DashStyle(new double[] { 4, 3 }, 0),
    };
    private static readonly Typeface LabelFace = new("Inter", FontStyle.Normal, FontWeight.Bold);

    // dB rings shown, from the outer ring inward (0 dB at the edge to the center).
    private static readonly double[] RingDb = { 0, -10, -20 };

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

        var pattern = Pattern;

        // dB rings at 0 / -10 / -20 dB (outer to inner), EZNEC-style, each labeled up the vertical.
        DrawArc(context, OuterPen, cx, cy, radius);
        // dB ring labels sit just to the RIGHT of the vertical axis so they clear the apex "90°" label.
        foreach (double db in RingDb)
        {
            double frac = (DynamicRangeDb + db) / DynamicRangeDb; // 0 dB -> outer, -30 dB -> center
            if (db != 0) DrawArc(context, GridPen, cx, cy, radius * frac);
            string tag = db == 0 ? "0 dB" : $"{db:0}";
            DrawLabel(context, tag, new Point(cx + 7, cy - radius * frac - 4), DbLabelBrush);
        }
        DrawLabel(context, "-30", new Point(cx + 7, cy - 4), DbLabelBrush); // center

        // Elevation spokes + angle labels on both sides (0 at each horizon, 90 at the top). The apex
        // "90°" is nudged up-left so it doesn't collide with the "0 dB" ring label on the right.
        foreach (double elev in new double[] { 0, 30, 60, 90 })
        {
            context.DrawLine(GridPen, new Point(cx, cy), Polar(elev, radius));           // right side
            Point topLabel = elev is 90 ? new Point(cx - 30, cy - radius - 4) : Polar(elev, radius + 12);
            DrawLabel(context, $"{elev:0}°", topLabel);
            if (elev is not 90)
            {
                context.DrawLine(GridPen, new Point(cx, cy), Polar(180 - elev, radius)); // left side
                DrawLabel(context, $"{elev:0}°", Polar(180 - elev, radius + 12));
            }
        }
        context.DrawLine(OuterPen, new Point(cx - radius, cy), new Point(cx + radius, cy)); // horizon
        double peak = pattern?.PeakGainDbi ?? 0;

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

        // Distinct take-off angle line: a green dashed radial at the elevation of peak gain (EZNEC-style).
        // The label is pushed further out (and, near the apex, offset right) to avoid the top labels.
        double takeoff = pattern.TakeoffAngleDeg;
        context.DrawLine(TakeoffPen, new Point(cx, cy), Polar(takeoff, radius));
        Point takeoffLabel =
            takeoff >= 80 ? new Point(cx + 34, cy - radius - 4)          // near the apex: up and right
            : takeoff <= 15 ? new Point(cx + radius * 0.55, cy - 20)      // near the horizon: inset + above the line
            : Polar(takeoff, radius + 22);                                // mid angles: just outside the ring
        DrawLabel(context, $"take-off {takeoff:0}°", takeoffLabel, TakeoffBrush);

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

    private static void DrawLabel(DrawingContext context, string text, Point origin, IBrush? brush = null)
    {
        var formatted = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            LabelFace, 12, brush ?? LabelBrush);
        context.DrawText(formatted, origin);
    }
}

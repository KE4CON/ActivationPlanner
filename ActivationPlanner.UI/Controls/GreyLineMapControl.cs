using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using ActivationPlanner.PropagationModel.Geo;

namespace ActivationPlanner.UI.Controls;

/// <summary>
/// An equirectangular world map with the live day/night terminator (the grey line) drawn on it:
/// continents, shaded night side, the terminator curve, a graticule, the sub-solar point (sun),
/// and the operator's QTH. The terminator advances in real time (re-rendered each minute).
/// Rendered entirely with Avalonia's drawing (Skia) — no external map/3D dependency.
/// </summary>
public sealed class GreyLineMapControl : Control
{
    public static readonly StyledProperty<double> QthLatitudeProperty =
        AvaloniaProperty.Register<GreyLineMapControl, double>(nameof(QthLatitude), double.NaN);

    public static readonly StyledProperty<double> QthLongitudeProperty =
        AvaloniaProperty.Register<GreyLineMapControl, double>(nameof(QthLongitude), double.NaN);

    private static readonly IBrush OceanBrush = new SolidColorBrush(Color.FromRgb(0x10, 0x22, 0x33));
    private static readonly IBrush LandBrush = new SolidColorBrush(Color.FromRgb(0xC2, 0xA8, 0x78)); // tan / soil
    private static readonly IBrush NightBrush = new SolidColorBrush(Color.FromArgb(0x8C, 0x04, 0x08, 0x18));
    private static readonly IBrush SunBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xD2, 0x4D));
    private static readonly IBrush QthBrush = new SolidColorBrush(Color.FromRgb(0x33, 0xD6, 0xFF));
    // Bright magenta contrasts strongly with the tan land and the dark ocean/night, and is
    // distinct from the yellow sun and cyan station markers.
    private static readonly Pen TerminatorPen = new(new SolidColorBrush(Color.FromRgb(0xFF, 0x2C, 0xB5)), 3.4)
    {
        LineCap = PenLineCap.Round,
        LineJoin = PenLineJoin.Round,
    };
    private static readonly Pen GraticulePen = new(new SolidColorBrush(Color.FromArgb(0x26, 0xFF, 0xFF, 0xFF)), 0.6);
    private static readonly Pen EquatorPen = new(new SolidColorBrush(Color.FromArgb(0x40, 0xFF, 0xFF, 0xFF)), 0.8);
    private static readonly Pen QthPen = new(new SolidColorBrush(Color.FromRgb(0x33, 0xD6, 0xFF)), 2);

    private readonly DispatcherTimer _timer;

    static GreyLineMapControl()
    {
        AffectsRender<GreyLineMapControl>(QthLatitudeProperty, QthLongitudeProperty);
    }

    public GreyLineMapControl()
    {
        // Advance the terminator once a minute.
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        _timer.Tick += (_, _) => InvalidateVisual();
    }

    public double QthLatitude
    {
        get => GetValue(QthLatitudeProperty);
        set => SetValue(QthLatitudeProperty, value);
    }

    public double QthLongitude
    {
        get => GetValue(QthLongitudeProperty);
        set => SetValue(QthLongitudeProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _timer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }

    public override void Render(DrawingContext context)
    {
        double w = Bounds.Width, h = Bounds.Height;
        if (w <= 0 || h <= 0)
            return;

        var now = DateTime.UtcNow;

        double X(double lon) => (lon + 180.0) / 360.0 * w;
        double Y(double lat) => (90.0 - lat) / 180.0 * h;

        // Ocean + land.
        context.FillRectangle(OceanBrush, new Rect(0, 0, w, h));
        foreach (var ring in WorldMapData.Land)
            DrawRing(context, ring, X, Y);

        // Night shading (per longitude column).
        DrawNight(context, now, w, h, X, Y);

        // Graticule.
        for (double lon = -150; lon <= 150; lon += 30)
            context.DrawLine(GraticulePen, new Point(X(lon), 0), new Point(X(lon), h));
        for (double lat = -60; lat <= 60; lat += 30)
            context.DrawLine(lat == 0 ? EquatorPen : GraticulePen, new Point(0, Y(lat)), new Point(w, Y(lat)));

        // Terminator curve.
        DrawTerminator(context, now, w, X, Y);

        // Sun (sub-solar point).
        GeoLocation sun = SolarCalculator.SubsolarPoint(now);
        context.DrawEllipse(SunBrush, null, new Point(X(sun.LongitudeDeg), Y(sun.LatitudeDeg)), 5, 5);

        // QTH.
        if (!double.IsNaN(QthLatitude) && !double.IsNaN(QthLongitude))
        {
            var p = new Point(X(SolarCalculator.NormalizeLongitude(QthLongitude)), Y(QthLatitude));
            context.DrawEllipse(QthBrush, null, p, 3, 3);
            context.DrawEllipse(null, QthPen, p, 7, 7);
        }
    }

    private static void DrawRing(DrawingContext context, MapRing ring, Func<double, double> x, Func<double, double> y)
    {
        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            var first = ring.Points[0];
            ctx.BeginFigure(new Point(x(first.Lon), y(first.Lat)), isFilled: true);
            for (int i = 1; i < ring.Points.Count; i++)
                ctx.LineTo(new Point(x(ring.Points[i].Lon), y(ring.Points[i].Lat)));
            ctx.EndFigure(true);
        }
        context.DrawGeometry(LandBrush, null, geo);
    }

    // Shade the night side by filling, per 2° longitude column, the latitude band in darkness.
    private void DrawNight(DrawingContext context, DateTime utc, double w, double h,
        Func<double, double> x, Func<double, double> y)
    {
        const double step = 2.0;
        for (double lon = -180; lon < 180; lon += step)
        {
            double? crossing = TerminatorCrossing(lon, utc);
            double left = x(lon), right = x(lon + step);

            if (crossing is null)
            {
                // Uniform column: all night or all day.
                if (Terminator.IsNight(new GeoLocation(0, Clamp180(lon)), utc))
                    context.FillRectangle(NightBrush, new Rect(left, 0, right - left, h));
                continue;
            }

            bool northIsNight = Terminator.IsNight(new GeoLocation(89.9, Clamp180(lon)), utc);
            double top = northIsNight ? y(90) : y(crossing.Value);
            double bottom = northIsNight ? y(crossing.Value) : y(-90);
            context.FillRectangle(NightBrush, new Rect(left, top, right - left, bottom - top));
        }
    }

    private void DrawTerminator(DrawingContext context, DateTime utc, double w, Func<double, double> x, Func<double, double> y)
    {
        const double step = 2.0;
        bool drawing = false;
        Point previous = default;
        for (double lon = -180; lon <= 180; lon += step)
        {
            double? crossing = TerminatorCrossing(lon, utc);
            if (crossing is null)
            {
                drawing = false;
                continue;
            }

            var point = new Point(x(lon), y(crossing.Value));
            if (drawing)
                context.DrawLine(TerminatorPen, previous, point);
            previous = point;
            drawing = true;
        }
    }

    /// <summary>Latitude where the terminator crosses this meridian, or null if the whole column is day or night.</summary>
    private static double? TerminatorCrossing(double lon, DateTime utc)
    {
        double clon = Clamp180(lon);
        bool prev = Terminator.IsNight(new GeoLocation(90, clon), utc);
        for (double lat = 89; lat >= -90; lat -= 1)
        {
            bool cur = Terminator.IsNight(new GeoLocation(lat, clon), utc);
            if (cur != prev)
                return lat + 0.5;
            prev = cur;
        }
        return null;
    }

    private static double Clamp180(double lon) => lon >= 180 ? 179.999 : lon;
}

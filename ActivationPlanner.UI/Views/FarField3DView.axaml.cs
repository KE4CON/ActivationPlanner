using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Ab4d.SharpEngine.AvaloniaUI;
using Ab4d.SharpEngine.Cameras;
using Ab4d.SharpEngine.Common;
using Ab4d.SharpEngine.Lights;
using Ab4d.SharpEngine.Materials;
using Ab4d.SharpEngine.Meshes;
using Ab4d.SharpEngine.SceneNodes;
using Ab4d.SharpEngine.Utilities;
using ActivationPlanner.PropagationModel.Antennas;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ActivationPlanner.UI.Views;

/// <summary>
/// A 3D far-field radiation-pattern surface, rendered with Ab4d.SharpEngine (Vulkan). The surface is
/// built from the antenna pattern's azimuth × elevation gain grid: distance from the center is the
/// normalized gain in that direction, and the color maps gain from blue (weak) to red (strong). If
/// no Vulkan GPU is present, it raises <see cref="GpuUnavailable"/> so the host can fall back to the
/// 2D polar plot; if the pattern has no grid it shows a short note.
/// </summary>
public partial class FarField3DView : UserControl
{
    /// <summary>The pattern to render; its <see cref="AntennaPattern.Grid"/> drives the surface.</summary>
    public static readonly StyledProperty<AntennaPattern?> PatternProperty =
        AvaloniaProperty.Register<FarField3DView, AntennaPattern?>(nameof(Pattern));

    public AntennaPattern? Pattern
    {
        get => GetValue(PatternProperty);
        set => SetValue(PatternProperty, value);
    }

    /// <summary>Raised once if the 3D engine can't start (no Vulkan GPU) — the host should show 2D.</summary>
    public event EventHandler? GpuUnavailable;

    private const double DynamicRangeDb = 30.0;
    private const float SurfaceScale = 100f;

    private SharpEngineSceneView? _sceneView;
    private PointerCameraController? _cameraController;
    private TargetPositionCamera? _camera;
    private GroupNode? _surfaceRoot;
    private bool _gpuFailed;

    public FarField3DView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    static FarField3DView()
    {
        PatternProperty.Changed.AddClassHandler<FarField3DView>((v, _) => v.RebuildSurface());
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (_sceneView is not null || _gpuFailed) return;
        try
        {
            _sceneView = new SharpEngineSceneView(PresentationTypes.WriteableBitmap, "FarFieldSceneView");
            _sceneView.GpuDeviceCreationFailed += (_, _) => FallBackToTwoD("3D view unavailable (no Vulkan GPU on this machine).");

            var scene = _sceneView.Scene;
            scene.Lights.Clear();
            scene.Lights.Add(new DirectionalLight(new Vector3(-0.5f, -0.5f, -0.6f)));
            scene.Lights.Add(new DirectionalLight(new Vector3(0.6f, 0.4f, 0.5f)));
            scene.SetAmbientLight(0.45f);

            // Reference scaffolding so the surface reads as 3D: the ground plane (y = 0) as concentric
            // range rings + compass spokes, and a vertical axis up to the zenith.
            var scaffold = new GroupNode("Scaffold");
            var gridColor = new Color4(0.60f, 0.62f, 0.68f, 0.55f);
            var axisColor = new Color4(0.40f, 0.55f, 0.75f, 0.85f);
            foreach (float rf in new[] { 0.5f, 1.0f })
            {
                float rr = rf * SurfaceScale;
                var ring = new Vector3[49];
                for (int i = 0; i <= 48; i++)
                {
                    double a = i * 2 * Math.PI / 48;
                    ring[i] = new Vector3((float)(rr * Math.Cos(a)), 0, (float)(rr * Math.Sin(a)));
                }
                scaffold.Add(new MultiLineNode(ring, isLineStrip: true, gridColor, 1f, $"Ring{rf}"));
            }
            for (int s = 0; s < 8; s++)
            {
                double a = s * Math.PI / 4;
                var end = new Vector3((float)(SurfaceScale * Math.Cos(a)), 0, (float)(SurfaceScale * Math.Sin(a)));
                scaffold.Add(new LineNode(new Vector3(0, 0, 0), end, gridColor, 1f, $"Spoke{s}"));
            }
            scaffold.Add(new LineNode(new Vector3(0, 0, 0), new Vector3(0, SurfaceScale, 0), axisColor, 1.5f, "ZenithAxis"));

            // Orientation labels (compass around the horizon + zenith/horizon) so it reads like the
            // familiar EZNEC-style 3D pattern. Guarded so a text hiccup can't take down the whole view.
            try
            {
                var text = new TextBlockFactory(scene)
                {
                    FontSize = 11,
                    TextColor = new Color4(0.24f, 0.30f, 0.40f, 1f),
                };
                float lr = SurfaceScale * 1.16f;
                scaffold.Add(text.CreateTextBlock("N", new Vector3(lr, 0, 0), 90f, 0f, "LblN"));
                scaffold.Add(text.CreateTextBlock("E", new Vector3(0, 0, lr), 90f, 0f, "LblE"));
                scaffold.Add(text.CreateTextBlock("S", new Vector3(-lr, 0, 0), 90f, 0f, "LblS"));
                scaffold.Add(text.CreateTextBlock("W", new Vector3(0, 0, -lr), 90f, 0f, "LblW"));
                scaffold.Add(text.CreateTextBlock("Horizon", new Vector3(lr * 0.7f, 0, lr * 0.7f), 90f, 0f, "LblHorizon"));
                // Zenith stands UP at the top of the plot (explicit up = world-Y), facing the default view.
                scaffold.Add(text.CreateTextBlock(
                    new Vector3(0, SurfaceScale * 1.12f, 0), PositionTypes.Center, "Zenith",
                    textDirection: new Vector3(1f, 0f, 0.35f), upDirection: new Vector3(0f, 1f, 0f), "LblZenith"));
            }
            catch
            {
                // Labels are optional decoration; ignore if the text renderer isn't available.
            }

            scene.RootNode.Add(scaffold);

            _surfaceRoot = new GroupNode("SurfaceRoot");
            scene.RootNode.Add(_surfaceRoot);

            // Y is up (zenith); the surface fills the upper hemisphere. A raised 3/4 view reads as 3D.
            // FitIntoView (in RebuildSurface) then frames each antenna consistently, so the user
            // doesn't have to drag to get a good starting view.
            _camera = new TargetPositionCamera
            {
                Heading = -35,
                Attitude = -28,
                Distance = 360,
                TargetPosition = new Vector3(0, SurfaceScale * 0.35f, 0),
                ShowCameraLight = ShowCameraLightType.Auto,
            };
            _sceneView.SceneView.Camera = _camera;
            _cameraController = new PointerCameraController(_sceneView);

            HostGrid.Children.Insert(0, _sceneView);
            RebuildSurface();
        }
        catch (Exception)
        {
            FallBackToTwoD("3D view could not start on this machine.");
        }
    }

    private void FallBackToTwoD(string message)
    {
        _gpuFailed = true;
        StatusText.Text = message + " Showing the 2D plot instead.";
        StatusText.IsVisible = true;
        GpuUnavailable?.Invoke(this, EventArgs.Empty);
    }

    private void RebuildSurface()
    {
        if (_surfaceRoot is null || _sceneView is null || _gpuFailed) return;
        _surfaceRoot.Clear();

        var grid = Pattern?.Grid;
        if (grid is null || grid.Count < 6)
        {
            StatusText.Text = "No 3D pattern data for this antenna. Use the 2D view.";
            StatusText.IsVisible = true;
            return;
        }

        StatusText.IsVisible = false;

        // --- Build the lattice of surface points (distance from center = normalized gain). ---
        var azimuths = grid.Select(g => g.AzimuthDeg).Distinct().OrderBy(a => a).ToList();
        var elevations = grid.Select(g => g.ElevationAngleDeg).Distinct().OrderBy(el => el).ToList();
        var gainByDir = new Dictionary<(double Az, double El), double>();
        foreach (var g in grid) gainByDir[(g.AzimuthDeg, g.ElevationAngleDeg)] = g.GainDbi;

        double minGain = Pattern!.PeakGainDbi - DynamicRangeDb;
        int na = azimuths.Count, ne = elevations.Count;
        var positions = new Vector3[na, ne];
        var vertices = new PositionNormalTextureVertex[na * ne];
        var colors = new Color4[na * ne];
        int Index(int ai, int ei) => ai * ne + ei;

        for (int ai = 0; ai < na; ai++)
        {
            double azRad = azimuths[ai] * Math.PI / 180.0;
            for (int ei = 0; ei < ne; ei++)
            {
                double gain = gainByDir.TryGetValue((azimuths[ai], elevations[ei]), out var gv) ? gv : minGain;
                double norm = Math.Clamp((gain - minGain) / DynamicRangeDb, 0.0, 1.0);
                float r = (float)Math.Max(norm, 0.03) * SurfaceScale; // small floor so nulls stay visible

                double elRad = elevations[ei] * Math.PI / 180.0;
                float y = (float)(r * Math.Sin(elRad));          // up = zenith
                float h = (float)(r * Math.Cos(elRad));
                var pos = new Vector3((float)(h * Math.Cos(azRad)), y, (float)(h * Math.Sin(azRad)));
                positions[ai, ei] = pos;

                var normal = pos.Length() > 1e-4f ? Vector3.Normalize(pos) : new Vector3(0, 1, 0);
                vertices[Index(ai, ei)] = new PositionNormalTextureVertex(
                    pos, normal, new Vector2((float)ai / Math.Max(1, na - 1), (float)ei / Math.Max(1, ne - 1)));
                colors[Index(ai, ei)] = Colormap(norm);
            }
        }

        var indices = new List<int>((na - 1) * (ne - 1) * 6);
        for (int ai = 0; ai < na - 1; ai++)
        {
            for (int ei = 0; ei < ne - 1; ei++)
            {
                int a = Index(ai, ei), b = Index(ai + 1, ei), c = Index(ai + 1, ei + 1), d = Index(ai, ei + 1);
                indices.Add(a); indices.Add(b); indices.Add(c);
                indices.Add(a); indices.Add(c); indices.Add(d);
            }
        }

        var mesh = new StandardMesh(vertices, indices.ToArray(), "FarFieldMesh");
        _surfaceRoot.Add(new MeshModelNode(mesh, new VertexColorMaterial(colors, "FarFieldColors"), "FarFieldSurface"));

        AddWireframe(positions, na, ne);

        // Take-off marker: a bold green line from the center out to the strongest direction (the
        // elevation/azimuth of peak gain) — the 3D echo of the 2D plot's take-off line.
        var peak = grid.Aggregate((a, b) => b.GainDbi > a.GainDbi ? b : a);
        double paz = peak.AzimuthDeg * Math.PI / 180.0, pel = peak.ElevationAngleDeg * Math.PI / 180.0;
        float pr = SurfaceScale * 1.03f;
        var tip = new Vector3(
            (float)(pr * Math.Cos(pel) * Math.Cos(paz)),
            (float)(pr * Math.Sin(pel)),
            (float)(pr * Math.Cos(pel) * Math.Sin(paz)));
        _surfaceRoot.Add(new LineNode(new Vector3(0, 0, 0), tip, new Color4(0.10f, 0.62f, 0.34f, 1f), 2.5f, "TakeoffLine"));

        // Frame the surface consistently so every antenna comes up well-composed without dragging.
        // Extra margin (1.75) leaves room for the ground rings and compass labels around the lobe.
        _camera?.FitIntoView(_surfaceRoot, adjustTargetPosition: true, adjustmentFactor: 1.75f);
    }

    /// <summary>
    /// Overlay the constant-azimuth curves and constant-elevation rings on the surface — the grid
    /// lines that make a far-field plot read as a proper 3D antenna pattern. Lines are pushed a hair
    /// outward from the surface to avoid z-fighting.
    /// </summary>
    private void AddWireframe(Vector3[,] positions, int na, int ne)
    {
        if (_surfaceRoot is null) return;
        var lineColor = new Color4(0.12f, 0.15f, 0.20f, 0.55f);
        const float lift = 1.012f; // push lines just off the surface

        int azStep = Math.Max(1, na / 12);          // ~12 meridians (constant-azimuth curves)
        for (int ai = 0; ai < na; ai += azStep)
        {
            var strip = new Vector3[ne];
            for (int ei = 0; ei < ne; ei++) strip[ei] = positions[ai, ei] * lift;
            _surfaceRoot.Add(new MultiLineNode(strip, isLineStrip: true, lineColor, 1f, $"Meridian{ai}"));
        }

        int elStep = Math.Max(1, ne / 6);           // ~6 rings (constant-elevation curves)
        for (int ei = 0; ei < ne; ei += elStep)
        {
            var ring = new Vector3[na];
            for (int ai = 0; ai < na; ai++) ring[ai] = positions[ai, ei] * lift;
            _surfaceRoot.Add(new MultiLineNode(ring, isLineStrip: true, lineColor, 1f, $"Ring{ei}"));
        }
    }

    /// <summary>Blue (weak) → cyan → green → yellow → red (strong).</summary>
    private static Color4 Colormap(double t)
    {
        t = Math.Clamp(t, 0.0, 1.0);
        (double Pos, float R, float G, float B)[] stops =
        {
            (0.00, 0.10f, 0.20f, 0.85f),
            (0.30, 0.00f, 0.75f, 0.90f),
            (0.55, 0.15f, 0.80f, 0.20f),
            (0.80, 0.95f, 0.85f, 0.10f),
            (1.00, 0.90f, 0.15f, 0.10f),
        };
        for (int i = 0; i < stops.Length - 1; i++)
        {
            var lo = stops[i];
            var hi = stops[i + 1];
            if (t <= hi.Pos)
            {
                float f = (float)((t - lo.Pos) / Math.Max(1e-6, hi.Pos - lo.Pos));
                return new Color4(
                    lo.R + (hi.R - lo.R) * f,
                    lo.G + (hi.G - lo.G) * f,
                    lo.B + (hi.B - lo.B) * f,
                    1f);
            }
        }
        var last = stops[^1];
        return new Color4(last.R, last.G, last.B, 1f);
    }
}

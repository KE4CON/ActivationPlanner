using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.PropagationModel.Missions;
using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.Location;
using ActivationPlanner.Services.Planning;
using ActivationPlanner.Services.Trend;
using ActivationPlanner.UI.ViewModels.Trend;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// Propagation trend view (v1.0): samples the current-hour reliability per band on a background
/// interval and shows a rolling "recently vs. now" strip per band, to support the replanning
/// moment (a band going dead). Session-local only — nothing is persisted (stateless-replanning
/// rule). The sampling loop is cancelled when the screen is navigated away from (<see cref="Dispose"/>).
/// </summary>
public sealed partial class TrendViewModel : ViewModelBase, IDisposable
{
    private static readonly TimeSpan SampleInterval = TimeSpan.FromMinutes(15);

    private readonly PlanningService _planning;
    private readonly LocationService _location;
    private readonly GearInventoryService _inventory;
    private readonly SessionState _session;
    private readonly PropagationTrend _trend = new();
    private readonly CancellationTokenSource _cts = new();

    private double _latitude = 39.83;
    private double _longitude = -98.58;
    private bool _sampling;

    public TrendViewModel(
        PlanningService planning, LocationService location,
        GearInventoryService inventory, SessionState session)
    {
        ArgumentNullException.ThrowIfNull(planning);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(session);
        _planning = planning;
        _location = location;
        _inventory = inventory;
        _session = session;

        _ = RunAsync(_cts.Token);
    }

    public ObservableCollection<TrendBandViewModel> Bands { get; } = [];

    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private string? _lastSampled;
    [ObservableProperty] private int _sampleCount;

    public bool HasSamples => SampleCount > 0;

    private async Task RunAsync(CancellationToken ct)
    {
        // One-time best-effort location fix, then sample immediately and on the interval.
        try
        {
            var fix = await _location.RefreshAsync(ct);
            _latitude = fix.Location.LatitudeDeg;
            _longitude = fix.Location.LongitudeDeg;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Fall back to the default location; the trend still runs.
        }

        await SampleAsync(ct);

        try
        {
            using var timer = new PeriodicTimer(SampleInterval);
            while (await timer.WaitForNextTickAsync(ct))
                await SampleAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // Screen navigated away — stop sampling.
        }
    }

    /// <summary>Force a sample now (also used for the initial load).</summary>
    [RelayCommand]
    private Task SampleNowAsync() => SampleAsync(_cts.Token);

    private async Task SampleAsync(CancellationToken ct)
    {
        if (_sampling)
            return;
        _sampling = true;
        try
        {
            SessionPlan plan = await _planning.PlanAsync(BuildQuery(), _inventory.Current.Antennas, ct);
            _trend.Add(PropagationTrend.SnapshotFrom(plan, DateTime.UtcNow));
            RebuildBands();
            LastSampled = "Last sampled " + DateTime.Now.ToString("HH:mm", CultureInfo.CurrentCulture)
                          + $" • {_trend.Snapshots.Count} sample(s) in the last few hours";
            StatusMessage = null;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Sampling failed: {ex.Message}";
        }
        finally
        {
            _sampling = false;
        }
    }

    private void RebuildBands()
    {
        var times = _trend.Snapshots.Select(s => s.CapturedAtUtc).ToList();

        Bands.Clear();
        foreach (HamBand band in HamBands.All)
        {
            IReadOnlyList<double?> series = _trend.SeriesFor(band);
            if (series.All(r => r is null))
                continue; // band never evaluated (e.g. NVIS framing) — omit its row

            var cells = series
                .Select((r, i) => new TrendCellViewModel(r, times[i]))
                .ToList();

            double? latest = series[^1];
            string latestLabel = latest is { } r ? $"{r * 100:0}%" : "—";
            Bands.Add(new TrendBandViewModel(HamBands.DisplayName(band), cells, latestLabel));
        }

        SampleCount = _trend.Snapshots.Count;
        OnPropertyChanged(nameof(HasSamples));
    }

    private CircuitQuery BuildQuery()
    {
        var now = DateTime.UtcNow;
        PropagationFraming framing = MissionProfiles.For(_session.SelectedMission).Framing;
        return new CircuitQuery
        {
            Transmitter = new GeoLocation(_latitude, _longitude),
            Receiver = new GeoLocation(_latitude + 5, _longitude + 5), // a nominal target for trend sampling
            Month = now.Month,
            Year = now.Year,
            SunspotNumber = 70,
            Framing = framing,
            Bands = PropagationFramingBands.For(framing),
        };
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}

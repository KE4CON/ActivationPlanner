using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.PropagationModel.Missions;
using ActivationPlanner.PropagationModel.Voacap;
using System.IO;
using ActivationPlanner.Services.Checklists;
using ActivationPlanner.Services.Export;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.Location;
using ActivationPlanner.Services.Planning;
using ActivationPlanner.Services.SpaceWeather;
using ActivationPlanner.UI.ViewModels.Planning;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// The core planning screen (Phase 4): enter the circuit and current solar conditions,
/// generate a plan, and see bands ranked by predicted reliability — each with a 24-hour
/// heatmap and the operator's matching antennas (Option A/B). Stateless per the replanning
/// rule: every run uses the values on screen; nothing is persisted.
/// </summary>
public sealed partial class PlanningViewModel : ViewModelBase
{
    private readonly PlanningService _planning;
    private readonly GearInventoryService _inventory;
    private readonly LocationService _location;
    private readonly ChecklistService _checklist;
    private readonly PdfExportService _pdf;
    private readonly SpaceWeatherClient _spaceWeather;
    private readonly SessionState _session;
    private SessionPlan? _lastPlan;

    public PlanningViewModel(
        PlanningService planning, GearInventoryService inventory, LocationService location,
        ChecklistService checklist, PdfExportService pdf, SpaceWeatherClient spaceWeather,
        SessionState session, bool isSampleData, bool quickStart = false)
    {
        ArgumentNullException.ThrowIfNull(planning);
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(checklist);
        ArgumentNullException.ThrowIfNull(pdf);
        ArgumentNullException.ThrowIfNull(spaceWeather);
        ArgumentNullException.ThrowIfNull(session);
        _planning = planning;
        _inventory = inventory;
        _location = location;
        _checklist = checklist;
        _pdf = pdf;
        _spaceWeather = spaceWeather;
        _session = session;
        IsSampleData = isSampleData;

        var now = DateTime.UtcNow;
        _month = now.Month;
        _year = now.Year;

        // Default the framing from the mission selected elsewhere in the session (overridable here).
        MissionProfile mission = MissionProfiles.For(session.SelectedMission);
        _framing = mission.Framing;
        MissionContext = $"Mission: {mission.DisplayName}";

        if (quickStart)
            // Quick Mode: fetch live solar, locate, and generate so the operator lands on live recs.
            _ = QuickStartAsync();
        else
            // Otherwise prefill the sunspot number from live space weather in the background.
            _ = FetchSolarAsync();
    }

    private async Task QuickStartAsync()
    {
        await FetchSolarAsync();     // use real solar numbers for the auto-generated plan
        await UseMyLocationAsync();  // best-effort; reports its own status/errors
        await GeneratePlanAsync();
    }

    private bool CanFetchSolar => !IsFetchingSolar;

    /// <summary>Pull current solar indices from the public feed and prefill the sunspot number.</summary>
    [RelayCommand(CanExecute = nameof(CanFetchSolar))]
    private async Task FetchSolarAsync()
    {
        IsFetchingSolar = true;
        try
        {
            SolarConditions sw = await _spaceWeather.GetCurrentAsync();
            if (sw.HasSunspotNumber)
                SunspotNumber = sw.SunspotNumber!.Value;
            SolarSummary = BuildSolarSummary(sw);
        }
        catch (Exception ex) when (ex is SpaceWeatherUnavailableException or SpaceWeatherFormatException)
        {
            SolarSummary = "Live solar data unavailable — using the value shown (type your own if needed).";
        }
        finally
        {
            IsFetchingSolar = false;
        }
    }

    private static string BuildSolarSummary(SolarConditions sw)
    {
        var parts = new List<string>();
        if (sw.SolarFluxIndex is { } sfi) parts.Add($"SFI {sfi}");
        if (sw.SunspotNumber is { } ssn) parts.Add($"SSN {ssn}");
        if (sw.KIndex is { } k) parts.Add($"K {k}");
        string metrics = parts.Count > 0 ? string.Join(" · ", parts) : "no data";
        return sw.UpdatedText is { } updated ? $"Live: {metrics} · {updated}" : $"Live: {metrics}";
    }

    /// <summary>True when predictions come from the offline sample stand-in, not a real VOACAP run.</summary>
    public bool IsSampleData { get; }

    public IReadOnlyList<NoiseEnvironment> NoiseOptions { get; } = Enum.GetValues<NoiseEnvironment>();

    public IReadOnlyList<PropagationFraming> FramingOptions { get; } = Enum.GetValues<PropagationFraming>();

    /// <summary>Which mission this plan defaulted its framing from (context for the operator).</summary>
    public string MissionContext { get; }

    // ---- inputs ----

    [ObservableProperty] private double _operatorLatitude = 39.83;
    [ObservableProperty] private double _operatorLongitude = -98.58;
    [ObservableProperty] private double _targetLatitude = 40.0;
    [ObservableProperty] private double _targetLongitude = -105.0;
    [ObservableProperty] private double _sunspotNumber = 70;
    [ObservableProperty] private string? _solarSummary;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(FetchSolarCommand))]
    private bool _isFetchingSolar;
    [ObservableProperty] private int _month;
    [ObservableProperty] private int _year;
    [ObservableProperty] private double _transmitPowerWatts = 100;
    [ObservableProperty] private NoiseEnvironment _noise = NoiseEnvironment.Residential;
    [ObservableProperty] private bool _useLongPath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsRegionalNvis))]
    private PropagationFraming _framing;

    /// <summary>True when the regional/NVIS framing is selected — surfaces near-in-target guidance.</summary>
    public bool IsRegionalNvis => Framing == PropagationFraming.RegionalNvis;

    // ---- location (refresh-on-demand) ----

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UseMyLocationCommand))]
    private bool _isLocating;

    [ObservableProperty] private string? _locationStatus;

    private bool CanLocate => !IsLocating;

    /// <summary>Fill the operator location from a one-shot location fix (network geo-IP).</summary>
    [RelayCommand(CanExecute = nameof(CanLocate))]
    private async Task UseMyLocationAsync()
    {
        IsLocating = true;
        LocationStatus = "Locating…";
        try
        {
            var fix = await _location.RefreshAsync();
            OperatorLatitude = Math.Round(fix.Location.LatitudeDeg, 4);
            OperatorLongitude = Math.Round(fix.Location.LongitudeDeg, 4);
            string place = fix.PlaceName is { } p ? p + " — " : string.Empty;
            LocationStatus = $"{place}{fix.SourceLabel}{(fix.IsApproximate ? " (approximate)" : string.Empty)}";
        }
        catch (LocationUnavailableException ex)
        {
            LocationStatus = ex.Message;
        }
        catch (Exception ex)
        {
            LocationStatus = $"Location failed: {ex.Message}";
        }
        finally
        {
            IsLocating = false;
        }
    }

    // ---- results / state ----

    public ObservableCollection<BandRecommendationViewModel> Bands { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    private bool _hasPlan;

    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private string? _summary;

    /// <summary>Hour labels for the heatmap axis (from the most recent plan).</summary>
    public ObservableCollection<int> HourAxis { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GeneratePlanCommand))]
    private bool _isBusy;

    public bool HasResults => HasPlan && Bands.Count > 0;

    private bool CanGenerate => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private async Task GeneratePlanAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            CircuitQuery query = BuildQuery();
            var antennas = _inventory.Current.Antennas;

            SessionPlan plan = await _planning.PlanAsync(query, antennas);
            _lastPlan = plan;

            Bands.Clear();
            foreach (var band in plan.Bands)
                Bands.Add(new BandRecommendationViewModel(band));

            HourAxis.Clear();
            foreach (int h in plan.HoursUtc)
                HourAxis.Add(h);

            Summary = BuildSummary(plan, antennas.Count);
            HasPlan = true;
            OnPropertyChanged(nameof(HasResults));
            OnPropertyChanged(nameof(CanExport));

            if (antennas.Count == 0)
                StatusMessage = "No antennas in your inventory yet — add some to see which to use per band.";
        }
        catch (ArgumentOutOfRangeException)
        {
            HasPlan = false;
            StatusMessage = "Check the coordinates: latitude must be within ±90° and longitude within ±180°.";
        }
        catch (Exception ex)
        {
            HasPlan = false;
            StatusMessage = $"Could not generate a plan: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ---- PDF export (operator-selectable sections) ----

    [ObservableProperty] private bool _exportBands = true;
    [ObservableProperty] private bool _exportAntennas = true;
    [ObservableProperty] private bool _exportChecklist = true;

    /// <summary>True once a plan exists to export.</summary>
    public bool CanExport => _lastPlan is not null;

    /// <summary>Suggested file name for the export dialog.</summary>
    public string SuggestedExportFileName =>
        $"activation-plan-{DateTime.Now:yyyyMMdd-HHmm}.pdf";

    /// <summary>Render the current plan to <paramref name="output"/> as PDF (called by the view's save flow).</summary>
    public async Task ExportAsync(Stream output)
    {
        if (_lastPlan is not { } plan)
            return;

        var request = new PdfExportRequest
        {
            Title = "Activation Plan",
            Subtitle = $"{MissionContext} • {plan.DistanceKm:0} km path • generated {DateTime.Now:yyyy-MM-dd HH:mm}",
            IsSampleData = IsSampleData,
            IncludeBands = ExportBands,
            IncludeAntennas = ExportAntennas,
            IncludeChecklist = ExportChecklist,
            Bands = plan.Bands,
            Checklist = ExportChecklist
                ? _checklist.Build(_session.SelectedMission, _inventory.Current)
                : null,
        };

        await _pdf.WriteAsync(request, output);
    }

    private CircuitQuery BuildQuery() => new()
    {
        Transmitter = new GeoLocation(OperatorLatitude, OperatorLongitude),
        Receiver = new GeoLocation(TargetLatitude, TargetLongitude),
        Month = Math.Clamp(Month, 1, 12),
        Year = Year,
        SunspotNumber = SunspotNumber,
        // Framing changes the question asked: regional/NVIS predicts the low bands, DX the full set.
        Framing = Framing,
        Bands = PropagationFramingBands.For(Framing),
        Noise = Noise,
        TransmitPowerWatts = TransmitPowerWatts,
        UseLongPath = UseLongPath,
    };

    private static string BuildSummary(SessionPlan plan, int antennaCount)
    {
        string best = plan.Bands.FirstOrDefault() is { } top
            ? $"Best band: {top.BandName} ({top.AverageReliabilityLabelPercent()}% avg)."
            : "No bands predicted.";
        return $"{plan.DistanceKm:0} km path. {best} {antennaCount} owned antenna(s) matched.";
    }
}

internal static class BandRecommendationSummaryExtensions
{
    public static int AverageReliabilityLabelPercent(this BandRecommendation band) =>
        (int)Math.Round(band.AverageReliability * 100);
}

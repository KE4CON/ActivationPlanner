using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ActivationPlanner.Services.Checklists;
using ActivationPlanner.Services.Export;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.Location;
using ActivationPlanner.PropagationModel.Antennas;
using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.Services.Missions;
using ActivationPlanner.Services.Planning;
using ActivationPlanner.Services.Pota;
using ActivationPlanner.Services.SpaceWeather;
using ActivationPlanner.Services.Weather;
using ActivationPlanner.UI.ViewModels.Wizard;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Inventory = ActivationPlanner.PropagationModel.Gear.GearInventory;

namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// Application shell. Loads the inventory on startup and routes to the first-use setup wizard
/// when nothing is owned yet, or into the main app (planning + inventory) otherwise. A simple
/// nav bar switches between the planning screen and the inventory editor; it is hidden during
/// the full-screen setup wizard. <see cref="CurrentPage"/> is rendered by the shell's
/// ContentControl via the <see cref="ViewLocator"/>.
/// </summary>
public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly GearInventoryService _inventory;
    private readonly PlanningService _planning;
    private readonly LocationService _location;
    private readonly MissionTypeService _missions;
    private readonly ChecklistService _checklist;
    private readonly PotaClient _pota;
    private readonly PotaSelfSpotter _selfSpotter;
    private readonly SpaceWeatherClient _spaceWeather;
    private readonly WeatherClient _weather;
    private readonly PdfExportService _pdf;
    private readonly IAntennaPatternSource _patternSource;
    private readonly bool _patternIsSample;
    private readonly SessionState _session;
    private readonly bool _isSampleData;

    public MainWindowViewModel(
        GearInventoryService inventory, PlanningService planning, LocationService location,
        MissionTypeService missions, ChecklistService checklist, PotaClient pota,
        PotaSelfSpotter selfSpotter, SpaceWeatherClient spaceWeather, WeatherClient weather,
        PdfExportService pdf, IAntennaPatternSource patternSource, bool patternIsSample,
        SessionState session, bool isSampleData)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(planning);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(missions);
        ArgumentNullException.ThrowIfNull(checklist);
        ArgumentNullException.ThrowIfNull(pota);
        ArgumentNullException.ThrowIfNull(selfSpotter);
        ArgumentNullException.ThrowIfNull(spaceWeather);
        ArgumentNullException.ThrowIfNull(weather);
        ArgumentNullException.ThrowIfNull(pdf);
        ArgumentNullException.ThrowIfNull(patternSource);
        ArgumentNullException.ThrowIfNull(session);
        _inventory = inventory;
        _planning = planning;
        _location = location;
        _missions = missions;
        _checklist = checklist;
        _pota = pota;
        _selfSpotter = selfSpotter;
        _spaceWeather = spaceWeather;
        _weather = weather;
        _pdf = pdf;
        _patternSource = patternSource;
        _patternIsSample = patternIsSample;
        _session = session;
        _isSampleData = isSampleData;

        // Persistent live clock (local above UTC) for the header.
        UpdateClock();
        _clock = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _clock.Tick += (_, _) => UpdateClock();
        _clock.Start();

        // App-wide weather-alert watch: poll in the background so watches/warnings pop up on ANY
        // page, not just the Weather tab. First check shortly after launch, then every 10 minutes.
        _alertTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(10) };
        _alertTimer.Tick += (_, _) => _ = MonitorAlertsAsync();
        _alertTimer.Start();
        _ = MonitorAlertsAsync();
    }

    private readonly DispatcherTimer _clock;

    [ObservableProperty] private string _localTimeText = "";
    [ObservableProperty] private string _utcTimeText = "";

    // ---- App-wide weather alerts (watches/warnings) — surface on any page ----

    private static readonly IBrush SevereAlertBrush = new SolidColorBrush(Color.FromRgb(0xC0, 0x39, 0x2B));
    private static readonly IBrush ModerateAlertBrush = new SolidColorBrush(Color.FromRgb(0xC9, 0x6A, 0x1B));
    private static readonly IBrush MinorAlertBrush = new SolidColorBrush(Color.FromRgb(0xC9, 0x9A, 0x2B));

    private readonly DispatcherTimer _alertTimer;
    private readonly HashSet<string> _acknowledgedAlertKeys = new(StringComparer.Ordinal);
    private double? _alertLat;
    private double? _alertLon;

    /// <summary>Currently active watches/warnings for the operator's area.</summary>
    public ObservableCollection<WeatherAlert> ActiveAlerts { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowAlertOverlay))]
    private bool _alertsAcknowledged = true;

    [ObservableProperty] private string _alertHeadline = "";
    [ObservableProperty] private IBrush _alertBrush = SevereAlertBrush;

    /// <summary>True while any alert is active (drives the persistent banner).</summary>
    public bool HasActiveAlerts => ActiveAlerts.Count > 0;

    /// <summary>The blocking overlay shows while active alerts haven't been acknowledged.</summary>
    public bool ShowAlertOverlay => HasActiveAlerts && !AlertsAcknowledged;

    /// <summary>Dismiss the blocking overlay — the operator has read the current alerts.</summary>
    [RelayCommand]
    private void AcknowledgeAlerts()
    {
        foreach (WeatherAlert a in ActiveAlerts)
            _acknowledgedAlertKeys.Add(a.Key);
        AlertsAcknowledged = true;
    }

    /// <summary>Reopen the alert overlay from the persistent banner.</summary>
    [RelayCommand]
    private void ReopenAlerts() => AlertsAcknowledged = false;

    private async Task MonitorAlertsAsync()
    {
        try
        {
            if (_alertLat is null || _alertLon is null)
            {
                LocationFix fix = await _location.RefreshAsync();
                _alertLat = fix.Location.LatitudeDeg;
                _alertLon = fix.Location.LongitudeDeg;
            }

            IReadOnlyList<WeatherAlert> alerts = await _weather.GetAlertsAsync(_alertLat.Value, _alertLon.Value);

            ActiveAlerts.Clear();
            foreach (WeatherAlert a in alerts)
                ActiveAlerts.Add(a);

            int worst = alerts.Count > 0 ? alerts.Max(a => a.SeverityRank) : 0;
            AlertBrush = worst >= 3 ? SevereAlertBrush : worst == 2 ? ModerateAlertBrush : MinorAlertBrush;
            AlertHeadline = alerts.Count switch
            {
                0 => "",
                1 => "⚠  1 active weather alert",
                var n => $"⚠  {n} active weather alerts",
            };

            // Force the blocking overlay only for a NEW significant alert — Moderate severity and up
            // (warnings, watches, significant advisories). Minor statements still show in the banner
            // (and in the overlay if reopened) but never interrupt.
            const int significantSeverity = 2; // Moderate
            if (alerts.Any(a => a.SeverityRank >= significantSeverity && !_acknowledgedAlertKeys.Contains(a.Key)))
                AlertsAcknowledged = false;
            else if (alerts.Count == 0)
                AlertsAcknowledged = true;

            OnPropertyChanged(nameof(HasActiveAlerts));
            OnPropertyChanged(nameof(ShowAlertOverlay));
        }
        catch
        {
            // Best-effort background poll; try again on the next tick.
        }
    }

    private void UpdateClock()
    {
        LocalTimeText = DateTime.Now.ToString("M/d/yyyy HH:mm:ss");
        UtcTimeText = DateTime.UtcNow.ToString("M/d/yyyy HH:mm:ss");
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowNavigation))]
    private ViewModelBase? _currentPage;

    /// <summary>Which nav destination is active — drives the highlighted (accent) nav button.</summary>
    [ObservableProperty] private NavPage _activePage = NavPage.Planning;

    // Dispose a page as we leave it, so any background work it owns (e.g. the trend sampler) stops.
    partial void OnCurrentPageChanging(ViewModelBase? oldValue, ViewModelBase? newValue)
    {
        if (oldValue is IDisposable disposable)
            disposable.Dispose();
    }

    /// <summary>Nav bar is shown for the main app, hidden during the first-run wizard.</summary>
    public bool ShowNavigation => CurrentPage is not SetupWizardViewModel and not null;

    /// <summary>Load persisted gear and choose the landing page.</summary>
    public async Task InitializeAsync()
    {
        await _inventory.LoadAsync();

        if (_inventory.IsFirstRun)
            CurrentPage = new SetupWizardViewModel(OnWizardCompletedAsync, ShowQuickPlan);
        else
            ShowPlanning();
    }

    [RelayCommand]
    private void ShowPlanning()
    {
        CurrentPage = new PlanningViewModel(
            _planning, _inventory, _location, _checklist, _pdf, _spaceWeather, _session, _isSampleData);
        ActivePage = NavPage.Planning;
    }

    /// <summary>Quick Mode: jump straight to the recommendation view and auto-generate a plan.</summary>
    [RelayCommand]
    private void ShowQuickPlan()
    {
        CurrentPage = new PlanningViewModel(
            _planning, _inventory, _location, _checklist, _pdf, _spaceWeather, _session, _isSampleData, quickStart: true);
        ActivePage = NavPage.QuickPlan;
    }

    [RelayCommand]
    private void ShowMissionChecklist()
    {
        CurrentPage = new MissionChecklistViewModel(_missions, _checklist, _inventory, _pdf, _session);
        ActivePage = NavPage.Mission;
    }

    [RelayCommand]
    private void ShowTrend()
    {
        CurrentPage = new TrendViewModel(_planning, _location, _inventory, _session);
        ActivePage = NavPage.Trend;
    }

    [RelayCommand]
    private void ShowGreyLine()
    {
        CurrentPage = new GreyLineViewModel(_planning, _location, _inventory);
        ActivePage = NavPage.GreyLine;
    }

    [RelayCommand]
    private void ShowPotaSpots()
    {
        CurrentPage = new PotaSpotsViewModel(_pota, _selfSpotter, _session);
        ActivePage = NavPage.Pota;
    }

    [RelayCommand]
    private void ShowAntennaPattern()
    {
        CurrentPage = new AntennaPatternViewModel(_patternSource, _inventory, _patternIsSample);
        ActivePage = NavPage.Antenna;
    }

    [RelayCommand]
    private void ShowInventory()
    {
        CurrentPage = new InventoryEditViewModel(_inventory);
        ActivePage = NavPage.Inventory;
    }

    [RelayCommand]
    private void ShowBatteryCalculator()
    {
        CurrentPage = new BatteryCalculatorViewModel(_inventory);
        ActivePage = NavPage.Battery;
    }

    [RelayCommand]
    private void ShowWeather()
    {
        CurrentPage = new WeatherViewModel(_weather, _location);
        ActivePage = NavPage.Weather;
    }

    [RelayCommand]
    private void ShowBandPlan()
    {
        CurrentPage = new BandPlanViewModel();
        ActivePage = NavPage.BandPlan;
    }

    private async Task OnWizardCompletedAsync(Inventory inventory)
    {
        await _inventory.ReplaceAsync(inventory);
        ShowPlanning();
    }
}

/// <summary>The top-nav destinations, used to highlight the active button.</summary>
public enum NavPage
{
    QuickPlan,
    Planning,
    Trend,
    GreyLine,
    Mission,
    Pota,
    Antenna,
    Inventory,
    Battery,
    Weather,
    BandPlan,
}

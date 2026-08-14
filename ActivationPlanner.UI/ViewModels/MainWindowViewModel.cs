using System;
using System.Threading.Tasks;
using ActivationPlanner.Services.Checklists;
using ActivationPlanner.Services.Export;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.Location;
using ActivationPlanner.PropagationModel.Antennas;
using ActivationPlanner.Services.Missions;
using ActivationPlanner.Services.Planning;
using ActivationPlanner.Services.Pota;
using ActivationPlanner.UI.ViewModels.Wizard;
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
    private readonly PdfExportService _pdf;
    private readonly IAntennaPatternSource _patternSource;
    private readonly bool _patternIsSample;
    private readonly SessionState _session;
    private readonly bool _isSampleData;

    public MainWindowViewModel(
        GearInventoryService inventory, PlanningService planning, LocationService location,
        MissionTypeService missions, ChecklistService checklist, PotaClient pota,
        PotaSelfSpotter selfSpotter, PdfExportService pdf, IAntennaPatternSource patternSource,
        bool patternIsSample, SessionState session, bool isSampleData)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(planning);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(missions);
        ArgumentNullException.ThrowIfNull(checklist);
        ArgumentNullException.ThrowIfNull(pota);
        ArgumentNullException.ThrowIfNull(selfSpotter);
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
    }

    private readonly DispatcherTimer _clock;

    [ObservableProperty] private string _localTimeText = "";
    [ObservableProperty] private string _utcTimeText = "";

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
            _planning, _inventory, _location, _checklist, _pdf, _session, _isSampleData);
        ActivePage = NavPage.Planning;
    }

    /// <summary>Quick Mode: jump straight to the recommendation view and auto-generate a plan.</summary>
    [RelayCommand]
    private void ShowQuickPlan()
    {
        CurrentPage = new PlanningViewModel(
            _planning, _inventory, _location, _checklist, _pdf, _session, _isSampleData, quickStart: true);
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
}

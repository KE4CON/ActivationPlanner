using System;
using System.Threading.Tasks;
using ActivationPlanner.Services.Checklists;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.Location;
using ActivationPlanner.Services.Missions;
using ActivationPlanner.Services.Planning;
using ActivationPlanner.UI.ViewModels.Wizard;
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
    private readonly SessionState _session;
    private readonly bool _isSampleData;

    public MainWindowViewModel(
        GearInventoryService inventory, PlanningService planning, LocationService location,
        MissionTypeService missions, ChecklistService checklist,
        SessionState session, bool isSampleData)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(planning);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(missions);
        ArgumentNullException.ThrowIfNull(checklist);
        ArgumentNullException.ThrowIfNull(session);
        _inventory = inventory;
        _planning = planning;
        _location = location;
        _missions = missions;
        _checklist = checklist;
        _session = session;
        _isSampleData = isSampleData;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowNavigation))]
    private ViewModelBase? _currentPage;

    /// <summary>Nav bar is shown for the main app, hidden during the first-run wizard.</summary>
    public bool ShowNavigation => CurrentPage is not SetupWizardViewModel and not null;

    /// <summary>Load persisted gear and choose the landing page.</summary>
    public async Task InitializeAsync()
    {
        await _inventory.LoadAsync();

        if (_inventory.IsFirstRun)
            CurrentPage = new SetupWizardViewModel(OnWizardCompletedAsync);
        else
            ShowPlanning();
    }

    [RelayCommand]
    private void ShowPlanning() =>
        CurrentPage = new PlanningViewModel(_planning, _inventory, _location, _session, _isSampleData);

    [RelayCommand]
    private void ShowMissionChecklist() =>
        CurrentPage = new MissionChecklistViewModel(_missions, _checklist, _inventory, _session);

    [RelayCommand]
    private void ShowInventory() =>
        CurrentPage = new InventoryEditViewModel(_inventory);

    private async Task OnWizardCompletedAsync(Inventory inventory)
    {
        await _inventory.ReplaceAsync(inventory);
        ShowPlanning();
    }
}

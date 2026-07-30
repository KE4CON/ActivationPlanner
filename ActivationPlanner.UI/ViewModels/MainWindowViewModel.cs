using System;
using System.Threading.Tasks;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.UI.ViewModels.Wizard;
using CommunityToolkit.Mvvm.ComponentModel;
using Inventory = ActivationPlanner.PropagationModel.Gear.GearInventory;

namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// Application shell. Loads the inventory on startup and routes to the first-use
/// setup wizard when nothing is owned yet, or straight to the inventory editor
/// otherwise. <see cref="CurrentPage"/> is rendered by the shell's ContentControl
/// via the <see cref="ViewLocator"/>.
/// </summary>
public sealed partial class MainWindowViewModel : ViewModelBase
{
    private readonly GearInventoryService _service;

    public MainWindowViewModel(GearInventoryService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    [ObservableProperty]
    private ViewModelBase? _currentPage;

    /// <summary>Load persisted gear and choose the landing page.</summary>
    public async Task InitializeAsync()
    {
        await _service.LoadAsync();

        if (_service.IsFirstRun)
            ShowWizard();
        else
            ShowInventoryEditor();
    }

    private void ShowWizard()
    {
        CurrentPage = new SetupWizardViewModel(OnWizardCompletedAsync);
    }

    private void ShowInventoryEditor()
    {
        CurrentPage = new InventoryEditViewModel(_service);
    }

    private async Task OnWizardCompletedAsync(Inventory inventory)
    {
        await _service.ReplaceAsync(inventory);
        ShowInventoryEditor();
    }
}

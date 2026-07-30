using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.UI.ViewModels;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace ActivationPlanner.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Composition root — Phase 1 wires the gear-inventory stack by hand
            // (no DI container package). JSON persistence to the per-user app-data dir.
            var store = JsonGearStore.CreateDefault();
            var inventoryService = new GearInventoryService(store);
            var mainViewModel = new MainWindowViewModel(inventoryService);

            desktop.MainWindow = new MainWindow { DataContext = mainViewModel };

            // Load persisted gear and route to wizard/editor once the window exists.
            _ = mainViewModel.InitializeAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

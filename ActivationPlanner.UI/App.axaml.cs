using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.Planning;
using ActivationPlanner.UI.Sample;
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
            // Composition root — wires the app by hand (no DI container package).
            var store = JsonGearStore.CreateDefault();
            var inventoryService = new GearInventoryService(store);

            // Propagation source. Until the user has configured a VOACAP install (a later
            // settings feature), predictions come from an offline sample stand-in and the
            // planning screen flags them as sample data. When real VOACAP wiring lands, the
            // real VoacapPropagationEngine is constructed here instead of the sample predictor.
            var samplePredictor = new SamplePropagationPredictor();
            IPropagationPredictor predictor = samplePredictor;
            var planningService = new PlanningService(predictor);

            var mainViewModel = new MainWindowViewModel(
                inventoryService, planningService, isSampleData: samplePredictor.IsSample);

            desktop.MainWindow = new MainWindow { DataContext = mainViewModel };

            // Load persisted gear and route to wizard / planning once the window exists.
            _ = mainViewModel.InitializeAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

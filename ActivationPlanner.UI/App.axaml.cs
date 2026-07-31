using System;
using System.Net.Http;
using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.Services.Checklists;
using ActivationPlanner.Services.Export;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.Location;
using ActivationPlanner.Services.Missions;
using ActivationPlanner.Services.Planning;
using ActivationPlanner.Services.Pota;
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

            // Mission-type selection and the template/instance checklist engine (Phase 5).
            var missionService = new MissionTypeService();
            var checklistService = new ChecklistService();

            // PDF plan export (QuestPDF).
            var pdfExportService = new PdfExportService();

            // Refresh-on-demand location (Phase 6). Approximate network geo-IP by default; the
            // lookup only runs when the operator presses "Use my location".
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var locationService = new LocationService(new GeoIpLocationProvider(httpClient));

            // POTA public read-only data (Phase 7). Self-spotting exists but is gated off pending
            // POTA confirmation, so it is deliberately not constructed/wired here.
            var potaClient = new PotaClient(httpClient);

            // Shared session selections carried between screens (e.g. mission -> planning framing).
            var sessionState = new SessionState();

            var mainViewModel = new MainWindowViewModel(
                inventoryService, planningService, locationService, missionService, checklistService,
                potaClient, pdfExportService, sessionState, isSampleData: samplePredictor.IsSample);

            desktop.MainWindow = new MainWindow { DataContext = mainViewModel };

            // Load persisted gear and route to wizard / planning once the window exists.
            _ = mainViewModel.InitializeAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

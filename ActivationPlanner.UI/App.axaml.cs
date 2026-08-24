using System;
using System.Net.Http;
using ActivationPlanner.PropagationModel.Antennas;
using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.UI.Composition;
using ActivationPlanner.Services.Checklists;
using ActivationPlanner.Services.Export;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.Location;
using ActivationPlanner.Services.Missions;
using ActivationPlanner.Services.Planning;
using ActivationPlanner.Services.Pota;
using ActivationPlanner.Services.SpaceWeather;
using ActivationPlanner.Services.Weather;
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
        // SharpEngine 3D license (free open-source license, valid for the public ActivationPlanner
        // repo + the 'ActivationPlanner.UI' assembly; covers SharpEngine versions published before
        // 2027-08-17). Set before any 3D control initializes. This open-source key is intended to live
        // in the source, so it is committed here.
        Ab4d.SharpEngine.Licensing.SetLicense(
            licenseOwner: "James Rospopo",
            licenseType: "OpenSourceLicense",
            license: "7597-7D9F-307C-6B98-16E7-2C9D-5703-6C82-ACBC-04DA-29C4-8146-8157-4C46-F64D-C62F-9CC1-FCF1-B641-1F25-DFA9-7558-0F3D-C9D2-A735-0C94-4304-77");

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Composition root — wires the app by hand (no DI container package).
            var store = JsonGearStore.CreateDefault();
            var inventoryService = new GearInventoryService(store);

            // Propagation source. If a bundled/configured VOACAP install is found, predictions come
            // from the real VoacapPropagationEngine (shelled out to voacapl). Otherwise they come
            // from an offline sample stand-in and the planning screen flags them as sample data.
            var voacapPaths = ExternalToolLocator.TryLocateVoacap();
            IPropagationPredictor predictor;
            bool isSampleData;
            if (voacapPaths is not null)
            {
                predictor = VoacapPropagationEngine.Create(voacapPaths.ExecutablePath, voacapPaths.ItshfbcDirectory);
                isSampleData = false;
            }
            else
            {
                predictor = new SamplePropagationPredictor();
                isSampleData = true;
            }
            var planningService = new PlanningService(predictor);

            // Mission-type selection and the template/instance checklist engine (Phase 5).
            var missionService = new MissionTypeService();
            var checklistService = new ChecklistService();

            // PDF plan export (QuestPDF).
            var pdfExportService = new PdfExportService();

            // Antenna pattern source. If a bundled/configured NEC2 engine is found, patterns come
            // from the real NecAntennaModeler (shelled out to nec2++/nec2c). Otherwise a
            // representative offline model stands in behind the same IAntennaPatternSource interface.
            var necPaths = ExternalToolLocator.TryLocateNec();
            IAntennaPatternSource patternSource;
            bool patternIsSample;
            if (necPaths is not null)
            {
                patternSource = NecAntennaModeler.Create(necPaths.ExecutablePath);
                patternIsSample = false;
            }
            else
            {
                patternSource = new SampleAntennaPatternProvider();
                patternIsSample = true;
            }

            // Refresh-on-demand location: prefer an external hardware NMEA GPS (USB/serial) when one
            // is connected, otherwise fall back to approximate network geo-IP (Item #18). The lookup
            // only runs when the operator asks for it.
            var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var locationProvider = new CompositeLocationProvider(
                new SerialGpsLocationProvider(),
                new GeoIpLocationProvider(httpClient));
            var locationService = new LocationService(locationProvider);

            // POTA public read-only data (Phase 7).
            var potaClient = new PotaClient(httpClient);

            // Self-spotting is fully wired (service + UI). ENABLED 2026-08-14 after POTA's helpdesk
            // (Shep, WY8N) confirmed the spot API is undocumented-but-widely-used and tolerated for
            // third parties (with the caveat it can change without notice). Our use is a good citizen:
            // manual only (one press = one spot), self-spot only (spotter == activator), and identified
            // via source="Activation Planner" + a descriptive User-Agent. Flip to false to disable
            // (hides the panel and refuses to send). See docs/POTA_self_spot_permission_request.txt.
            const bool selfSpottingEnabled = true;
            var potaSelfSpotter = new PotaSelfSpotter(httpClient, enabled: selfSpottingEnabled);

            // Live solar/space-weather (SSN/SFI/K) from the public N0NBH feed, to auto-fill the
            // VOACAP sunspot input with real conditions instead of a typed guess.
            var spaceWeatherClient = new SpaceWeatherClient(httpClient);

            // Field weather forecast for the operating location (US National Weather Service).
            var weatherClient = new WeatherClient(httpClient);

            // Shared session selections carried between screens (e.g. mission -> planning framing).
            var sessionState = new SessionState();

            var mainViewModel = new MainWindowViewModel(
                inventoryService, planningService, locationService, missionService, checklistService,
                potaClient, potaSelfSpotter, spaceWeatherClient, weatherClient, pdfExportService,
                patternSource, patternIsSample: patternIsSample, sessionState, isSampleData: isSampleData);

            desktop.MainWindow = new MainWindow { DataContext = mainViewModel };

            // Load persisted gear and route to wizard / planning once the window exists.
            _ = mainViewModel.InitializeAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }
}

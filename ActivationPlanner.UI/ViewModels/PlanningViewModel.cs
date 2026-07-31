using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.Planning;
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

    public PlanningViewModel(PlanningService planning, GearInventoryService inventory, bool isSampleData)
    {
        ArgumentNullException.ThrowIfNull(planning);
        ArgumentNullException.ThrowIfNull(inventory);
        _planning = planning;
        _inventory = inventory;
        IsSampleData = isSampleData;

        var now = DateTime.UtcNow;
        _month = now.Month;
        _year = now.Year;
    }

    /// <summary>True when predictions come from the offline sample stand-in, not a real VOACAP run.</summary>
    public bool IsSampleData { get; }

    public IReadOnlyList<NoiseEnvironment> NoiseOptions { get; } = Enum.GetValues<NoiseEnvironment>();

    // ---- inputs ----

    [ObservableProperty] private double _operatorLatitude = 39.83;
    [ObservableProperty] private double _operatorLongitude = -98.58;
    [ObservableProperty] private double _targetLatitude = 40.0;
    [ObservableProperty] private double _targetLongitude = -105.0;
    [ObservableProperty] private double _sunspotNumber = 70;
    [ObservableProperty] private int _month;
    [ObservableProperty] private int _year;
    [ObservableProperty] private double _transmitPowerWatts = 100;
    [ObservableProperty] private NoiseEnvironment _noise = NoiseEnvironment.Residential;
    [ObservableProperty] private bool _useLongPath;

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

            Bands.Clear();
            foreach (var band in plan.Bands)
                Bands.Add(new BandRecommendationViewModel(band));

            HourAxis.Clear();
            foreach (int h in plan.HoursUtc)
                HourAxis.Add(h);

            Summary = BuildSummary(plan, antennas.Count);
            HasPlan = true;
            OnPropertyChanged(nameof(HasResults));

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

    private CircuitQuery BuildQuery() => new()
    {
        Transmitter = new GeoLocation(OperatorLatitude, OperatorLongitude),
        Receiver = new GeoLocation(TargetLatitude, TargetLongitude),
        Month = Math.Clamp(Month, 1, 12),
        Year = Year,
        SunspotNumber = SunspotNumber,
        Bands = HamBands.All,
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

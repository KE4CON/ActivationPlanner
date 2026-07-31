using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.PropagationModel.Missions;
using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.Location;
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
    private readonly LocationService _location;

    public PlanningViewModel(
        PlanningService planning, GearInventoryService inventory, LocationService location,
        SessionState session, bool isSampleData, bool quickStart = false)
    {
        ArgumentNullException.ThrowIfNull(planning);
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(session);
        _planning = planning;
        _inventory = inventory;
        _location = location;
        IsSampleData = isSampleData;

        var now = DateTime.UtcNow;
        _month = now.Month;
        _year = now.Year;

        // Default the framing from the mission selected elsewhere in the session (overridable here).
        MissionProfile mission = MissionProfiles.For(session.SelectedMission);
        _framing = mission.Framing;
        MissionContext = $"Mission: {mission.DisplayName}";

        // Quick Mode: locate and generate immediately so the operator lands on live recommendations.
        if (quickStart)
            _ = QuickStartAsync();
    }

    private async Task QuickStartAsync()
    {
        await UseMyLocationAsync();  // best-effort; reports its own status/errors
        await GeneratePlanAsync();
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

            IReadOnlySet<int> greyLine = ComputeGreyLineHours(plan.HoursUtc);

            Bands.Clear();
            foreach (var band in plan.Bands)
                Bands.Add(new BandRecommendationViewModel(band, greyLine));

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

    [ObservableProperty] private string? _greyLineInfo;

    /// <summary>Grey-line hours for the operator's location today, used to mark the heatmap.</summary>
    private IReadOnlySet<int> ComputeGreyLineHours(IReadOnlyList<int> hours)
    {
        try
        {
            var location = new GeoLocation(OperatorLatitude, OperatorLongitude);
            var today = DateTime.UtcNow;
            SolarEvents events = SolarCalculator.ForDate(location, today.Year, today.Month, today.Day);

            if (!events.HasGreyLine)
            {
                GreyLineInfo = "No grey line today (polar day/night).";
                return new HashSet<int>();
            }

            GreyLineInfo = $"Grey line ▸ sunrise {HourLabel(events.SunriseUtcHour!.Value)}, " +
                           $"sunset {HourLabel(events.SunsetUtcHour!.Value)} UTC (marked on the strip).";
            return hours.Where(h => SolarCalculator.IsWithinGreyLine(h, events)).ToHashSet();
        }
        catch (ArgumentOutOfRangeException)
        {
            GreyLineInfo = null;
            return new HashSet<int>();
        }
    }

    private static string HourLabel(double hourUtc)
    {
        int hh = (int)hourUtc % 24;
        int mm = (int)Math.Round((hourUtc - Math.Floor(hourUtc)) * 60) % 60;
        return $"{hh:00}:{mm:00}";
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

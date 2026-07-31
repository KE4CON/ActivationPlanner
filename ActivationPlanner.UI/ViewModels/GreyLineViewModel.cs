using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.GreyLine;
using ActivationPlanner.Services.Location;
using ActivationPlanner.Services.Planning;
using ActivationPlanner.UI.ViewModels.GreyLine;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// Dedicated grey-line tab (Item #13, revised from a chart overlay to its own screen). Shows the
/// day's sunrise/sunset for the operator's location and the bands VOACAP already ranks well at
/// those hours — surfacing an existing correlation, never boosting any band's ranking.
/// </summary>
public sealed partial class GreyLineViewModel : ViewModelBase
{
    private readonly PlanningService _planning;
    private readonly LocationService _location;
    private readonly GearInventoryService _inventory;

    private double _latitude = 39.83;
    private double _longitude = -98.58;

    public GreyLineViewModel(PlanningService planning, LocationService location, GearInventoryService inventory)
    {
        ArgumentNullException.ThrowIfNull(planning);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(inventory);
        _planning = planning;
        _location = location;
        _inventory = inventory;

        DateLabel = DateTime.Now.ToString("dddd, MMM d yyyy", CultureInfo.CurrentCulture);
        _ = RefreshAsync();
    }

    public ObservableCollection<GreyLineWindowViewModel> Windows { get; } = [];

    public string DateLabel { get; }

    [ObservableProperty] private string? _locationLabel;
    [ObservableProperty] private string? _sunriseLabel;
    [ObservableProperty] private string? _sunsetLabel;
    [ObservableProperty] private string? _statusMessage;

    // Operator QTH for the map marker (NaN until resolved).
    [ObservableProperty] private double _qthLatitude = double.NaN;
    [ObservableProperty] private double _qthLongitude = double.NaN;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults))]
    private bool _hasGreyLine;

    public bool HasResults => HasGreyLine && Windows.Count > 0;

    private bool CanRefresh => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            await ResolveLocationAsync();

            var location = new GeoLocation(_latitude, _longitude);
            var today = DateTime.UtcNow;
            SolarEvents events = SolarCalculator.ForDate(location, today.Year, today.Month, today.Day);

            SunriseLabel = events.SunriseUtcHour is { } sr ? FormatEventTime(sr) : "—";
            SunsetLabel = events.SunsetUtcHour is { } ss ? FormatEventTime(ss) : "—";

            if (!events.HasGreyLine)
            {
                Windows.Clear();
                HasGreyLine = false;
                StatusMessage = "No grey line today at this location (polar day or night).";
                return;
            }

            var plan = await _planning.PlanAsync(BuildQuery(location), _inventory.Current.Antennas);
            GreyLineReport report = GreyLineAnalysis.Analyze(plan, events);

            Windows.Clear();
            foreach (var window in report.Windows)
            {
                string title = window.Label == "Sunrise"
                    ? $"Around sunrise — {FormatEventTime(window.EventHourUtc)}"
                    : $"Around sunset — {FormatEventTime(window.EventHourUtc)}";
                Windows.Add(new GreyLineWindowViewModel(title, window));
            }

            HasGreyLine = true;
            OnPropertyChanged(nameof(HasResults));
        }
        catch (Exception ex)
        {
            HasGreyLine = false;
            StatusMessage = $"Could not compute the grey line: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ResolveLocationAsync()
    {
        try
        {
            var fix = await _location.RefreshAsync();
            _latitude = fix.Location.LatitudeDeg;
            _longitude = fix.Location.LongitudeDeg;
            LocationLabel = fix.PlaceName ?? $"{_latitude:0.##}, {_longitude:0.##}";
        }
        catch (LocationUnavailableException)
        {
            LocationLabel = $"{_latitude:0.##}, {_longitude:0.##} (default — location unavailable)";
        }

        QthLatitude = _latitude;
        QthLongitude = _longitude;
    }

    private static CircuitQuery BuildQuery(GeoLocation location)
    {
        var now = DateTime.UtcNow;
        return new CircuitQuery
        {
            Transmitter = location,
            Receiver = new GeoLocation(location.LatitudeDeg + 5, location.LongitudeDeg + 15),
            Month = now.Month,
            Year = now.Year,
            SunspotNumber = 70,
            Bands = HamBands.All,
        };
    }

    private static string FormatEventTime(double utcHour)
    {
        var utc = DateTime.SpecifyKind(DateTime.UtcNow.Date.AddHours(utcHour), DateTimeKind.Utc);
        var local = utc.ToLocalTime();
        return $"{utc:HH:mm} UTC  ({local:HH:mm} local)";
    }
}

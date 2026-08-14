using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.Services.Location;
using ActivationPlanner.Services.Weather;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// Field weather tab: the National Weather Service forecast for the operator's current location, so
/// they can plan around wind, rain, and temperature. Active watches/warnings are handled app-wide by
/// the shell (see <see cref="MainWindowViewModel"/>), so they surface on any page — not just here.
/// Resolves location on open (GPS/geo-IP) then fetches; refresh on demand. US only (NWS). Session-local.
/// </summary>
public sealed partial class WeatherViewModel : ViewModelBase
{
    private readonly WeatherClient _weather;
    private readonly LocationService _location;

    public WeatherViewModel(WeatherClient weather, LocationService location)
    {
        ArgumentNullException.ThrowIfNull(weather);
        ArgumentNullException.ThrowIfNull(location);
        _weather = weather;
        _location = location;
        _ = RefreshAsync();
    }

    public ObservableCollection<WeatherPeriod> Periods { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    private bool _isBusy;

    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private string? _locationLabel;

    public bool HasPeriods => Periods.Count > 0;

    private bool CanRefresh => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            LocationFix fix = await _location.RefreshAsync();
            WeatherForecast forecast = await _weather.GetForecastAsync(
                fix.Location.LatitudeDeg, fix.Location.LongitudeDeg);

            LocationLabel = forecast.LocationName
                ?? fix.PlaceName
                ?? $"{fix.Location.LatitudeDeg.ToString("0.##", CultureInfo.CurrentCulture)}, {fix.Location.LongitudeDeg.ToString("0.##", CultureInfo.CurrentCulture)}";

            Periods.Clear();
            foreach (WeatherPeriod p in forecast.Periods)
                Periods.Add(p);
            OnPropertyChanged(nameof(HasPeriods));

            if (Periods.Count == 0)
                StatusMessage = "No forecast periods were returned.";
        }
        catch (LocationUnavailableException ex)
        {
            StatusMessage = $"Couldn't get your location: {ex.Message}";
        }
        catch (WeatherUnavailableException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (WeatherFormatException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not load the forecast: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

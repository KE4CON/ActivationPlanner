using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ActivationPlanner.Services.Pota;
using ActivationPlanner.UI.ViewModels.Pota;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// Live POTA activator spots (Phase 7, read-only) plus operator self-spotting. Self-spotting is
/// fully wired but stays hidden and inert unless <see cref="IsSelfSpotEnabled"/> is true — which is
/// controlled by a single app flag kept OFF until POTA confirms third-party self-spotting is OK.
/// When enabled it posts one self-spot per button press (spotter == activator), never automatically.
/// </summary>
public sealed partial class PotaSpotsViewModel : ViewModelBase
{
    private readonly PotaClient _pota;
    private readonly PotaSelfSpotter _selfSpotter;
    private readonly SessionState _session;

    public PotaSpotsViewModel(PotaClient pota, PotaSelfSpotter selfSpotter, SessionState session)
    {
        ArgumentNullException.ThrowIfNull(pota);
        ArgumentNullException.ThrowIfNull(selfSpotter);
        ArgumentNullException.ThrowIfNull(session);
        _pota = pota;
        _selfSpotter = selfSpotter;
        _session = session;
        _callsign = session.Callsign ?? "";
        _ = RefreshAsync(); // load on open; the operator navigated here intentionally
    }

    public ObservableCollection<PotaSpotItemViewModel> Spots { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    private bool _isBusy;

    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private string? _lastUpdated;

    public bool HasSpots => Spots.Count > 0;

    private bool CanRefresh => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        StatusMessage = null;
        try
        {
            var spots = await _pota.GetActivatorSpotsAsync();

            Spots.Clear();
            foreach (var spot in spots
                         .OrderBy(s => s.Band is null)       // HF (has a band) first
                         .ThenBy(s => s.FrequencyKhz))
            {
                Spots.Add(new PotaSpotItemViewModel(spot));
            }

            LastUpdated = $"{spots.Count} spots • updated {DateTime.Now.ToString("HH:mm", CultureInfo.CurrentCulture)}";
            OnPropertyChanged(nameof(HasSpots));
            if (spots.Count == 0)
                StatusMessage = "No current activator spots.";
        }
        catch (PotaUnavailableException ex)
        {
            StatusMessage = ex.Message;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Could not load spots: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // ---- Self-spotting (hidden + inert unless the app flag enables it) ----

    /// <summary>True only when the app flag turns self-spotting on; drives the panel's visibility.</summary>
    public bool IsSelfSpotEnabled => _selfSpotter.IsEnabled;

    /// <summary>Common POTA operating modes for the picker.</summary>
    public IReadOnlyList<string> Modes { get; } = ["SSB", "CW", "FT8", "FT4", "RTTY", "AM", "FM", "DIGITAL"];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SelfSpotCommand))]
    private string _callsign = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SelfSpotCommand))]
    private string _parkReference = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SelfSpotCommand))]
    private string _frequencyMhz = "";

    [ObservableProperty] private string _selfSpotMode = "SSB";
    [ObservableProperty] private string _selfSpotComments = "";
    [ObservableProperty] private string? _selfSpotStatus;
    [ObservableProperty] private bool _selfSpotSucceeded;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SelfSpotCommand))]
    private bool _isSelfSpotting;

    private bool CanSelfSpot =>
        !IsSelfSpotting
        && !string.IsNullOrWhiteSpace(Callsign)
        && !string.IsNullOrWhiteSpace(ParkReference)
        && double.TryParse(FrequencyMhz, NumberStyles.Float, CultureInfo.InvariantCulture, out double mhz)
        && mhz > 0;

    /// <summary>Post a single self-spot for the operator (one press = one spot; never automatic).</summary>
    [RelayCommand(CanExecute = nameof(CanSelfSpot))]
    private async Task SelfSpotAsync()
    {
        IsSelfSpotting = true;
        SelfSpotSucceeded = false;
        SelfSpotStatus = null;
        try
        {
            double mhz = double.Parse(FrequencyMhz, NumberStyles.Float, CultureInfo.InvariantCulture);
            var request = new PotaSelfSpotRequest
            {
                Activator = Callsign.Trim().ToUpperInvariant(),
                Reference = ParkReference.Trim().ToUpperInvariant(),
                FrequencyKhz = mhz * 1000.0,
                Mode = SelfSpotMode,
                Comments = string.IsNullOrWhiteSpace(SelfSpotComments) ? null : SelfSpotComments.Trim(),
            };

            await _selfSpotter.SubmitAsync(request);

            _session.Callsign = request.Activator; // remember the callsign for the session
            SelfSpotSucceeded = true;
            SelfSpotStatus = $"Spotted {request.Activator} at {request.Reference} on {mhz:0.000} MHz ({SelfSpotMode}).";
            _ = RefreshAsync(); // show the fresh spot in the list
        }
        catch (PotaSelfSpotDisabledException ex)
        {
            SelfSpotStatus = ex.Message;
        }
        catch (PotaUnavailableException ex)
        {
            SelfSpotStatus = $"POTA didn't accept the spot: {ex.Message}";
        }
        catch (Exception ex)
        {
            SelfSpotStatus = $"Could not post the self-spot: {ex.Message}";
        }
        finally
        {
            IsSelfSpotting = false;
        }
    }
}

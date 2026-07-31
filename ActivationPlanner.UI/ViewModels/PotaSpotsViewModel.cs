using System;
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
/// Live POTA activator spots (Phase 7, read-only). Loads current spots from POTA's public
/// unauthenticated feed on open and on demand, HF bands first. No self-spotting here — that
/// feature is built but gated off pending POTA confirmation.
/// </summary>
public sealed partial class PotaSpotsViewModel : ViewModelBase
{
    private readonly PotaClient _pota;

    public PotaSpotsViewModel(PotaClient pota)
    {
        ArgumentNullException.ThrowIfNull(pota);
        _pota = pota;
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
}

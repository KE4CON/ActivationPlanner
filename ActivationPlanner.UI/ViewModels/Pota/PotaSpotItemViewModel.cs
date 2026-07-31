using System.Globalization;
using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.Services.Pota;

namespace ActivationPlanner.UI.ViewModels.Pota;

/// <summary>Display wrapper for a single POTA spot (formatting only). Immutable.</summary>
public sealed class PotaSpotItemViewModel
{
    public PotaSpotItemViewModel(PotaSpot spot)
    {
        Activator = spot.Activator;
        Reference = spot.Reference;
        ParkName = spot.ParkName ?? spot.Reference;
        Frequency = spot.FrequencyMhz.ToString("0.000", CultureInfo.InvariantCulture) + " MHz";
        BandLabel = spot.Band is { } b ? HamBands.DisplayName(b) : "—";
        Mode = spot.Mode ?? string.Empty;
        Spotter = spot.Spotter ?? string.Empty;
        IsSelfSpot = spot.IsSelfSpot;
        LocationDesc = spot.LocationDesc ?? string.Empty;
        Comments = spot.Comments ?? string.Empty;
        Time = spot.SpotTimeUtc is { } t
            ? t.ToString("HH:mm", CultureInfo.InvariantCulture) + " UTC"
            : string.Empty;
    }

    public string Activator { get; }
    public string Reference { get; }
    public string ParkName { get; }
    public string Frequency { get; }
    public string BandLabel { get; }
    public string Mode { get; }
    public string Spotter { get; }
    public bool IsSelfSpot { get; }
    public string LocationDesc { get; }
    public string Comments { get; }
    public string Time { get; }

    public bool HasComments => !string.IsNullOrWhiteSpace(Comments);
}

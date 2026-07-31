using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Antennas;
using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.Services.GearInventory;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// Antenna far-field pattern tab (Item #17, 2D): pick an owned antenna and a band and see its
/// elevation-plane radiation pattern as a polar plot, with peak gain, take-off angle, and feed-point
/// impedance. Pattern data comes from an <see cref="IAntennaPatternSource"/> — the real NEC2 modeler
/// once configured, a representative model otherwise (flagged as such).
/// </summary>
public sealed partial class AntennaPatternViewModel : ViewModelBase
{
    private readonly IAntennaPatternSource _source;

    public AntennaPatternViewModel(IAntennaPatternSource source, GearInventoryService inventory, bool isSampleData)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(inventory);
        _source = source;
        IsSampleData = isSampleData;

        Antennas = inventory.Current.Antennas.ToList();
        _selectedAntenna = Antennas.FirstOrDefault();
        _ = LoadAsync();
    }

    public bool IsSampleData { get; }
    public IReadOnlyList<AntennaProfile> Antennas { get; }
    public IReadOnlyList<HamBand> Bands { get; } = HamBands.All;
    public bool HasAntennas => Antennas.Count > 0;

    [ObservableProperty] private AntennaProfile? _selectedAntenna;
    [ObservableProperty] private HamBand _selectedBand = HamBand.M20;

    [ObservableProperty] private AntennaPattern? _pattern;
    [ObservableProperty] private string? _peakGainLabel;
    [ObservableProperty] private string? _takeoffLabel;
    [ObservableProperty] private string? _impedanceLabel;
    [ObservableProperty] private string? _statusMessage;
    [ObservableProperty] private string? _estimateNote;

    partial void OnSelectedAntennaChanged(AntennaProfile? value) => _ = LoadAsync();
    partial void OnSelectedBandChanged(HamBand value) => _ = LoadAsync();

    private async Task LoadAsync()
    {
        if (SelectedAntenna is not { } antenna)
        {
            Pattern = null;
            return;
        }

        double freq = HamBands.RepresentativeFrequencyMhz(SelectedBand);
        try
        {
            AntennaPattern pattern = await _source.GetPatternAsync(antenna, freq);
            Pattern = pattern;
            PeakGainLabel = $"{pattern.PeakGainDbi:0.0} dBi";
            TakeoffLabel = $"{pattern.TakeoffAngleDeg:0}° elevation";
            ImpedanceLabel = pattern.FeedpointResistanceOhms is { } r
                ? $"{r:0} {(pattern.FeedpointReactanceOhms >= 0 ? "+" : "−")} j{Math.Abs(pattern.FeedpointReactanceOhms ?? 0):0} Ω"
                : null;
            EstimateNote = pattern.EstimateNote;
            StatusMessage = null;
        }
        catch (NotSupportedException ex)
        {
            Pattern = null;
            EstimateNote = null;
            StatusMessage = ex.Message;
        }
        catch (Exception ex)
        {
            Pattern = null;
            EstimateNote = null;
            StatusMessage = $"Could not model this antenna: {ex.Message}";
        }
    }
}

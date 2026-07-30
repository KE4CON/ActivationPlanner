using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.Services.GearInventory;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// The permanent, non-wizard inventory management screen (Item #10): add, edit, or
/// remove any gear or antenna at any time. Every change writes through
/// <see cref="GearInventoryService"/> immediately, so edits are durable without a
/// separate Save step.
/// </summary>
public sealed partial class InventoryEditViewModel : ViewModelBase
{
    private readonly GearInventoryService _service;

    public InventoryEditViewModel(GearInventoryService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
        Sync();
    }

    public IReadOnlyList<GearCategory> GearCategories { get; } = Enum.GetValues<GearCategory>();
    public IReadOnlyList<AntennaCategory> AntennaCategories { get; } = Enum.GetValues<AntennaCategory>();
    public IReadOnlyList<FeedPointType> FeedPoints { get; } = Enum.GetValues<FeedPointType>();

    public ObservableCollection<GearItem> Gear { get; } = [];
    public ObservableCollection<AntennaProfile> Antennas { get; } = [];

    // ---- Gear form -------------------------------------------------------

    private Guid? _editingGearId;

    [ObservableProperty] private GearCategory _gearCategory = GearCategory.Radio;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveGearCommand))]
    private string _gearName = string.Empty;

    [ObservableProperty] private string _gearNotes = string.Empty;

    public string GearButtonText => _editingGearId is null ? "Add gear" : "Save gear";

    private bool CanSaveGear => !string.IsNullOrWhiteSpace(GearName);

    [RelayCommand(CanExecute = nameof(CanSaveGear))]
    private async Task SaveGearAsync()
    {
        var item = new GearItem
        {
            Id = _editingGearId ?? Guid.NewGuid(),
            Category = GearCategory,
            Name = GearName.Trim(),
            Notes = string.IsNullOrWhiteSpace(GearNotes) ? null : GearNotes.Trim(),
        };

        if (_editingGearId is null)
            await _service.AddItemAsync(item);
        else
            await _service.UpdateItemAsync(item);

        ResetGearForm();
        Sync();
    }

    [RelayCommand]
    private void EditGear(GearItem? item)
    {
        if (item is null) return;
        _editingGearId = item.Id;
        GearCategory = item.Category;
        GearName = item.Name;
        GearNotes = item.Notes ?? string.Empty;
        OnPropertyChanged(nameof(GearButtonText));
    }

    [RelayCommand]
    private async Task RemoveGearAsync(GearItem? item)
    {
        if (item is null) return;
        await _service.RemoveItemAsync(item.Id);
        if (_editingGearId == item.Id) ResetGearForm();
        Sync();
    }

    [RelayCommand]
    private void ClearGearForm() => ResetGearForm();

    private void ResetGearForm()
    {
        _editingGearId = null;
        GearCategory = GearCategory.Radio;
        GearName = string.Empty;
        GearNotes = string.Empty;
        OnPropertyChanged(nameof(GearButtonText));
    }

    // ---- Antenna form ----------------------------------------------------

    private Guid? _editingAntennaId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveAntennaCommand))]
    private string _antennaName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowRadials))]
    private AntennaCategory _antennaCategory = AntennaCategory.Vertical;

    [ObservableProperty] private FeedPointType _feedPoint = FeedPointType.BaseFed;
    [ObservableProperty] private double _lengthFeet;
    [ObservableProperty] private double _heightFeet;
    [ObservableProperty] private int? _radialCount;
    [ObservableProperty] private double? _radialLengthFeet;

    public bool ShowRadials => AntennaCategory is AntennaCategory.Vertical or AntennaCategory.Whip;

    public string AntennaButtonText => _editingAntennaId is null ? "Add antenna" : "Save antenna";

    private bool CanSaveAntenna => !string.IsNullOrWhiteSpace(AntennaName);

    [RelayCommand(CanExecute = nameof(CanSaveAntenna))]
    private async Task SaveAntennaAsync()
    {
        var usesRadials = ShowRadials;
        var antenna = new AntennaProfile
        {
            Id = _editingAntennaId ?? Guid.NewGuid(),
            Name = AntennaName.Trim(),
            Category = AntennaCategory,
            FeedPoint = FeedPoint,
            LengthFeet = LengthFeet,
            HeightFeet = HeightFeet,
            RadialCount = usesRadials ? RadialCount : null,
            RadialLengthFeet = usesRadials ? RadialLengthFeet : null,
        };

        if (_editingAntennaId is null)
            await _service.AddAntennaAsync(antenna);
        else
            await _service.UpdateAntennaAsync(antenna);

        ResetAntennaForm();
        Sync();
    }

    [RelayCommand]
    private void EditAntenna(AntennaProfile? antenna)
    {
        if (antenna is null) return;
        _editingAntennaId = antenna.Id;
        AntennaName = antenna.Name;
        AntennaCategory = antenna.Category;
        FeedPoint = antenna.FeedPoint;
        LengthFeet = antenna.LengthFeet;
        HeightFeet = antenna.HeightFeet;
        RadialCount = antenna.RadialCount;
        RadialLengthFeet = antenna.RadialLengthFeet;
        OnPropertyChanged(nameof(AntennaButtonText));
    }

    [RelayCommand]
    private async Task RemoveAntennaAsync(AntennaProfile? antenna)
    {
        if (antenna is null) return;
        await _service.RemoveAntennaAsync(antenna.Id);
        if (_editingAntennaId == antenna.Id) ResetAntennaForm();
        Sync();
    }

    [RelayCommand]
    private void ClearAntennaForm() => ResetAntennaForm();

    private void ResetAntennaForm()
    {
        _editingAntennaId = null;
        AntennaName = string.Empty;
        AntennaCategory = AntennaCategory.Vertical;
        FeedPoint = FeedPointType.BaseFed;
        LengthFeet = 0;
        HeightFeet = 0;
        RadialCount = null;
        RadialLengthFeet = null;
        OnPropertyChanged(nameof(AntennaButtonText));
    }

    // ---- Sync from service ----------------------------------------------

    private void Sync()
    {
        Gear.Clear();
        foreach (var item in _service.Current.Items)
            Gear.Add(item);

        Antennas.Clear();
        foreach (var antenna in _service.Current.Antennas)
            Antennas.Add(antenna);
    }
}

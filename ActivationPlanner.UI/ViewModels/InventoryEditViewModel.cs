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
    [NotifyPropertyChangedFor(nameof(LengthLabel))]
    [NotifyPropertyChangedFor(nameof(LengthHint))]
    [NotifyPropertyChangedFor(nameof(HeightHint))]
    private AntennaCategory _antennaCategory = AntennaCategory.Vertical;

    [ObservableProperty] private FeedPointType _feedPoint = FeedPointType.BaseFed;
    [ObservableProperty] private double _lengthFeet;
    [ObservableProperty] private double _heightFeet;
    [ObservableProperty] private int? _radialCount;
    [ObservableProperty] private double? _radialLengthFeet;

    public bool ShowRadials => AntennaCategory is AntennaCategory.Vertical or AntennaCategory.Whip;

    /// <summary>Length-field label; "length" means a different thing per antenna type. Mirrors the wizard.</summary>
    public string LengthLabel => AntennaCategory switch
    {
        AntennaCategory.Dipole => "Length — tip to tip (ft)",
        AntennaCategory.EndFedHalfWave => "Wire length (ft)",
        AntennaCategory.Vertical or AntennaCategory.Whip => "Element length (ft)",
        AntennaCategory.NvisCrossedDipole => "Leg length (ft)",
        _ => "Length (ft)",
    };

    /// <summary>Plain-language help for the length box, including what happens if it is left at 0.</summary>
    public string LengthHint => AntennaCategory switch
    {
        AntennaCategory.Dipole =>
            "The whole dipole end to end, both legs together. Not sure? Leave it 0 — we'll model a resonant half-wave and label the pattern an estimate.",
        AntennaCategory.EndFedHalfWave =>
            "Total length of the wire. Not sure? Leave it 0 — we'll model a resonant half-wave and label the pattern an estimate.",
        AntennaCategory.Vertical or AntennaCategory.Whip =>
            "Just the vertical element (not the radials). Loaded or modular — a Chameleon, a screwdriver, a Wolf River? Leave it 0 and we'll estimate a quarter-wave.",
        AntennaCategory.NvisCrossedDipole =>
            "Length of ONE of the four wires, measured from the center feed out to its far (staked) end — not all four added up. Chameleon's 4-wire NVIS uses ~45 ft legs. Not sure? Leave it 0 and we'll estimate a resonant quarter-wave leg.",
        _ =>
            "Longest dimension of the radiating element. Leave it 0 to let us estimate a resonant length.",
    };

    /// <summary>Plain-language help for the height box.</summary>
    public string HeightHint => AntennaCategory switch
    {
        AntennaCategory.Vertical or AntennaCategory.Whip =>
            "Height of the feed point (the base) above ground. Standing on the ground? Enter 0.",
        AntennaCategory.NvisCrossedDipole =>
            "Height of the center feed at the top of the mast (the apex) — the four legs slope down from here toward the ground. A typical NVIS mast is ~15 ft.",
        _ =>
            "Height of the feed point — the center of a dipole, the fed end of an end-fed — above ground.",
    };

    /// <summary>Plain-language help for the radial boxes.</summary>
    public string RadialHint =>
        "On-ground wires spread out under a vertical. No radials (or a self-contained antenna)? Leave both at 0.";

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

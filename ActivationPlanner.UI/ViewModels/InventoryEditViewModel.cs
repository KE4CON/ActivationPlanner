using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.Presets;
using ActivationPlanner.UI.ViewModels.Wizard;
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
        : this(service, PresetCatalog.Default) { }

    private readonly GearPresetCatalog _catalog;

    public InventoryEditViewModel(GearInventoryService service, GearPresetCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(catalog);
        _service = service;
        _catalog = catalog;

        var choices = new List<AntennaPresetChoice> { AntennaPresetChoice.Custom };
        choices.AddRange(catalog.Antennas.Select(a => new AntennaPresetChoice(a.DisplayName, a)));
        AntennaPresets = choices;
        _selectedAntennaPreset = choices[0];

        RebuildGearPresets();
        Sync();
    }

    public IReadOnlyList<GearCategory> GearCategories { get; } = Enum.GetValues<GearCategory>();
    public IReadOnlyList<AntennaCategory> AntennaCategories { get; } = Enum.GetValues<AntennaCategory>();
    public IReadOnlyList<FeedPointType> FeedPoints { get; } = Enum.GetValues<FeedPointType>();

    /// <summary>"Start from a model" options: catalog presets plus a Custom / Home-brew escape hatch.</summary>
    public IReadOnlyList<AntennaPresetChoice> AntennaPresets { get; }

    public ObservableCollection<GearItem> Gear { get; } = [];
    public ObservableCollection<AntennaProfile> Antennas { get; } = [];

    // ---- Gear form -------------------------------------------------------

    private Guid? _editingGearId;

    [ObservableProperty] private GearCategory _gearCategory = GearCategory.Radio;

    /// <summary>Preset models for the selected gear category (Custom-only until a category is populated).</summary>
    [ObservableProperty] private IReadOnlyList<GearPresetChoice> _gearPresets = [];
    [ObservableProperty] private GearPresetChoice? _selectedGearPreset;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveGearCommand))]
    private string _gearName = string.Empty;

    [ObservableProperty] private string _gearNotes = string.Empty;

    /// <summary>When the category changes, re-offer the presets that belong to it.</summary>
    partial void OnGearCategoryChanged(GearCategory value) => RebuildGearPresets();

    private void RebuildGearPresets()
    {
        var choices = new List<GearPresetChoice> { GearPresetChoice.Custom };
        if (GearCategory == GearCategory.Radio)
        {
            choices.AddRange(_catalog.Radios.Select(r => new GearPresetChoice(
                r.DisplayName, r.DisplayName, $"{r.Bands}, {r.PowerWatts:0} W. {r.Note}".Trim())));
        }
        else
        {
            choices.AddRange(_catalog.Gear
                .Where(g => g.Category == GearCategory)
                .Select(g => new GearPresetChoice(g.DisplayName, g.DisplayName, g.Note)));
        }
        GearPresets = choices;
        SelectedGearPreset = choices[0];
    }

    /// <summary>Picking a real model prefills the (still editable) gear form; Custom leaves it alone.</summary>
    partial void OnSelectedGearPresetChanged(GearPresetChoice? value)
    {
        if (value?.PrefillName is not { } name)
            return; // Custom
        GearName = name;
        GearNotes = value.PrefillNotes ?? string.Empty;
    }

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
        if (GearPresets.Count > 0)
            SelectedGearPreset = GearPresets[0]; // back to Custom (even if the category didn't change)
        GearName = string.Empty;
        GearNotes = string.Empty;
        OnPropertyChanged(nameof(GearButtonText));
    }

    // ---- Antenna form ----------------------------------------------------

    private Guid? _editingAntennaId;

    [ObservableProperty] private AntennaPresetChoice? _selectedAntennaPreset;

    /// <summary>Provenance / confidence note for the chosen preset (null for Custom).</summary>
    [ObservableProperty] private string? _antennaPresetNote;

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
    [ObservableProperty] private double? _radialHeightFeet;

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
            "Height of the feed point (base) above ground. On the ground? Enter 0. Elevated vertical (e.g. POTA PERformer, ~5 ft)? Enter that height. (For elevated radials, use the separate Radial height box below.)",
        AntennaCategory.NvisCrossedDipole =>
            "Height of the center feed at the top of the mast (the apex) — the four legs slope down from here toward the ground. A typical NVIS mast is ~15 ft.",
        _ =>
            "Height of the feed point — the center of a dipole, the fed end of an end-fed — above ground.",
    };

    /// <summary>Plain-language help for the radial boxes.</summary>
    public string RadialHint =>
        "Wires spread out under a vertical as its 'ground'. No radials (or a self-contained antenna)? Leave count and length at 0. " +
        "Radial height: leave 0 if they lie on the ground; if you raise them on stakes (a few feet up), enter that height — elevated radials lower the take-off angle and need far fewer wires.";

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
            RadialHeightFeet = usesRadials ? RadialHeightFeet : null,
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
        RadialHeightFeet = antenna.RadialHeightFeet;
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

    /// <summary>Picking a real model prefills the (still editable) antenna form; Custom leaves it alone.</summary>
    partial void OnSelectedAntennaPresetChanged(AntennaPresetChoice? value)
    {
        if (value?.Preset is not { } p)
        {
            AntennaPresetNote = null;
            return;
        }

        AntennaName = p.DisplayName;
        AntennaCategory = p.Category;
        FeedPoint = p.FeedPoint;
        LengthFeet = p.LengthFeet;
        HeightFeet = p.HeightFeet;
        RadialCount = p.RadialCount;
        RadialLengthFeet = p.RadialLengthFeet;
        RadialHeightFeet = p.RadialHeightFeet;

        AntennaPresetNote = p.ModelingConfidence == ModelingConfidence.Approximate
            ? $"Approximate model — {p.Note} You can edit any field to match your actual antenna."
            : p.Note;
    }

    [RelayCommand]
    private void ClearAntennaForm() => ResetAntennaForm();

    private void ResetAntennaForm()
    {
        _editingAntennaId = null;
        SelectedAntennaPreset = AntennaPresets[0]; // back to Custom (clears the preset note)
        AntennaName = string.Empty;
        AntennaCategory = AntennaCategory.Vertical;
        FeedPoint = FeedPointType.BaseFed;
        LengthFeet = 0;
        HeightFeet = 0;
        RadialCount = null;
        RadialLengthFeet = null;
        RadialHeightFeet = null;
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

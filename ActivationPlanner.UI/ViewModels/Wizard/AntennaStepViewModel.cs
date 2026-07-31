using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.Services.Presets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActivationPlanner.UI.ViewModels.Wizard;

/// <summary>
/// Antenna entry uses a sub-list-and-detail pattern (Item #10): the operator fills
/// a focused mini-form and adds one antenna at a time to a running list, editing or
/// removing any row. Kept simple per entry while handling a variable number of
/// owned antennas.
/// </summary>
public sealed partial class AntennaStepViewModel : WizardStepViewModel
{
    public override string Title => "Antennas";

    public override string Instructions =>
        "Add each antenna you own. Fill the details, then press Add to list. " +
        "Select a row to edit it.";

    public AntennaStepViewModel() : this(PresetCatalog.Default) { }

    public AntennaStepViewModel(GearPresetCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var choices = new List<AntennaPresetChoice> { AntennaPresetChoice.Custom };
        choices.AddRange(catalog.Antennas.Select(a => new AntennaPresetChoice(a.DisplayName, a)));
        Presets = choices;
        _selectedPreset = choices[0];
    }

    /// <summary>Antennas collected so far.</summary>
    public ObservableCollection<AntennaProfile> Antennas { get; } = [];

    /// <summary>"Start from a model" options: the catalog presets plus a Custom / Home-brew escape hatch.</summary>
    public IReadOnlyList<AntennaPresetChoice> Presets { get; }

    public IReadOnlyList<AntennaCategory> Categories { get; } = Enum.GetValues<AntennaCategory>();

    public IReadOnlyList<FeedPointType> FeedPoints { get; } = Enum.GetValues<FeedPointType>();

    // Id of the row currently being edited; null means the form will add a new one.
    private Guid? _editingId;

    [ObservableProperty]
    private AntennaPresetChoice? _selectedPreset;

    /// <summary>Provenance / confidence note for the chosen preset (null for Custom).</summary>
    [ObservableProperty]
    private string? _presetNote;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowRadials))]
    [NotifyPropertyChangedFor(nameof(LengthLabel))]
    [NotifyPropertyChangedFor(nameof(LengthHint))]
    [NotifyPropertyChangedFor(nameof(HeightHint))]
    private AntennaCategory _selectedCategory = AntennaCategory.Vertical;

    [ObservableProperty]
    private FeedPointType _selectedFeedPoint = FeedPointType.BaseFed;

    [ObservableProperty]
    private double _lengthFeet;

    [ObservableProperty]
    private double _heightFeet;

    [ObservableProperty]
    private int? _radialCount;

    [ObservableProperty]
    private double? _radialLengthFeet;

    /// <summary>Radial fields only make sense for verticals/whips.</summary>
    public bool ShowRadials =>
        SelectedCategory is AntennaCategory.Vertical or AntennaCategory.Whip;

    /// <summary>Field label for the length box — the meaning of "length" depends on the antenna type.</summary>
    public string LengthLabel => SelectedCategory switch
    {
        AntennaCategory.Dipole => "Length — tip to tip (ft)",
        AntennaCategory.EndFedHalfWave => "Wire length (ft)",
        AntennaCategory.Vertical or AntennaCategory.Whip => "Element length (ft)",
        AntennaCategory.NvisCrossedDipole => "Leg length (ft)",
        _ => "Length (ft)",
    };

    /// <summary>Plain-language help for the length box, including what happens if it is left at 0.</summary>
    public string LengthHint => SelectedCategory switch
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
    public string HeightHint => SelectedCategory switch
    {
        AntennaCategory.Vertical or AntennaCategory.Whip =>
            "Height of the feed point (base) above ground. On the ground? Enter 0. Elevated vertical (e.g. POTA PERformer, ~5 ft)? Enter that height — the radials sit at this height too.",
        AntennaCategory.NvisCrossedDipole =>
            "Height of the center feed at the top of the mast (the apex) — the four legs slope down from here toward the ground. A typical NVIS mast is ~15 ft.",
        _ =>
            "Height of the feed point — the center of a dipole, the fed end of an end-fed — above ground.",
    };

    /// <summary>Plain-language help for the radial boxes.</summary>
    public string RadialHint =>
        "Wires spread out under a vertical — they sit at the feed height set above (on the ground, or elevated with the feed). No radials, or a self-contained antenna? Leave both at 0.";

    /// <summary>Add vs. Save Changes label for the form's primary button.</summary>
    public string SaveButtonText => _editingId is null ? "Add to list" : "Save changes";

    /// <summary>Picking a real model prefills the (still editable) form; Custom leaves it alone.</summary>
    partial void OnSelectedPresetChanged(AntennaPresetChoice? value)
    {
        if (value?.Preset is not { } p)
        {
            PresetNote = null;
            return;
        }

        Name = p.DisplayName;
        SelectedCategory = p.Category;
        SelectedFeedPoint = p.FeedPoint;
        LengthFeet = p.LengthFeet;
        HeightFeet = p.HeightFeet;
        RadialCount = p.RadialCount;
        RadialLengthFeet = p.RadialLengthFeet;

        PresetNote = p.ModelingConfidence == ModelingConfidence.Approximate
            ? $"Approximate model — {p.Note} You can edit any field to match your actual antenna."
            : p.Note;
    }

    private bool CanSave => !string.IsNullOrWhiteSpace(Name);

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        var usesRadials = ShowRadials;
        var built = new AntennaProfile
        {
            Id = _editingId ?? Guid.NewGuid(),
            Name = Name.Trim(),
            Category = SelectedCategory,
            FeedPoint = SelectedFeedPoint,
            LengthFeet = LengthFeet,
            HeightFeet = HeightFeet,
            RadialCount = usesRadials ? RadialCount : null,
            RadialLengthFeet = usesRadials ? RadialLengthFeet : null,
        };

        if (_editingId is { } editing)
        {
            for (var i = 0; i < Antennas.Count; i++)
            {
                if (Antennas[i].Id == editing)
                {
                    Antennas[i] = built;
                    break;
                }
            }
        }
        else
        {
            Antennas.Add(built);
        }

        ResetForm();
    }

    [RelayCommand]
    private void Edit(AntennaProfile? antenna)
    {
        if (antenna is null)
            return;

        _editingId = antenna.Id;
        Name = antenna.Name;
        SelectedCategory = antenna.Category;
        SelectedFeedPoint = antenna.FeedPoint;
        LengthFeet = antenna.LengthFeet;
        HeightFeet = antenna.HeightFeet;
        RadialCount = antenna.RadialCount;
        RadialLengthFeet = antenna.RadialLengthFeet;
        OnPropertyChanged(nameof(SaveButtonText));
    }

    [RelayCommand]
    private void Remove(AntennaProfile? antenna)
    {
        if (antenna is null)
            return;

        Antennas.Remove(antenna);
        if (_editingId == antenna.Id)
            ResetForm();
    }

    [RelayCommand]
    private void ClearForm() => ResetForm();

    private void ResetForm()
    {
        _editingId = null;
        SelectedPreset = Presets[0]; // back to Custom (clears the preset note)
        Name = string.Empty;
        SelectedCategory = AntennaCategory.Vertical;
        SelectedFeedPoint = FeedPointType.BaseFed;
        LengthFeet = 0;
        HeightFeet = 0;
        RadialCount = null;
        RadialLengthFeet = null;
        OnPropertyChanged(nameof(SaveButtonText));
    }
}

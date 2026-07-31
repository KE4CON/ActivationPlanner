using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ActivationPlanner.PropagationModel.Gear;
using ActivationPlanner.Services.Presets;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActivationPlanner.UI.ViewModels.Wizard;

/// <summary>
/// A generic "add simple gear to a running list" step, reused for every
/// non-antenna category (Radios, Power, Digital Interfaces, EMCOMM gear). Each
/// entry is just a name plus optional notes.
/// </summary>
public sealed partial class GearListStepViewModel : WizardStepViewModel
{
    public GearListStepViewModel(
        GearCategory category, string title, string instructions, string namePlaceholder,
        GearPresetCatalog? catalog = null)
    {
        Category = category;
        Title = title;
        Instructions = instructions;
        NamePlaceholder = namePlaceholder;

        catalog ??= PresetCatalog.Default;
        var choices = new List<GearPresetChoice> { GearPresetChoice.Custom };
        if (category == GearCategory.Radio)
        {
            choices.AddRange(catalog.Radios.Select(r => new GearPresetChoice(
                r.DisplayName, r.DisplayName, $"{r.Bands}, {r.PowerWatts:0} W. {r.Note}".Trim())));
        }
        else
        {
            choices.AddRange(catalog.Gear
                .Where(g => g.Category == category)
                .Select(g => new GearPresetChoice(g.DisplayName, g.DisplayName, g.Note)));
        }
        Presets = choices;
        _selectedPreset = choices[0];
    }

    public GearCategory Category { get; }

    /// <summary>"Start from a model" options for this step's category (Custom-only until populated).</summary>
    public IReadOnlyList<GearPresetChoice> Presets { get; }

    /// <summary>True when there are real models to pick (hides the picker for empty categories).</summary>
    public bool HasPresets => Presets.Count > 1;

    public override string Title { get; }

    public override string Instructions { get; }

    /// <summary>Hint text for the name field (e.g. "Radio model").</summary>
    public string NamePlaceholder { get; }

    /// <summary>Gear collected so far in this step.</summary>
    public ObservableCollection<GearItem> Items { get; } = [];

    [ObservableProperty] private GearPresetChoice? _selectedPreset;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _newNotes = string.Empty;

    /// <summary>Picking a real model prefills the (still editable) name and notes; Custom leaves them.</summary>
    partial void OnSelectedPresetChanged(GearPresetChoice? value)
    {
        if (value?.PrefillName is not { } name)
            return;
        NewName = name;
        NewNotes = value.PrefillNotes ?? string.Empty;
    }

    private bool CanAdd => !string.IsNullOrWhiteSpace(NewName);

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        Items.Add(new GearItem
        {
            Category = Category,
            Name = NewName.Trim(),
            Notes = string.IsNullOrWhiteSpace(NewNotes) ? null : NewNotes.Trim(),
        });
        SelectedPreset = Presets[0]; // back to Custom
        NewName = string.Empty;
        NewNotes = string.Empty;
    }

    [RelayCommand]
    private void Remove(GearItem? item)
    {
        if (item is not null)
            Items.Remove(item);
    }
}

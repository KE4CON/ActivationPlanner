using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ActivationPlanner.PropagationModel.Gear;
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

    /// <summary>Antennas collected so far.</summary>
    public ObservableCollection<AntennaProfile> Antennas { get; } = [];

    public IReadOnlyList<AntennaCategory> Categories { get; } = Enum.GetValues<AntennaCategory>();

    public IReadOnlyList<FeedPointType> FeedPoints { get; } = Enum.GetValues<FeedPointType>();

    // Id of the row currently being edited; null means the form will add a new one.
    private Guid? _editingId;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowRadials))]
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

    /// <summary>Add vs. Save Changes label for the form's primary button.</summary>
    public string SaveButtonText => _editingId is null ? "Add to list" : "Save changes";

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

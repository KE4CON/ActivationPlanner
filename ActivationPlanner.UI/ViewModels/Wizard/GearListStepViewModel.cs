using System.Collections.ObjectModel;
using ActivationPlanner.PropagationModel.Gear;
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
    public GearListStepViewModel(GearCategory category, string title, string instructions, string namePlaceholder)
    {
        Category = category;
        Title = title;
        Instructions = instructions;
        NamePlaceholder = namePlaceholder;
    }

    public GearCategory Category { get; }

    public override string Title { get; }

    public override string Instructions { get; }

    /// <summary>Hint text for the name field (e.g. "Radio model").</summary>
    public string NamePlaceholder { get; }

    /// <summary>Gear collected so far in this step.</summary>
    public ObservableCollection<GearItem> Items { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private string _newName = string.Empty;

    [ObservableProperty]
    private string _newNotes = string.Empty;

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

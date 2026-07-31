using ActivationPlanner.PropagationModel.Checklists;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ActivationPlanner.UI.ViewModels.Checklists;

/// <summary>
/// A single packable checklist row with session-local check-off state. Wraps an owned or
/// reminder <see cref="ChecklistInstanceItem"/>; check state is deliberately not persisted
/// (stateless-replanning rule) and is reset per planning session.
/// </summary>
public sealed partial class ChecklistItemViewModel : ObservableObject
{
    public ChecklistItemViewModel(ChecklistInstanceItem item)
    {
        Name = item.Name;
        Essential = item.Essential;
        IsOwned = item.Status == ChecklistItemStatus.Owned;
        // Owned gear is confirmed in the kit; a reminder is a personal item to remember to bring.
        StatusLabel = IsOwned ? "in your kit" : "reminder";
    }

    public string Name { get; }
    public bool Essential { get; }
    public bool IsOwned { get; }
    public string StatusLabel { get; }

    [ObservableProperty] private bool _isChecked;
}

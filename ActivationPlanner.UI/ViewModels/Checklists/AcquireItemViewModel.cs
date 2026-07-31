using ActivationPlanner.PropagationModel.Checklists;

namespace ActivationPlanner.UI.ViewModels.Checklists;

/// <summary>
/// A secondary "consider acquiring" row — a mission need the operator doesn't own. Kept in its
/// own list, never mixed into the pack-this tier (CLAUDE.md gear-suggestion rule). Read-only:
/// no check-off, since it isn't something being packed.
/// </summary>
public sealed class AcquireItemViewModel
{
    public AcquireItemViewModel(ChecklistInstanceItem item)
    {
        Name = item.Name;
        Essential = item.Essential;
        Category = ChecklistCategoryGroupViewModel.TitleFor(item.Category);
    }

    public string Name { get; }
    public bool Essential { get; }
    public string Category { get; }
}

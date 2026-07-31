using System.Collections.Generic;
using ActivationPlanner.PropagationModel.Checklists;

namespace ActivationPlanner.UI.ViewModels.Checklists;

/// <summary>
/// A category heading with its packable items — the grouped, icon-friendly presentation
/// called for in Item #5 ("grouped categories, progress indicators", not a flat list).
/// </summary>
public sealed class ChecklistCategoryGroupViewModel
{
    private static readonly IReadOnlyDictionary<ChecklistCategory, string> Titles =
        new Dictionary<ChecklistCategory, string>
        {
            [ChecklistCategory.Power] = "Power",
            [ChecklistCategory.Communications] = "Communications",
            [ChecklistCategory.Documentation] = "Documentation & Traffic Handling",
            [ChecklistCategory.Safety] = "Safety & Comfort",
            [ChecklistCategory.Identification] = "Identification & Coordination",
        };

    public ChecklistCategoryGroupViewModel(ChecklistCategory category, IReadOnlyList<ChecklistItemViewModel> items)
    {
        Title = Titles[category];
        Items = items;
    }

    public string Title { get; }
    public IReadOnlyList<ChecklistItemViewModel> Items { get; }

    /// <summary>Display name for a category, used for the acquire list too.</summary>
    public static string TitleFor(ChecklistCategory category) => Titles[category];
}

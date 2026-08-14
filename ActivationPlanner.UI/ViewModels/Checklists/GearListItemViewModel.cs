using CommunityToolkit.Mvvm.ComponentModel;

namespace ActivationPlanner.UI.ViewModels.Checklists;

/// <summary>
/// An editable row in the tailored gear list: a named item with a display group, a source label
/// ("in your kit" / "reminder" / "added"), and session-local check-off state. Removal is handled by
/// the parent list. Nothing here is persisted (stateless-replanning rule).
/// </summary>
public sealed partial class GearListItemViewModel : ObservableObject
{
    public GearListItemViewModel(string name, string group, bool essential, string statusLabel,
        bool isOwned, bool recommended = false)
    {
        Name = name;
        Group = group;
        Essential = essential;
        StatusLabel = statusLabel;
        IsOwned = isOwned;
        Recommended = recommended;
    }

    public string Name { get; }
    public string Group { get; }
    public bool Essential { get; }
    public string StatusLabel { get; }

    /// <summary>True for a real owned inventory item (vs. a reminder or a free-text add).</summary>
    public bool IsOwned { get; }

    /// <summary>True when this item is especially suited to the selected operation — shows a "Suggested" badge.</summary>
    public bool Recommended { get; }

    /// <summary>Checked once physically packed. Starts unchecked and is not persisted.</summary>
    [ObservableProperty] private bool _isPacked;

    /// <summary>
    /// Whether this item appears on the printed packing list — independent of <see cref="IsPacked"/>
    /// (you print the list before you've packed anything). Defaults to on; not persisted.
    /// </summary>
    [ObservableProperty] private bool _includeInPrint = true;
}

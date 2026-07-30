using CommunityToolkit.Mvvm.ComponentModel;

namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// Base for all view models. Uses CommunityToolkit.Mvvm's <see cref="ObservableObject"/>
/// (matching IcomRigControl) — not ReactiveUI.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
}

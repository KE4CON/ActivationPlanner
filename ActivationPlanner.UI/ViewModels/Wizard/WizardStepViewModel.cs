namespace ActivationPlanner.UI.ViewModels.Wizard;

/// <summary>
/// One step in the first-use setup wizard. Every step is skippable — the operator
/// may own nothing in a category — so a step never blocks Next.
/// </summary>
public abstract class WizardStepViewModel : ViewModelBase
{
    /// <summary>Short heading shown in the wizard header (e.g. "Radios").</summary>
    public abstract string Title { get; }

    /// <summary>One-line guidance shown under the heading.</summary>
    public virtual string Instructions => string.Empty;
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Gear;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Inventory = ActivationPlanner.PropagationModel.Gear.GearInventory;

namespace ActivationPlanner.UI.ViewModels.Wizard;

/// <summary>
/// The required first-use setup wizard: step-by-step with Back/Next, a visible
/// progress indicator, skippable steps, and a Finish summary that commits
/// everything at once (Item #10). Not re-run later — post-setup editing happens on
/// the non-wizard inventory screen.
/// </summary>
public sealed partial class SetupWizardViewModel : ViewModelBase
{
    private readonly Func<Inventory, Task> _onCompleted;
    private readonly Action? _onSkip;
    private readonly List<GearListStepViewModel> _gearSteps;
    private readonly AntennaStepViewModel _antennaStep;
    private readonly SummaryStepViewModel _summaryStep;

    /// <param name="onCompleted">
    /// Invoked with the assembled inventory when the operator presses Finish —
    /// the shell persists it and navigates onward.
    /// </param>
    /// <param name="onSkip">
    /// Optional: invoked when the operator chooses to skip setup and go straight to a quick plan
    /// (Quick Mode). When null, no skip affordance is offered.
    /// </param>
    public SetupWizardViewModel(Func<Inventory, Task> onCompleted, Action? onSkip = null)
    {
        _onCompleted = onCompleted;
        _onSkip = onSkip;

        var radios = new GearListStepViewModel(
            GearCategory.Radio, "Radios",
            "Add the transceivers you'll bring.", "Radio model (e.g. IC-705)");
        _antennaStep = new AntennaStepViewModel();
        var power = new GearListStepViewModel(
            GearCategory.Power, "Power",
            "Batteries, solar panels, generators, power supplies.", "Power source");
        var digital = new GearListStepViewModel(
            GearCategory.DigitalInterface, "Digital Interfaces",
            "Digital-mode interfaces (skip if you run phone/CW only).", "Interface (e.g. Digirig)");
        var emcomm = new GearListStepViewModel(
            GearCategory.Emcomm, "EMCOMM Gear",
            "Go-kit items specific to emergency operating.", "EMCOMM item");
        _summaryStep = new SummaryStepViewModel();

        _gearSteps = [radios, power, digital, emcomm];

        Steps =
        [
            radios,
            _antennaStep,
            power,
            digital,
            emcomm,
            _summaryStep,
        ];
    }

    public IReadOnlyList<WizardStepViewModel> Steps { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CurrentStep))]
    [NotifyPropertyChangedFor(nameof(StepNumber))]
    [NotifyPropertyChangedFor(nameof(ProgressText))]
    [NotifyPropertyChangedFor(nameof(ProgressFraction))]
    [NotifyPropertyChangedFor(nameof(IsLastStep))]
    [NotifyPropertyChangedFor(nameof(IsNotLastStep))]
    [NotifyCanExecuteChangedFor(nameof(BackCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private int _currentIndex;

    public WizardStepViewModel CurrentStep => Steps[CurrentIndex];

    public int StepNumber => CurrentIndex + 1;

    public int StepCount => Steps.Count;

    public string ProgressText => $"Step {StepNumber} of {StepCount}";

    public double ProgressFraction => (double)StepNumber / StepCount;

    public bool IsLastStep => CurrentIndex == Steps.Count - 1;

    public bool IsNotLastStep => !IsLastStep;

    private bool CanGoBack => CurrentIndex > 0;

    [RelayCommand(CanExecute = nameof(CanGoBack))]
    private void Back()
    {
        if (CurrentIndex > 0)
            CurrentIndex--;
    }

    private bool CanGoNext => CurrentIndex < Steps.Count - 1;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void Next()
    {
        if (CurrentIndex >= Steps.Count - 1)
            return;

        CurrentIndex++;

        // Refresh the summary as soon as we arrive on it.
        if (ReferenceEquals(CurrentStep, _summaryStep))
            _summaryStep.Refresh(BuildInventory());
    }

    [RelayCommand]
    private async Task FinishAsync()
    {
        await _onCompleted(BuildInventory());
    }

    /// <summary>True when a skip-to-quick-plan affordance should be shown.</summary>
    public bool CanSkip => _onSkip is not null;

    /// <summary>Skip setup and go straight to Quick Mode.</summary>
    [RelayCommand]
    private void Skip() => _onSkip?.Invoke();

    /// <summary>Assemble the inventory from every step's collected entries.</summary>
    public Inventory BuildInventory() => new()
    {
        Items = _gearSteps.SelectMany(s => s.Items).ToList(),
        Antennas = _antennaStep.Antennas.ToList(),
    };
}

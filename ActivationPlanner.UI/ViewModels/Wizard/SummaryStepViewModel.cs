using System.Collections.ObjectModel;
using System.Linq;
using ActivationPlanner.PropagationModel.Gear;
using CommunityToolkit.Mvvm.ComponentModel;
using Inventory = ActivationPlanner.PropagationModel.Gear.GearInventory;

namespace ActivationPlanner.UI.ViewModels.Wizard;

/// <summary>
/// Final review step. The wizard calls <see cref="Refresh"/> when the operator
/// lands here so the counts reflect everything entered in the earlier steps,
/// before Finish commits the inventory.
/// </summary>
public sealed partial class SummaryStepViewModel : WizardStepViewModel
{
    public override string Title => "Review & finish";

    public override string Instructions =>
        "Here's what will be saved. Go Back to change anything, or press Finish. " +
        "You can edit your inventory any time afterward.";

    [ObservableProperty] private int _radioCount;
    [ObservableProperty] private int _powerCount;
    [ObservableProperty] private int _digitalCount;
    [ObservableProperty] private int _emcommCount;
    [ObservableProperty] private int _antennaCount;
    [ObservableProperty] private bool _isEmpty;

    /// <summary>Category → count rows for a compact summary table.</summary>
    public ObservableCollection<SummaryLine> Lines { get; } = [];

    public void Refresh(Inventory inventory)
    {
        RadioCount = inventory.ItemsIn(GearCategory.Radio).Count();
        PowerCount = inventory.ItemsIn(GearCategory.Power).Count();
        DigitalCount = inventory.ItemsIn(GearCategory.DigitalInterface).Count();
        EmcommCount = inventory.ItemsIn(GearCategory.Emcomm).Count();
        AntennaCount = inventory.Antennas.Count;
        IsEmpty = inventory.IsEmpty;

        Lines.Clear();
        Lines.Add(new SummaryLine("Radios", RadioCount));
        Lines.Add(new SummaryLine("Antennas", AntennaCount));
        Lines.Add(new SummaryLine("Power", PowerCount));
        Lines.Add(new SummaryLine("Digital interfaces", DigitalCount));
        Lines.Add(new SummaryLine("EMCOMM gear", EmcommCount));
    }
}

/// <summary>A single "category: count" row in the summary.</summary>
public sealed record SummaryLine(string Category, int Count);

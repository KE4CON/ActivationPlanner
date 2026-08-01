using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Checklists;
using ActivationPlanner.PropagationModel.Missions;
using ActivationPlanner.Services.Checklists;
using ActivationPlanner.Services.Export;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.Missions;
using ActivationPlanner.UI.ViewModels.Checklists;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// Mission selection + tailored gear list (Phase 5). Choosing an operation type generates a packing
/// list from the operator's <b>actual owned inventory</b> (radios, antennas, power, interfaces,
/// computers), plus personal reminders, tailored by the mission — and a clearly separated "consider
/// acquiring" list for mission needs nothing is owned for. The list is fully editable: check off,
/// remove, add an owned item the generator skipped, or add a free-text one-off. Session-local.
/// </summary>
public sealed partial class MissionChecklistViewModel : ViewModelBase
{
    private readonly ChecklistService _checklist;
    private readonly GearInventoryService _inventory;
    private readonly PdfExportService _pdf;
    private readonly SessionState _session;

    // Owned item name -> display group, so removed items can be re-offered and re-added correctly.
    private Dictionary<string, string> _ownedByGroup = new(StringComparer.OrdinalIgnoreCase);

    public MissionChecklistViewModel(
        MissionTypeService missions, ChecklistService checklist,
        GearInventoryService inventory, PdfExportService pdf, SessionState session)
    {
        ArgumentNullException.ThrowIfNull(missions);
        ArgumentNullException.ThrowIfNull(checklist);
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(pdf);
        ArgumentNullException.ThrowIfNull(session);
        _checklist = checklist;
        _inventory = inventory;
        _pdf = pdf;
        _session = session;

        MissionOptions = missions.Profiles;
        _selectedMission = MissionOptions.FirstOrDefault(p => p.Type == session.SelectedMission)
                           ?? MissionOptions[0];
        Rebuild();
    }

    public IReadOnlyList<MissionProfile> MissionOptions { get; }

    [ObservableProperty]
    private MissionProfile _selectedMission;

    partial void OnSelectedMissionChanged(MissionProfile value)
    {
        _session.SelectedMission = value.Type;
        Rebuild();
    }

    // ---- mission framing ----
    [ObservableProperty] private string _framingLabel = "";
    [ObservableProperty] private string _framingNote = "";
    [ObservableProperty] private string _packingTip = "";
    [ObservableProperty] private string _templateName = "";

    // ---- the editable list ----
    public ObservableCollection<GearListItemViewModel> PackItems { get; } = [];
    public ObservableCollection<GearPlanEntry> AcquireItems { get; } = [];
    [ObservableProperty] private bool _hasAcquireItems;

    /// <summary>Owned items the operator removed from the list — offered for re-adding.</summary>
    public ObservableCollection<string> AddableOwnedItems { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddOwnedItemCommand))]
    private string? _selectedAddableItem;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCustomItemCommand))]
    private string _newItemName = "";

    // ---- progress ----
    [ObservableProperty] private int _packedCount;
    [ObservableProperty] private int _packTotal;
    [ObservableProperty] private double _progressPercent;
    [ObservableProperty] private string _progressText = "";
    [ObservableProperty] private int _essentialRemaining;
    [ObservableProperty] private bool _allEssentialsPacked;

    private void Rebuild()
    {
        FramingLabel = SelectedMission.Framing == PropagationFraming.RegionalNvis
            ? "Regional / NVIS" : "DX / point-to-point";
        FramingNote = SelectedMission.FramingNote;

        GearPlan plan = _checklist.BuildGearPlan(SelectedMission.Type, _inventory.Current);
        TemplateName = plan.TemplateName;
        PackingTip = plan.PackingTip;

        foreach (var row in PackItems)
            row.PropertyChanged -= OnItemChanged;
        PackItems.Clear();

        _ownedByGroup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (GearPlanEntry e in plan.Pack)
        {
            bool owned = e.Source == GearPlanSource.OwnedGear;
            if (owned)
                _ownedByGroup[e.Name] = e.Group;
            AddRow(new GearListItemViewModel(
                e.Name, e.Group, e.Essential, owned ? "in your kit" : "reminder", owned, e.Recommended));
        }

        AcquireItems.Clear();
        foreach (GearPlanEntry e in plan.Acquire)
            AcquireItems.Add(e);
        HasAcquireItems = AcquireItems.Count > 0;

        RefreshAddable();
        UpdateProgress();
    }

    private void AddRow(GearListItemViewModel row)
    {
        row.PropertyChanged += OnItemChanged;
        PackItems.Add(row);
    }

    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GearListItemViewModel.IsPacked))
            UpdateProgress();
    }

    private void RefreshAddable()
    {
        var present = PackItems.Select(i => i.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        AddableOwnedItems.Clear();
        foreach (string name in _ownedByGroup.Keys.Where(n => !present.Contains(n)).OrderBy(n => n))
            AddableOwnedItems.Add(name);
    }

    [RelayCommand]
    private void Remove(GearListItemViewModel? item)
    {
        if (item is null)
            return;
        item.PropertyChanged -= OnItemChanged;
        PackItems.Remove(item);
        RefreshAddable();
        UpdateProgress();
    }

    private bool CanAddOwned => SelectedAddableItem is not null;

    [RelayCommand(CanExecute = nameof(CanAddOwned))]
    private void AddOwnedItem()
    {
        if (SelectedAddableItem is not { } name || !_ownedByGroup.TryGetValue(name, out string? group))
            return;
        AddRow(new GearListItemViewModel(name, group, essential: false, "in your kit", isOwned: true));
        RefreshAddable();
        UpdateProgress();
    }

    private bool CanAddCustom => !string.IsNullOrWhiteSpace(NewItemName);

    [RelayCommand(CanExecute = nameof(CanAddCustom))]
    private void AddCustomItem()
    {
        AddRow(new GearListItemViewModel(NewItemName.Trim(), "Added by you", essential: false, "added", isOwned: false));
        NewItemName = "";
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        PackTotal = PackItems.Count;
        PackedCount = PackItems.Count(i => i.IsPacked);
        ProgressPercent = PackTotal == 0 ? 0 : (double)PackedCount / PackTotal * 100;
        ProgressText = $"{PackedCount} of {PackTotal} packed";
        EssentialRemaining = PackItems.Count(i => i.Essential && !i.IsPacked);
        AllEssentialsPacked = EssentialRemaining == 0;
        OnPropertyChanged(nameof(CanPrint));
    }

    // ---- print the selected gear (checked items only) ----

    /// <summary>Enabled once at least one item is checked — the print includes only checked items.</summary>
    public bool CanPrint => PackItems.Any(i => i.IsPacked);

    /// <summary>Default file name for the packing-list PDF (no spaces, safe for a save dialog).</summary>
    public string SuggestedPrintFileName =>
        $"{TemplateName.Replace(' ', '-')}-packing-list.pdf";

    /// <summary>Build the print request from the checked items only, grouped in display order.</summary>
    public GearListPrintRequest BuildPrintRequest() => new()
    {
        Title = $"{TemplateName} — packing list",
        Subtitle = SelectedMission.DisplayName,
        PackingTip = PackingTip,
        Items = PackItems.Where(i => i.IsPacked)
            .Select(i => new GearPrintItem { Name = i.Name, Group = i.Group, Essential = i.Essential })
            .ToList(),
    };

    /// <summary>Render the selected-items packing list to <paramref name="output"/> as PDF.</summary>
    public Task PrintSelectedAsync(Stream output) => _pdf.WriteGearListAsync(BuildPrintRequest(), output);

    /// <summary>Uncheck everything — "reset for next time" without losing the edited list.</summary>
    [RelayCommand]
    private void Reset()
    {
        foreach (var item in PackItems)
            item.IsPacked = false;
    }
}

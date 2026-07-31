using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using ActivationPlanner.PropagationModel.Checklists;
using ActivationPlanner.PropagationModel.Missions;
using ActivationPlanner.Services.Checklists;
using ActivationPlanner.Services.GearInventory;
using ActivationPlanner.Services.Missions;
using ActivationPlanner.UI.ViewModels.Checklists;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// Mission selection + packing checklist (Phase 5). Choosing a mission type selects its
/// checklist template and shows the operator's owned gear as "pack this" (grouped, with
/// check-off and progress) alongside a clearly separated "consider acquiring" list. Also
/// surfaces the mission's default propagation framing. Check-off state is session-local.
/// </summary>
public sealed partial class MissionChecklistViewModel : ViewModelBase
{
    private readonly ChecklistService _checklist;
    private readonly GearInventoryService _inventory;
    private readonly SessionState _session;

    public MissionChecklistViewModel(
        MissionTypeService missions, ChecklistService checklist,
        GearInventoryService inventory, SessionState session)
    {
        ArgumentNullException.ThrowIfNull(missions);
        ArgumentNullException.ThrowIfNull(checklist);
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(session);
        _checklist = checklist;
        _inventory = inventory;
        _session = session;

        MissionOptions = missions.Profiles;
        // Restore the mission carried over from elsewhere in the session.
        _selectedMission = MissionOptions.FirstOrDefault(p => p.Type == session.SelectedMission)
                           ?? MissionOptions[0];
        Rebuild();
    }

    public IReadOnlyList<MissionProfile> MissionOptions { get; }

    [ObservableProperty]
    private MissionProfile _selectedMission;

    partial void OnSelectedMissionChanged(MissionProfile value)
    {
        // Carry the choice so the planning screen can default its framing from it.
        _session.SelectedMission = value.Type;
        Rebuild();
    }

    // ---- mission framing ----

    [ObservableProperty] private string _framingLabel = "";
    [ObservableProperty] private string _framingNote = "";

    // ---- checklist ----

    public ObservableCollection<ChecklistCategoryGroupViewModel> PackGroups { get; } = [];
    public ObservableCollection<AcquireItemViewModel> AcquireItems { get; } = [];

    [ObservableProperty] private string _templateName = "";
    [ObservableProperty] private bool _hasAcquireItems;

    // ---- progress ----

    [ObservableProperty] private int _packedCount;
    [ObservableProperty] private int _packTotal;
    [ObservableProperty] private double _progressPercent;
    [ObservableProperty] private string _progressText = "";
    [ObservableProperty] private int _essentialRemaining;

    /// <summary>True when every essential pack item is checked — safe to head out.</summary>
    [ObservableProperty] private bool _allEssentialsPacked;

    private IReadOnlyList<ChecklistItemViewModel> _allPackItems = [];

    private void Rebuild()
    {
        FramingLabel = SelectedMission.Framing == PropagationFraming.RegionalNvis
            ? "Regional / NVIS" : "DX / point-to-point";
        FramingNote = SelectedMission.FramingNote;

        ChecklistInstance instance = _checklist.Build(SelectedMission.Type, _inventory.Current);
        TemplateName = instance.TemplateName;

        // Detach old handlers before rebuilding so stale rows don't keep firing progress updates.
        foreach (var item in _allPackItems)
            item.PropertyChanged -= OnItemChanged;

        PackGroups.Clear();
        var packItemVms = new List<ChecklistItemViewModel>();

        // Group packable items by category, preserving enum order.
        foreach (ChecklistCategory category in Enum.GetValues<ChecklistCategory>())
        {
            var rows = instance.PackItems
                .Where(i => i.Category == category)
                .Select(i => new ChecklistItemViewModel(i))
                .ToList();
            if (rows.Count == 0)
                continue;

            foreach (var row in rows)
            {
                row.PropertyChanged += OnItemChanged;
                packItemVms.Add(row);
            }
            PackGroups.Add(new ChecklistCategoryGroupViewModel(category, rows));
        }

        _allPackItems = packItemVms;

        AcquireItems.Clear();
        foreach (var item in instance.AcquireItems)
            AcquireItems.Add(new AcquireItemViewModel(item));
        HasAcquireItems = AcquireItems.Count > 0;

        UpdateProgress();
    }

    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ChecklistItemViewModel.IsChecked))
            UpdateProgress();
    }

    private void UpdateProgress()
    {
        PackTotal = _allPackItems.Count;
        PackedCount = _allPackItems.Count(i => i.IsChecked);
        ProgressPercent = PackTotal == 0 ? 0 : (double)PackedCount / PackTotal * 100;
        ProgressText = $"{PackedCount} of {PackTotal} packed";
        EssentialRemaining = _allPackItems.Count(i => i.Essential && !i.IsChecked);
        AllEssentialsPacked = EssentialRemaining == 0;
    }

    /// <summary>Uncheck everything — "reset for next time" without losing the checklist.</summary>
    [RelayCommand]
    private void Reset()
    {
        foreach (var item in _allPackItems)
            item.IsChecked = false;
    }
}

using ActivationPlanner.PropagationModel.Missions;

namespace ActivationPlanner.UI;

/// <summary>
/// Lightweight per-run state shared across screens (the screens are recreated on navigation,
/// so a shared holder carries selections between them). Currently just the chosen mission type,
/// so the planning screen can default its propagation framing from the mission picked on the
/// mission &amp; checklist screen. Session-local; nothing here is persisted (stateless-replanning rule).
/// </summary>
public sealed class SessionState
{
    /// <summary>The mission type the operator most recently selected. Defaults to General.</summary>
    public MissionType SelectedMission { get; set; } = MissionType.General;

    /// <summary>The operator's callsign, remembered within the session (e.g. for POTA self-spotting).</summary>
    public string? Callsign { get; set; }
}

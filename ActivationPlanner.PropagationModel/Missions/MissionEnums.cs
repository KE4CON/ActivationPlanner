namespace ActivationPlanner.PropagationModel.Missions;

/// <summary>
/// The kind of operating session being planned. Chosen up front (Item #7): it selects which
/// checklist <see cref="Checklists.ChecklistTemplate"/> to start from, drives owned-gear
/// suggestions, and sets the default propagation <see cref="PropagationFraming"/>.
/// </summary>
public enum MissionType
{
    /// <summary>Parks On The Air — portable, single-operator, DX/hunter-facing.</summary>
    Pota,

    /// <summary>Summits On The Air — weight-critical portable operating from a summit.</summary>
    Sota,

    /// <summary>ARRL Field Day — multi-band, longer-duration, more support gear.</summary>
    FieldDay,

    /// <summary>Emergency communications — regional/NVIS net focus, load-bearing go-kit.</summary>
    Emcomm,

    /// <summary>General / casual operating — no mission-specific emphasis.</summary>
    General,
}

/// <summary>
/// How the propagation question is framed for a mission. This changes what is *asked* of
/// VOACAP (the receive point represented), never the physics-based answer — no band is ever
/// assumed to work (Item #7). The operator can override the framing regardless of mission.
/// </summary>
public enum PropagationFraming
{
    /// <summary>DX / point-to-point: a distant target (hunters, a specific DX entity).</summary>
    DxPointToPoint,

    /// <summary>
    /// Regional / NVIS: a near-in receive point representing a served agency or net-control
    /// station, for short-to-medium-haul coverage rather than DX.
    /// </summary>
    RegionalNvis,
}

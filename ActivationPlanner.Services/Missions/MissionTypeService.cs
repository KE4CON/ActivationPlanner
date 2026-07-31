using ActivationPlanner.PropagationModel.Missions;

namespace ActivationPlanner.Services.Missions;

/// <summary>
/// Mission-type selection (Item #7): exposes the available <see cref="MissionProfile"/>s so the
/// UI can offer them, and resolves the profile for a chosen type. A mission drives which
/// checklist template is used (see <c>ChecklistService</c>) and the default propagation framing
/// — it changes the question asked of VOACAP, never the physics-based answer.
/// <para>Layer-3 service: consumes PropagationModel only. Peer to the checklist and gear services.</para>
/// </summary>
public sealed class MissionTypeService
{
    /// <summary>All mission profiles, in the order they should be offered.</summary>
    public IReadOnlyList<MissionProfile> Profiles => MissionProfiles.All;

    /// <summary>The profile for <paramref name="type"/>.</summary>
    public MissionProfile Get(MissionType type) => MissionProfiles.For(type);
}

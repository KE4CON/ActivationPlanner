using System;
using System.Linq;
using ActivationPlanner.PropagationModel.Missions;
using ActivationPlanner.Services.Missions;
using Xunit;

namespace ActivationPlanner.Services.Tests.Missions;

public sealed class MissionTypeServiceTests
{
    [Fact]
    public void Offers_a_profile_for_every_mission_type()
    {
        var svc = new MissionTypeService();

        foreach (MissionType type in Enum.GetValues<MissionType>())
        {
            var profile = svc.Get(type);
            Assert.Equal(type, profile.Type);
            Assert.False(string.IsNullOrWhiteSpace(profile.DisplayName));
            Assert.False(string.IsNullOrWhiteSpace(profile.FramingNote));
        }

        Assert.Equal(Enum.GetValues<MissionType>().Length, svc.Profiles.Count);
    }

    [Fact]
    public void Emcomm_defaults_to_regional_nvis_framing()
    {
        var svc = new MissionTypeService();
        Assert.Equal(PropagationFraming.RegionalNvis, svc.Get(MissionType.Emcomm).Framing);
    }

    [Fact]
    public void Dx_missions_default_to_point_to_point_framing()
    {
        var svc = new MissionTypeService();

        Assert.Equal(PropagationFraming.DxPointToPoint, svc.Get(MissionType.Pota).Framing);
        Assert.Equal(PropagationFraming.DxPointToPoint, svc.Get(MissionType.Sota).Framing);
        Assert.Equal(PropagationFraming.DxPointToPoint, svc.Get(MissionType.FieldDay).Framing);
        Assert.Equal(PropagationFraming.DxPointToPoint, svc.Get(MissionType.General).Framing);
    }
}

using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Missions;

namespace ActivationPlanner.PropagationModel.Tests.Bands;

public sealed class PropagationFramingBandsTests
{
    [Fact]
    public void Dx_framing_asks_about_all_hf_bands()
    {
        Assert.Equal(HamBands.All, PropagationFramingBands.For(PropagationFraming.DxPointToPoint));
    }

    [Fact]
    public void Regional_nvis_framing_asks_about_the_low_bands_only()
    {
        var bands = PropagationFramingBands.For(PropagationFraming.RegionalNvis);
        Assert.Equal([HamBand.M80, HamBand.M60, HamBand.M40, HamBand.M30], bands);
    }

    [Fact]
    public void Nvis_bands_are_a_subset_of_all_hf_bands()
    {
        Assert.All(PropagationFramingBands.Nvis, b => Assert.Contains(b, HamBands.All));
    }

    [Fact]
    public void Nvis_excludes_the_high_bands()
    {
        Assert.DoesNotContain(HamBand.M10, PropagationFramingBands.Nvis);
        Assert.DoesNotContain(HamBand.M20, PropagationFramingBands.Nvis);
    }
}

using System.Linq;
using ActivationPlanner.PropagationModel.Bands;

namespace ActivationPlanner.PropagationModel.Tests.Bands;

public sealed class UsBandPlanTests
{
    [Fact]
    public void Every_band_is_complete()
    {
        Assert.NotEmpty(UsBandPlan.Bands);
        Assert.All(UsBandPlan.Bands, b =>
        {
            Assert.False(string.IsNullOrWhiteSpace(b.Name));
            Assert.False(string.IsNullOrWhiteSpace(b.Range));
            Assert.False(string.IsNullOrWhiteSpace(b.Summary));
            Assert.NotEmpty(b.Segments);
            Assert.All(b.Segments, s =>
            {
                Assert.False(string.IsNullOrWhiteSpace(s.Range));
                Assert.False(string.IsNullOrWhiteSpace(s.Modes));
                Assert.False(string.IsNullOrWhiteSpace(s.Licenses));
            });
        });
    }

    [Fact]
    public void Covers_hf_through_uhf()
    {
        var names = UsBandPlan.Bands.Select(b => b.Name).ToList();
        Assert.Contains("160m", names);
        Assert.Contains("10m", names);
        Assert.Contains("70cm", names);
    }

    [Fact]
    public void Forty_meter_general_phone_starts_at_7_175()
    {
        BandPlanBand forty = UsBandPlan.Bands.Single(b => b.Name == "40m");
        // The General/Adv/Extra phone segment is 7.175–7.300; 7.125–7.175 is Advanced/Extra only.
        Assert.Contains(forty.Segments, s => s.Range == "7.175–7.300" && s.Licenses.Contains("General"));
        Assert.Contains(forty.Segments, s => s.Range == "7.125–7.175" && !s.Licenses.Contains("General"));
    }

    [Fact]
    public void Thirty_meters_is_marked_no_phone()
    {
        BandPlanBand thirty = UsBandPlan.Bands.Single(b => b.Name == "30m");
        // No voice on 30m: no segment permits SSB, and it's called out as "no phone".
        Assert.All(thirty.Segments, s => Assert.DoesNotContain("SSB", s.Modes));
        Assert.Contains(thirty.Segments, s => s.Modes.Contains("no phone"));
    }
}

using ActivationPlanner.PropagationModel.Bands;

namespace ActivationPlanner.PropagationModel.Tests.Bands;

public sealed class CallingFrequenciesTests
{
    [Fact]
    public void Twenty_meters_lists_ssb_cw_and_ft8()
    {
        string s = CallingFrequencies.Summary(HamBand.M20);
        Assert.Contains("SSB 14.285", s);
        Assert.Contains("CW 14.060", s);
        Assert.Contains("FT8 14.074", s);
    }

    [Fact]
    public void Thirty_meters_has_no_phone()
    {
        // 30m is CW/data only — no SSB entry.
        string s = CallingFrequencies.Summary(HamBand.M30);
        Assert.DoesNotContain("SSB", s);
        Assert.Contains("CW 10.116", s);
        Assert.Contains("FT8 10.136", s);
    }

    [Fact]
    public void Every_band_returns_at_least_one_frequency()
    {
        foreach (HamBand band in HamBands.All)
            Assert.False(string.IsNullOrWhiteSpace(CallingFrequencies.Summary(band)), $"{band} had no calling freq");
    }
}

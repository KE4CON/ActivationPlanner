using ActivationPlanner.PropagationModel.Bands;

namespace ActivationPlanner.PropagationModel.Tests.Bands;

public sealed class BandForFrequencyTests
{
    [Theory]
    [InlineData(14.295, HamBand.M20)]   // a 20m SSB POTA spot
    [InlineData(7.032, HamBand.M40)]
    [InlineData(3.573, HamBand.M80)]
    [InlineData(28.074, HamBand.M10)]
    [InlineData(10.136, HamBand.M30)]
    [InlineData(18.100, HamBand.M17)]
    public void Maps_in_band_frequency_to_its_band(double mhz, HamBand expected)
    {
        Assert.Equal(expected, HamBands.BandForFrequencyMhz(mhz));
    }

    [Theory]
    [InlineData(146.520)]  // 2m
    [InlineData(50.125)]   // 6m
    [InlineData(1.000)]    // below 160m
    [InlineData(9.000)]    // between 40m and 30m (no band)
    public void Returns_null_outside_the_hf_bands(double mhz)
    {
        Assert.Null(HamBands.BandForFrequencyMhz(mhz));
    }

    [Fact]
    public void Band_edges_are_inclusive()
    {
        Assert.Equal(HamBand.M20, HamBands.BandForFrequencyMhz(14.00));
        Assert.Equal(HamBand.M20, HamBands.BandForFrequencyMhz(14.35));
    }
}

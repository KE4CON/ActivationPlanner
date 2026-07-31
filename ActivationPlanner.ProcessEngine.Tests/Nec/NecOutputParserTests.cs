using ActivationPlanner.ProcessEngine.Nec;
using ActivationPlanner.ProcessEngine.Tests.Fixtures;

namespace ActivationPlanner.ProcessEngine.Tests.Nec;

// Fixture is a real nec2c run of a 20m dipole at 10m over average ground (14.1 MHz).
public sealed class NecOutputParserTests
{
    private static NecRawResult Parse() =>
        new NecOutputParser().Parse(NecFixtures.DipoleOutputText());

    [Fact]
    public void Parses_feed_point_impedance()
    {
        var z = Parse().Impedance;
        Assert.NotNull(z);
        Assert.Equal(66.289, z!.ResistanceOhms, precision: 3);
        Assert.Equal(-47.059, z.ReactanceOhms, precision: 3);
    }

    [Fact]
    public void Parses_the_full_elevation_cut()
    {
        var pattern = Parse().Pattern;
        // theta 0..90 in 5-degree steps (the 90-degree zenith row has a blank SENSE field).
        Assert.Equal(19, pattern.Count);
        Assert.Equal(0.0, pattern[0].ThetaDeg);
        Assert.Equal(90.0, pattern[^1].ThetaDeg);
    }

    [Fact]
    public void Finds_the_peak_gain_sample()
    {
        var peak = Parse().PeakGain;
        Assert.NotNull(peak);
        Assert.Equal(7.05, peak!.TotalGainDb, precision: 2);
        Assert.Equal(60.0, peak.ThetaDeg); // 30-degree take-off for a dipole ~0.5 wavelength up
    }

    [Fact]
    public void Reads_the_gain_columns_positionally()
    {
        // nec2c labels the gain columns MAJOR/MINOR/TOTAL; we read them positionally, so
        // VerticalGainDb=major, HorizontalGainDb=minor. This dipole is linearly polarized, so the
        // minor axis is the NEC "no field" sentinel and the major axis equals the total gain.
        var peak = Parse().Pattern.Single(s => s.ThetaDeg == 60.0);
        Assert.Equal(7.05, peak.VerticalGainDb, precision: 2);   // major
        Assert.Equal(-999.99, peak.HorizontalGainDb, precision: 2); // minor (linear pol)
        Assert.Equal(7.05, peak.TotalGainDb, precision: 2);
    }

    [Fact]
    public void Throws_when_no_radiation_pattern_present()
    {
        var parser = new NecOutputParser();
        Assert.Throws<FormatException>(() => parser.Parse("no nec sections here\njust text\n"));
    }
}

using ActivationPlanner.ProcessEngine.Nec;
using ActivationPlanner.ProcessEngine.Tests.Fixtures;

namespace ActivationPlanner.ProcessEngine.Tests.Nec;

public sealed class NecOutputParserTests
{
    private static NecRawResult Parse() =>
        new NecOutputParser().Parse(NecFixtures.DipoleOutputText());

    [Fact]
    public void Parses_feed_point_impedance()
    {
        var z = Parse().Impedance;
        Assert.NotNull(z);
        Assert.Equal(79.889, z!.ResistanceOhms, precision: 3);
        Assert.Equal(16.298, z.ReactanceOhms, precision: 3);
    }

    [Fact]
    public void Parses_the_full_elevation_cut()
    {
        var pattern = Parse().Pattern;
        Assert.Equal(19, pattern.Count); // theta 0..90 in 5-degree steps
        Assert.Equal(0.0, pattern[0].ThetaDeg);
        Assert.Equal(90.0, pattern[^1].ThetaDeg);
    }

    [Fact]
    public void Finds_the_peak_gain_sample()
    {
        var peak = Parse().PeakGain;
        Assert.NotNull(peak);
        Assert.Equal(6.20, peak!.TotalGainDb, precision: 2);
        Assert.Equal(65.0, peak.ThetaDeg); // elevation 25 deg for a dipole at 10 m
    }

    [Fact]
    public void Reads_vertical_and_horizontal_gain_columns()
    {
        var peak = Parse().Pattern.Single(s => s.ThetaDeg == 65.0);
        Assert.Equal(6.14, peak.HorizontalGainDb, precision: 2);
        Assert.Equal(-14.50, peak.VerticalGainDb, precision: 2);
    }

    [Fact]
    public void Throws_when_no_radiation_pattern_present()
    {
        var parser = new NecOutputParser();
        Assert.Throws<FormatException>(() => parser.Parse("no nec sections here\njust text\n"));
    }
}

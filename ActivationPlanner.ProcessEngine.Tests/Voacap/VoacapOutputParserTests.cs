using ActivationPlanner.ProcessEngine.Tests.Fixtures;
using ActivationPlanner.ProcessEngine.Voacap;

namespace ActivationPlanner.ProcessEngine.Tests.Voacap;

/// <summary>
/// Pins the output parser to real VOACAP output by parsing the known-good
/// <c>tests/p2p/test01.out</c> and asserting exact values read from the file.
/// </summary>
public sealed class VoacapOutputParserTests
{
    private static VoacapRawPrediction Parse() =>
        new VoacapOutputParser().Parse(VoacapFixtures.SampleOutputText());

    [Fact]
    public void Parses_all_twentyfour_hour_blocks()
    {
        var result = Parse();
        Assert.Equal(24, result.Hours.Count);
        Assert.Equal(1.0, result.Hours[0].HourUtc);
        Assert.Equal(24.0, result.Hours[^1].HourUtc);
    }

    [Fact]
    public void Parses_circuit_metadata_from_page_header()
    {
        var result = Parse();
        Assert.Equal(100.0, result.SunspotNumber);
        Assert.Equal("TANGIER, Morocco", result.TransmitterLabel);
        Assert.Equal("BELGRADE", result.ReceiverLabel);
    }

    [Fact]
    public void First_hour_block_has_expected_muf_and_frequencies()
    {
        var hour1 = Parse().Hours[0];
        Assert.Equal(16.2, hour1.MufMhz);
        // Nine evaluated frequencies (the two 0.0 padding slots are dropped).
        Assert.Equal(9, hour1.Samples.Count);
        Assert.Equal(6.1, hour1.Samples[0].FrequencyMhz);
        Assert.Equal(25.9, hour1.Samples[^1].FrequencyMhz);
    }

    [Fact]
    public void Reads_exact_parameter_values_for_first_hour_first_frequency()
    {
        // From test01.out hour 1.0, the 6.1 MHz column (first frequency slot):
        //   MODE=1F2  REL=1.00  SNR=106  S DBW=-43  N DBW=-149  LOSS=100  TANGLE=7.8  MUFday=1.00
        var s = Parse().Hours[0].Samples[0];
        Assert.Equal("1F2", s.Mode);
        Assert.Equal(1.00, s.Reliability);
        Assert.Equal(106, s.Snr);
        Assert.Equal(-43, s.SignalPowerDbw);
        Assert.Equal(-149, s.NoisePowerDbw);
        Assert.Equal(100, s.PathLossDb);
        Assert.Equal(7.8, s.TakeoffAngleDeg);
        Assert.Equal(1.00, s.MufDays);
    }

    [Fact]
    public void Reads_low_reliability_high_band_at_night()
    {
        // Hour 1.0, 25.9 MHz (last slot): REL = 0.00 in the fixture.
        var s = Parse().Hours[0].Samples[^1];
        Assert.Equal(25.9, s.FrequencyMhz);
        Assert.Equal(0.00, s.Reliability);
    }

    [Fact]
    public void Raw_row_preserves_every_parameter_label()
    {
        var s = Parse().Hours[0].Samples[0];
        // Full set of METHOD 30 parameter rows should be captured.
        Assert.Contains("REL", s.RawRow.Keys);
        Assert.Contains("S DBW", s.RawRow.Keys);
        Assert.Contains("SNRxx", s.RawRow.Keys);
        Assert.Contains("MUFday", s.RawRow.Keys);
        Assert.Equal(21, s.RawRow.Count); // 21 labeled parameter rows per block
    }

    [Fact]
    public void Every_hour_has_nine_frequency_samples()
    {
        var result = Parse();
        Assert.All(result.Hours, h => Assert.Equal(9, h.Samples.Count));
    }

    [Fact]
    public void Throws_on_output_without_hour_blocks()
    {
        var parser = new VoacapOutputParser();
        Assert.Throws<FormatException>(() => parser.Parse("no voacap blocks here\njust text\n"));
    }
}

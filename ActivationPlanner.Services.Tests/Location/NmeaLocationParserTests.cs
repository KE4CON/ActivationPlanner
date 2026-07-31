using ActivationPlanner.Services.Location;

namespace ActivationPlanner.Services.Tests.Location;

public sealed class NmeaLocationParserTests
{
    [Fact]
    public void Parses_a_valid_gga_fix()
    {
        var fix = NmeaLocationParser.TryParse("$GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47");
        Assert.NotNull(fix);
        Assert.Equal(48.1173, fix!.Value.LatitudeDeg, precision: 3);
        Assert.Equal(11.5167, fix.Value.LongitudeDeg, precision: 3);
    }

    [Fact]
    public void Parses_a_valid_rmc_fix_with_southern_western_hemispheres()
    {
        // A southern/western position (Sydney-ish): S / E here flipped to S / W for coverage.
        var fix = NmeaLocationParser.TryParse("$GPRMC,081836,A,3751.65,S,14507.36,W,000.0,360.0,130998,011.3,E*70");
        Assert.NotNull(fix);
        Assert.Equal(-37.8608, fix!.Value.LatitudeDeg, precision: 3); // S -> negative
        Assert.Equal(-145.1227, fix.Value.LongitudeDeg, precision: 3); // W -> negative
    }

    [Fact]
    public void Accepts_gnss_talker_prefix()
    {
        var fix = NmeaLocationParser.TryParse("$GNGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*59");
        Assert.NotNull(fix);
    }

    [Fact]
    public void Rejects_gga_with_no_fix()
    {
        Assert.Null(NmeaLocationParser.TryParse("$GPGGA,123519,4807.038,N,01131.000,E,0,00,,,M,,M,,*4E"));
    }

    [Fact]
    public void Rejects_rmc_with_void_status()
    {
        Assert.Null(NmeaLocationParser.TryParse("$GPRMC,123519,V,4807.038,N,01131.000,E,,,230394,,*11"));
    }

    [Fact]
    public void Rejects_bad_checksum()
    {
        Assert.Null(NmeaLocationParser.TryParse("$GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*00"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not an nmea line")]
    [InlineData("$GPGSV,3,1,11,03,03,111,00")] // satellites-in-view, no position
    public void Rejects_non_fix_input(string input)
    {
        Assert.Null(NmeaLocationParser.TryParse(input));
    }
}

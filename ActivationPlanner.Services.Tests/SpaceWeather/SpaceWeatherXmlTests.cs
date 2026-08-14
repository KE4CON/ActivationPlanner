using ActivationPlanner.Services.SpaceWeather;

namespace ActivationPlanner.Services.Tests.SpaceWeather;

public sealed class SpaceWeatherXmlTests
{
    private const string Sample = """
        <?xml version="1.0"?>
        <solar>
          <solardata>
            <source url="http://www.hamqsl.com/solar.html">N0NBH</source>
            <updated>14 Aug 2026 0346 GMT</updated>
            <solarflux>108</solarflux>
            <aindex>7</aindex>
            <kindex>2</kindex>
            <sunspots>101</sunspots>
          </solardata>
        </solar>
        """;

    [Fact]
    public void Parses_all_indices_from_the_hamqsl_feed()
    {
        SolarConditions sw = SpaceWeatherXml.Parse(Sample);

        Assert.Equal(101, sw.SunspotNumber);
        Assert.Equal(108, sw.SolarFluxIndex);
        Assert.Equal(7, sw.AIndex);
        Assert.Equal(2, sw.KIndex);
        Assert.Equal("14 Aug 2026 0346 GMT", sw.UpdatedText);
        Assert.True(sw.HasSunspotNumber);
    }

    [Fact]
    public void Tolerates_whitespace_and_missing_fields()
    {
        const string partial = """
            <solar><solardata>
              <sunspots>  55  </sunspots>
              <solarflux></solarflux>
            </solardata></solar>
            """;

        SolarConditions sw = SpaceWeatherXml.Parse(partial);

        Assert.Equal(55, sw.SunspotNumber);
        Assert.Null(sw.SolarFluxIndex);
        Assert.Null(sw.KIndex);
    }

    [Fact]
    public void Malformed_xml_throws_format_exception()
    {
        Assert.Throws<SpaceWeatherFormatException>(() => SpaceWeatherXml.Parse("<solar><solardata>"));
    }

    [Fact]
    public void Missing_solardata_throws_format_exception()
    {
        Assert.Throws<SpaceWeatherFormatException>(() => SpaceWeatherXml.Parse("<solar></solar>"));
    }
}

using ActivationPlanner.Services.Location;

namespace ActivationPlanner.Services.Tests.Location;

public sealed class GeoIpLocationTests
{
    private const string SampleJson = """
        {"ip":"1.2.3.4","city":"Denver","region":"Colorado","country_name":"United States",
         "country":"US","latitude":39.7392,"longitude":-104.9903,"timezone":"America/Denver"}
        """;

    [Fact]
    public void Parses_coordinates()
    {
        var fix = GeoIpLocation.Parse(SampleJson, "Network (geo-IP)");
        Assert.Equal(39.7392, fix.Location.LatitudeDeg, precision: 4);
        Assert.Equal(-104.9903, fix.Location.LongitudeDeg, precision: 4);
    }

    [Fact]
    public void Builds_place_name_and_marks_approximate()
    {
        var fix = GeoIpLocation.Parse(SampleJson, "Network (geo-IP)");
        Assert.Equal("Denver, Colorado, United States", fix.PlaceName);
        Assert.True(fix.IsApproximate);
        Assert.Equal("Network (geo-IP)", fix.SourceLabel);
    }

    [Fact]
    public void Accepts_coordinates_given_as_strings()
    {
        const string json = """{"latitude":"51.5","longitude":"-0.12","city":"London"}""";
        var fix = GeoIpLocation.Parse(json, "Network (geo-IP)");
        Assert.Equal(51.5, fix.Location.LatitudeDeg, precision: 3);
        Assert.Equal(-0.12, fix.Location.LongitudeDeg, precision: 3);
        Assert.Equal("London", fix.PlaceName);
    }

    [Fact]
    public void Throws_on_error_response()
    {
        const string json = """{"error":true,"reason":"RateLimited"}""";
        var ex = Assert.Throws<LocationUnavailableException>(() => GeoIpLocation.Parse(json, "x"));
        Assert.Contains("RateLimited", ex.Message);
    }

    [Fact]
    public void Throws_when_coordinates_missing()
    {
        const string json = """{"city":"Nowhere"}""";
        Assert.Throws<LocationUnavailableException>(() => GeoIpLocation.Parse(json, "x"));
    }

    [Fact]
    public void Throws_on_invalid_json()
    {
        Assert.Throws<LocationUnavailableException>(() => GeoIpLocation.Parse("not json", "x"));
    }

    [Fact]
    public void Throws_on_out_of_range_coordinates()
    {
        const string json = """{"latitude":200,"longitude":0}""";
        Assert.Throws<LocationUnavailableException>(() => GeoIpLocation.Parse(json, "x"));
    }
}

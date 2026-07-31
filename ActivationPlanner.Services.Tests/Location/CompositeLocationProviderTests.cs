using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.Services.Location;

namespace ActivationPlanner.Services.Tests.Location;

public sealed class CompositeLocationProviderTests
{
    private sealed class StubProvider(LocationFix? fix, string label) : ILocationProvider
    {
        public int Calls { get; private set; }
        public string SourceLabel => label;

        public Task<LocationFix> GetCurrentAsync(CancellationToken ct = default)
        {
            Calls++;
            return fix is not null
                ? Task.FromResult(fix)
                : Task.FromException<LocationFix>(new LocationUnavailableException("unavailable"));
        }
    }

    private static LocationFix Fix(double lat, string source) => new()
    {
        Location = new GeoLocation(lat, 0),
        SourceLabel = source,
        IsApproximate = false,
    };

    [Fact]
    public async Task Uses_preferred_when_it_succeeds()
    {
        var gps = new StubProvider(Fix(40, "GPS"), "GPS");
        var geoip = new StubProvider(Fix(41, "geo-IP"), "geo-IP");
        var composite = new CompositeLocationProvider(gps, geoip);

        var result = await composite.GetCurrentAsync();

        Assert.Equal("GPS", result.SourceLabel);
        Assert.Equal(40, result.Location.LatitudeDeg);
        Assert.Equal(0, geoip.Calls); // fallback never touched
    }

    [Fact]
    public async Task Falls_back_when_preferred_is_unavailable()
    {
        var gps = new StubProvider(null, "GPS");            // throws LocationUnavailable
        var geoip = new StubProvider(Fix(41, "geo-IP"), "geo-IP");
        var composite = new CompositeLocationProvider(gps, geoip);

        var result = await composite.GetCurrentAsync();

        Assert.Equal("geo-IP", result.SourceLabel);
        Assert.Equal(1, gps.Calls);
        Assert.Equal(1, geoip.Calls);
    }
}

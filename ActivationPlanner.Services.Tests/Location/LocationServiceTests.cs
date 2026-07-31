using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.Services.Location;

namespace ActivationPlanner.Services.Tests.Location;

public sealed class LocationServiceTests
{
    private sealed class FakeProvider(LocationFix? fix = null, Exception? throwEx = null) : ILocationProvider
    {
        public int Calls { get; private set; }
        public string SourceLabel => "Fake";

        public Task<LocationFix> GetCurrentAsync(CancellationToken ct = default)
        {
            Calls++;
            if (throwEx is not null)
                return Task.FromException<LocationFix>(throwEx);
            return Task.FromResult(fix ?? new LocationFix
            {
                Location = new GeoLocation(40, -105),
                SourceLabel = "Fake",
                IsApproximate = true,
            });
        }
    }

    [Fact]
    public async Task Refresh_returns_and_remembers_the_fix()
    {
        var service = new LocationService(new FakeProvider());
        Assert.Null(service.Last);

        var fix = await service.RefreshAsync();

        Assert.Equal(40, fix.Location.LatitudeDeg);
        Assert.Same(fix, service.Last);
    }

    [Fact]
    public async Task Refresh_resolves_one_fix_per_call_no_streaming()
    {
        var provider = new FakeProvider();
        var service = new LocationService(provider);

        await service.RefreshAsync();
        await service.RefreshAsync();

        Assert.Equal(2, provider.Calls); // exactly one provider hit per explicit refresh
    }

    [Fact]
    public async Task Refresh_propagates_unavailable_and_leaves_last_untouched()
    {
        var service = new LocationService(new FakeProvider(throwEx: new LocationUnavailableException("offline")));
        await Assert.ThrowsAsync<LocationUnavailableException>(() => service.RefreshAsync());
        Assert.Null(service.Last);
    }
}

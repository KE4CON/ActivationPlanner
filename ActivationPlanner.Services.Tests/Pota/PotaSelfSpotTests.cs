using System.Text.Json;
using ActivationPlanner.Services.Pota;

namespace ActivationPlanner.Services.Tests.Pota;

public sealed class PotaSelfSpotTests
{
    private static PotaSelfSpotRequest Request() => new()
    {
        Activator = "K4ABC",
        Reference = "US-4534",
        FrequencyKhz = 14295.0,
        Mode = "SSB",
        Comments = "QRV now",
    };

    [Fact]
    public void BuildBody_sets_spotter_equal_to_activator()
    {
        using var doc = JsonDocument.Parse(PotaSelfSpotter.BuildBody(Request()));
        var root = doc.RootElement;
        Assert.Equal("K4ABC", root.GetProperty("activator").GetString());
        Assert.Equal("K4ABC", root.GetProperty("spotter").GetString()); // self-spot invariant
    }

    [Fact]
    public void BuildBody_formats_frequency_and_includes_source()
    {
        using var doc = JsonDocument.Parse(PotaSelfSpotter.BuildBody(Request()));
        var root = doc.RootElement;
        Assert.Equal("14295.0", root.GetProperty("frequency").GetString());
        Assert.Equal("SSB", root.GetProperty("mode").GetString());
        Assert.Equal(PotaSelfSpotter.Source, root.GetProperty("source").GetString());
    }

    [Fact]
    public void BuildBody_rejects_missing_fields()
    {
        Assert.Throws<ArgumentException>(() => PotaSelfSpotter.BuildBody(Request() with { Activator = " " }));
        Assert.Throws<ArgumentOutOfRangeException>(() => PotaSelfSpotter.BuildBody(Request() with { FrequencyKhz = 0 }));
    }

    [Fact]
    public async Task SubmitAsync_is_disabled_by_default_and_sends_nothing()
    {
        // A handler that would fail the test if any request were actually sent.
        var spotter = new PotaSelfSpotter(new HttpClient(new ThrowingHandler()));
        Assert.False(spotter.IsEnabled);
        await Assert.ThrowsAsync<PotaSelfSpotDisabledException>(() => spotter.SubmitAsync(Request()));
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) =>
            throw new InvalidOperationException("No HTTP request should be sent while self-spotting is disabled.");
    }
}

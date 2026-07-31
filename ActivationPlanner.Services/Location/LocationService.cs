using ActivationPlanner.PropagationModel.Geo;

namespace ActivationPlanner.Services.Location;

/// <summary>
/// Refresh-on-demand location for the planner (Item #location rule): resolves the operator's
/// current position when asked, and remembers only the most recent fix. No background or
/// continuous tracking — a fix happens only when <see cref="RefreshAsync"/> is called.
/// <para>Layer-3 service: consumes PropagationModel; the actual positioning I/O lives behind
/// <see cref="ILocationProvider"/>. Peer to the gear/mission/planning services.</para>
/// </summary>
public sealed class LocationService
{
    private readonly ILocationProvider _provider;

    public LocationService(ILocationProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    /// <summary>The most recent fix, or null until the first successful refresh.</summary>
    public LocationFix? Last { get; private set; }

    /// <summary>Resolve the current location now and remember it.</summary>
    /// <exception cref="LocationUnavailableException">The fix could not be resolved.</exception>
    public async Task<LocationFix> RefreshAsync(CancellationToken ct = default)
    {
        LocationFix fix = await _provider.GetCurrentAsync(ct).ConfigureAwait(false);
        Last = fix;
        return fix;
    }
}

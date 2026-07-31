using ActivationPlanner.PropagationModel.Geo;

namespace ActivationPlanner.Services.Location;

/// <summary>
/// A source of on-demand location fixes. Abstracted so the location source (network geo-IP,
/// a future platform GPS provider, or a fake in tests) is swappable without touching callers.
/// Implementations resolve a single fix per call — there is no subscription/streaming API,
/// keeping the app's refresh-on-demand-only rule structural.
/// </summary>
public interface ILocationProvider
{
    /// <summary>Short label describing this source, e.g. "Network (geo-IP)".</summary>
    string SourceLabel { get; }

    /// <summary>Resolve the current location once.</summary>
    /// <exception cref="LocationUnavailableException">The fix could not be resolved (offline, blocked, no data).</exception>
    Task<LocationFix> GetCurrentAsync(CancellationToken ct = default);
}

/// <summary>Thrown when a location fix cannot be resolved.</summary>
public sealed class LocationUnavailableException : Exception
{
    public LocationUnavailableException(string message) : base(message) { }
    public LocationUnavailableException(string message, Exception inner) : base(message, inner) { }
}

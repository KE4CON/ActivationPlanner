using ActivationPlanner.PropagationModel.Geo;

namespace ActivationPlanner.Services.Location;

/// <summary>
/// Prefers one location source and falls back to another. Used to make an external hardware GPS the
/// primary source (Item #18) and network geo-IP the fallback: if the GPS is connected and gives a
/// fix, that wins; otherwise geo-IP is used. Precedence is exactly the constructor order.
/// </summary>
public sealed class CompositeLocationProvider : ILocationProvider
{
    private readonly ILocationProvider _preferred;
    private readonly ILocationProvider _fallback;

    public CompositeLocationProvider(ILocationProvider preferred, ILocationProvider fallback)
    {
        _preferred = preferred ?? throw new ArgumentNullException(nameof(preferred));
        _fallback = fallback ?? throw new ArgumentNullException(nameof(fallback));
    }

    /// <inheritdoc />
    public string SourceLabel => $"{_preferred.SourceLabel} → {_fallback.SourceLabel}";

    /// <summary>Try the preferred source; on <see cref="LocationUnavailableException"/> use the fallback.</summary>
    public async Task<LocationFix> GetCurrentAsync(CancellationToken ct = default)
    {
        try
        {
            return await _preferred.GetCurrentAsync(ct).ConfigureAwait(false);
        }
        catch (LocationUnavailableException)
        {
            return await _fallback.GetCurrentAsync(ct).ConfigureAwait(false);
        }
    }
}

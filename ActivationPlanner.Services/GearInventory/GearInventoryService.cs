using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Gear;
using Inventory = ActivationPlanner.PropagationModel.Gear.GearInventory;

namespace ActivationPlanner.Services.GearInventory;

/// <summary>
/// Owns the operator's gear inventory in memory and keeps it persisted through an
/// <see cref="IGearStore"/>. Every mutation writes through to the store so the
/// non-wizard edit screen changes are durable immediately (Phase 1 spec: data is
/// fully editable afterward). The first-run wizard commits its whole working set
/// in one shot via <see cref="ReplaceAsync"/>.
/// <para>
/// Layer-3 service: consumes PropagationModel only. Contains no propagation math
/// and no UI — Option A/B trigger logic arrives in Phase 3.
/// </para>
/// </summary>
public sealed class GearInventoryService
{
    private readonly IGearStore _store;

    // Serializes writes so overlapping async mutations can't interleave file saves
    // or corrupt the in-memory snapshot.
    private readonly SemaphoreSlim _gate = new(1, 1);

    public GearInventoryService(IGearStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        _store = store;
    }

    /// <summary>Current in-memory snapshot. Starts empty until <see cref="LoadAsync"/> runs.</summary>
    public Inventory Current { get; private set; } = Inventory.Empty;

    /// <summary>True when the operator owns nothing — drives the first-run setup wizard.</summary>
    public bool IsFirstRun => Current.IsEmpty;

    /// <summary>Load persisted inventory into <see cref="Current"/>.</summary>
    public async Task LoadAsync(CancellationToken ct = default)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            Current = await _store.LoadAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Replace the entire inventory and persist it. Used by the setup wizard's
    /// Finish step to commit everything at once.
    /// </summary>
    public Task ReplaceAsync(Inventory inventory, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        return MutateAsync(_ => inventory, ct);
    }

    // ---- Non-antenna gear ------------------------------------------------

    public Task AddItemAsync(GearItem item, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        return MutateAsync(
            inv => inv with { Items = [.. inv.Items, item] }, ct);
    }

    public Task UpdateItemAsync(GearItem item, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(item);
        return MutateAsync(
            inv => inv with { Items = Replace(inv.Items, item, x => x.Id == item.Id) }, ct);
    }

    public Task RemoveItemAsync(Guid id, CancellationToken ct = default) =>
        MutateAsync(
            inv => inv with { Items = inv.Items.Where(x => x.Id != id).ToList() }, ct);

    // ---- Antennas --------------------------------------------------------

    public Task AddAntennaAsync(AntennaProfile antenna, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(antenna);
        return MutateAsync(
            inv => inv with { Antennas = [.. inv.Antennas, antenna] }, ct);
    }

    public Task UpdateAntennaAsync(AntennaProfile antenna, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(antenna);
        return MutateAsync(
            inv => inv with { Antennas = Replace(inv.Antennas, antenna, x => x.Id == antenna.Id) }, ct);
    }

    public Task RemoveAntennaAsync(Guid id, CancellationToken ct = default) =>
        MutateAsync(
            inv => inv with { Antennas = inv.Antennas.Where(x => x.Id != id).ToList() }, ct);

    // ---- Internals -------------------------------------------------------

    private async Task MutateAsync(Func<Inventory, Inventory> mutate, CancellationToken ct)
    {
        await _gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var next = mutate(Current);
            await _store.SaveAsync(next, ct).ConfigureAwait(false);
            Current = next; // only commit in-memory state after a successful save
        }
        finally
        {
            _gate.Release();
        }
    }

    private static List<T> Replace<T>(IReadOnlyList<T> source, T replacement, Func<T, bool> match)
    {
        var result = new List<T>(source.Count);
        var replaced = false;
        foreach (var existing in source)
        {
            if (!replaced && match(existing))
            {
                result.Add(replacement);
                replaced = true;
            }
            else
            {
                result.Add(existing);
            }
        }

        if (!replaced)
            throw new InvalidOperationException("No matching entry to update; add it instead.");

        return result;
    }
}

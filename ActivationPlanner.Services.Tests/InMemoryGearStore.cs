using System.Threading;
using System.Threading.Tasks;
using ActivationPlanner.Services.GearInventory;
using Inventory = ActivationPlanner.PropagationModel.Gear.GearInventory;

namespace ActivationPlanner.Services.Tests;

/// <summary>
/// Test double for <see cref="IGearStore"/> — keeps the inventory in memory and
/// counts saves, so service tests can assert persistence happened without touching
/// the filesystem.
/// </summary>
internal sealed class InMemoryGearStore : IGearStore
{
    public Inventory Saved { get; private set; } = Inventory.Empty;
    public int SaveCount { get; private set; }

    public InMemoryGearStore() { }

    public InMemoryGearStore(Inventory seed) => Saved = seed;

    public Task<Inventory> LoadAsync(CancellationToken ct = default) => Task.FromResult(Saved);

    public Task SaveAsync(Inventory inventory, CancellationToken ct = default)
    {
        Saved = inventory;
        SaveCount++;
        return Task.CompletedTask;
    }
}

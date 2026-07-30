using System.Threading;
using System.Threading.Tasks;
using ActivationPlanner.PropagationModel.Gear;

namespace ActivationPlanner.Services.GearInventory;

/// <summary>
/// Persistence boundary for the gear inventory. Abstracted so the inventory
/// service can be unit-tested against an in-memory implementation without
/// touching the filesystem.
/// </summary>
public interface IGearStore
{
    /// <summary>
    /// Load the persisted inventory. Returns <see cref="PropagationModel.Gear.GearInventory.Empty"/>
    /// when nothing has been saved yet (first run).
    /// </summary>
    Task<PropagationModel.Gear.GearInventory> LoadAsync(CancellationToken ct = default);

    /// <summary>Persist the full inventory, replacing any previously saved state.</summary>
    Task SaveAsync(PropagationModel.Gear.GearInventory inventory, CancellationToken ct = default);
}

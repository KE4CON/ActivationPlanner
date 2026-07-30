using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace ActivationPlanner.Services.GearInventory;

/// <summary>
/// <see cref="IGearStore"/> backed by a single JSON file. Uses framework
/// <c>System.Text.Json</c> (no NuGet dependency). Enums are written as strings so
/// the file stays human-readable and stable if enum order changes.
/// </summary>
public sealed class JsonGearStore : IGearStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _filePath;

    /// <param name="filePath">
    /// Full path to the JSON file. Use <see cref="DefaultFilePath"/> for the
    /// standard per-user location.
    /// </param>
    public JsonGearStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path must be provided.", nameof(filePath));
        _filePath = filePath;
    }

    /// <summary>Convenience factory using the standard per-user application-data location.</summary>
    public static JsonGearStore CreateDefault() => new(DefaultFilePath());

    /// <summary>
    /// Cross-platform per-user path:
    /// <c>&lt;ApplicationData&gt;/ActivationPlanner/gear-inventory.json</c>.
    /// </summary>
    public static string DefaultFilePath()
    {
        var baseDir = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData,
            Environment.SpecialFolderOption.Create);
        return Path.Combine(baseDir, "ActivationPlanner", "gear-inventory.json");
    }

    public async Task<PropagationModel.Gear.GearInventory> LoadAsync(CancellationToken ct = default)
    {
        if (!File.Exists(_filePath))
            return PropagationModel.Gear.GearInventory.Empty;

        await using var stream = File.OpenRead(_filePath);
        var inventory = await JsonSerializer.DeserializeAsync<PropagationModel.Gear.GearInventory>(
            stream, SerializerOptions, ct).ConfigureAwait(false);
        return inventory ?? PropagationModel.Gear.GearInventory.Empty;
    }

    public async Task SaveAsync(PropagationModel.Gear.GearInventory inventory, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(inventory);

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        // Write to a temp file then move, so a crash mid-write can't corrupt the
        // existing inventory.
        var tempPath = _filePath + ".tmp";
        await using (var stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, inventory, SerializerOptions, ct)
                .ConfigureAwait(false);
        }

        File.Move(tempPath, _filePath, overwrite: true);
    }
}

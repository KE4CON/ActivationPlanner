using System.Text.Json;
using System.Text.Json.Serialization;
using ActivationPlanner.PropagationModel.Gear;

namespace ActivationPlanner.Services.Presets;

/// <summary>
/// How much to trust a preset's modeling geometry.
/// </summary>
public enum ModelingConfidence
{
    /// <summary>Real wire dimensions — the NEC model is accurate (dipoles, EFHW, the NVIS crossed dipole).</summary>
    Measured,

    /// <summary>
    /// Nominal dimensions only — a loaded/modular antenna (e.g. a coil-loaded vertical) whose true
    /// electrical length is not published. The pattern is representative, not exact; the UI says so.
    /// </summary>
    Approximate,
}

/// <summary>
/// A real, off-the-shelf antenna the operator can pick in setup instead of typing dimensions. It
/// prefills the (still editable) antenna form. Simple-wire antennas carry accurate geometry; loaded
/// designs carry nominal dimensions flagged <see cref="ModelingConfidence.Approximate"/>.
/// </summary>
public sealed record AntennaPreset(
    string Id,
    string Manufacturer,
    string Model,
    AntennaCategory Category,
    FeedPointType FeedPoint,
    double LengthFeet,
    double HeightFeet,
    int? RadialCount,
    double? RadialLengthFeet,
    ModelingConfidence ModelingConfidence,
    string? Note,
    string? Source,
    double? RadialHeightFeet = null)
{
    /// <summary>Manufacturer + model, for the picker.</summary>
    public string DisplayName => $"{Manufacturer} {Model}";
}

/// <summary>A real radio the operator can pick in setup. Radios need no modeling, only description.</summary>
public sealed record RadioPreset(
    string Id,
    string Manufacturer,
    string Model,
    string Bands,
    double PowerWatts,
    string? Note)
{
    public string DisplayName => $"{Manufacturer} {Model}";
}

/// <summary>
/// A real, description-only piece of gear (battery, digital interface, etc.) tagged with the gear
/// <see cref="Category"/> it belongs to, so the gear form can offer it under the right category.
/// </summary>
public sealed record GearItemPreset(
    string Id,
    GearCategory Category,
    string Manufacturer,
    string Model,
    string? Note)
{
    public string DisplayName => $"{Manufacturer} {Model}";
}

/// <summary>The full bundled preset catalog, one list per gear category covered so far.</summary>
public sealed record GearPresetCatalog(
    IReadOnlyList<AntennaPreset> Antennas,
    IReadOnlyList<RadioPreset> Radios,
    IReadOnlyList<GearItemPreset> Gear)
{
    public static GearPresetCatalog Empty { get; } = new([], [], []);
}

/// <summary>
/// Loads the bundled manufacturer preset catalog (a data file, not hardcoded — updatable without a
/// recompile). Reference data, so exposed as a lazily-loaded <see cref="Default"/> singleton;
/// <see cref="Load"/> is provided for tests.
/// </summary>
public static class PresetCatalog
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = { new JsonStringEnumConverter() },
    };

    private static readonly Lazy<GearPresetCatalog> LazyDefault = new(LoadEmbedded);

    /// <summary>The bundled catalog, parsed once on first use.</summary>
    public static GearPresetCatalog Default => LazyDefault.Value;

    /// <summary>Parse a catalog from a JSON stream (used by tests and by <see cref="Default"/>).</summary>
    public static GearPresetCatalog Load(Stream json)
    {
        ArgumentNullException.ThrowIfNull(json);
        return JsonSerializer.Deserialize<GearPresetCatalog>(json, Options) ?? GearPresetCatalog.Empty;
    }

    private static GearPresetCatalog LoadEmbedded()
    {
        var assembly = typeof(PresetCatalog).Assembly;
        string? resource = Array.Find(
            assembly.GetManifestResourceNames(),
            n => n.EndsWith("gear-presets.json", StringComparison.OrdinalIgnoreCase));
        if (resource is null)
            return GearPresetCatalog.Empty;

        using Stream? stream = assembly.GetManifestResourceStream(resource);
        return stream is null ? GearPresetCatalog.Empty : Load(stream);
    }
}

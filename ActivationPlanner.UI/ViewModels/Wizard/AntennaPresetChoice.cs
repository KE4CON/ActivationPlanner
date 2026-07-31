using ActivationPlanner.Services.Presets;

namespace ActivationPlanner.UI.ViewModels.Wizard;

/// <summary>
/// One entry in the antenna "start from a model" picker: either a real manufacturer
/// <see cref="AntennaPreset"/>, or the always-present <see cref="Custom"/> escape hatch for a
/// home-brew antenna or a model not in the catalog.
/// </summary>
public sealed record AntennaPresetChoice(string Label, AntennaPreset? Preset)
{
    /// <summary>The "type it in myself" option — no preset, prefills nothing.</summary>
    public static AntennaPresetChoice Custom { get; } = new("Custom / Home-brew…", null);
}

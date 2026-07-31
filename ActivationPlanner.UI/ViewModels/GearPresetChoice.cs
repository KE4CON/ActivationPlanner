namespace ActivationPlanner.UI.ViewModels;

/// <summary>
/// One entry in the gear form's "start from a model" picker. Category-agnostic: a preset just
/// prefills the generic gear form's name and notes, so the same type serves radios today and
/// tuners / power / interfaces as those catalog sections get filled in. <see cref="Custom"/> is the
/// always-present escape hatch (prefills nothing).
/// </summary>
public sealed record GearPresetChoice(string Label, string? PrefillName, string? PrefillNotes)
{
    /// <summary>The "type it in myself" option.</summary>
    public static GearPresetChoice Custom { get; } = new("Custom / Home-brew…", null, null);
}

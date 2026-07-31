using ActivationPlanner.PropagationModel.Checklists;
using ActivationPlanner.Services.Planning;

namespace ActivationPlanner.Services.Export;

/// <summary>
/// What to include in an exported plan PDF. The three content sections are independently
/// selectable (bands / antennas / checklist, any combination) per the export feature.
/// </summary>
public sealed record PdfExportRequest
{
    /// <summary>Document title.</summary>
    public string Title { get; init; } = "Activation Plan";

    /// <summary>Optional subtitle line (e.g. location + generated time).</summary>
    public string? Subtitle { get; init; }

    /// <summary>True to flag that the predictions are sample data (VOACAP not configured).</summary>
    public bool IsSampleData { get; init; }

    /// <summary>Include the ranked band recommendations table.</summary>
    public bool IncludeBands { get; init; } = true;

    /// <summary>Include the per-band antenna recommendations.</summary>
    public bool IncludeAntennas { get; init; } = true;

    /// <summary>Include the packing checklist.</summary>
    public bool IncludeChecklist { get; init; } = true;

    /// <summary>Ranked band recommendations (best first).</summary>
    public IReadOnlyList<BandRecommendation> Bands { get; init; } = [];

    /// <summary>Packing checklist, if a mission checklist was built.</summary>
    public ChecklistInstance? Checklist { get; init; }

    /// <summary>True when at least one selected section has content to render.</summary>
    public bool HasContent =>
        (IncludeBands && Bands.Count > 0)
        || (IncludeAntennas && Bands.Any(b => b.OwnedAntennas.Count > 0))
        || (IncludeChecklist && Checklist is not null);
}

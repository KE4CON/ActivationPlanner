using ActivationPlanner.PropagationModel.Gear;

namespace ActivationPlanner.Services.Antennas;

/// <summary>
/// The community-library antenna catalog: the models available for Option A matching, each
/// with the physical assumptions it bakes in. The default catalog is a curated starting set
/// of common portable/home HF antennas; the actual pattern files live in the user's VOACAP
/// install. A custom catalog can be supplied (e.g. once the user's installed library is
/// enumerated).
/// <para>
/// Category name alone is not enough to pick a model — feed point distinguishes antennas
/// that share a category — so lookups match on both, preferring an exact feed match.
/// </para>
/// </summary>
public sealed class AntennaLibrary
{
    private readonly IReadOnlyList<AntennaLibraryEntry> _entries;

    public AntennaLibrary(IReadOnlyList<AntennaLibraryEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        _entries = entries;
    }

    /// <summary>All catalog entries.</summary>
    public IReadOnlyList<AntennaLibraryEntry> Entries => _entries;

    /// <summary>
    /// Candidate models for a category + feed point. Entries whose feed point matches exactly
    /// are preferred; if none match the feed, the category entries are returned so a close
    /// model can still be considered.
    /// </summary>
    public IReadOnlyList<AntennaLibraryEntry> Candidates(AntennaCategory category, FeedPointType feedPoint)
    {
        var byCategory = _entries.Where(e => e.Category == category).ToList();
        var feedMatches = byCategory.Where(e => e.FeedPoint == feedPoint).ToList();
        return feedMatches.Count > 0 ? feedMatches : byCategory;
    }

    /// <summary>The candidate whose assumed height is closest to <paramref name="heightWavelengths"/>, or null if none.</summary>
    public AntennaLibraryEntry? NearestByHeight(AntennaCategory category, FeedPointType feedPoint, double heightWavelengths) =>
        Candidates(category, feedPoint)
            .OrderBy(e => Math.Abs(e.AssumedHeightWavelengths - heightWavelengths))
            .FirstOrDefault();

    /// <summary>The candidate whose expected length is closest to <paramref name="lengthWavelengths"/>, or null if none.</summary>
    public AntennaLibraryEntry? NearestByLength(AntennaCategory category, FeedPointType feedPoint, double lengthWavelengths) =>
        Candidates(category, feedPoint)
            .OrderBy(e => Math.Abs(e.ExpectedLengthWavelengths - lengthWavelengths))
            .FirstOrDefault();

    /// <summary>
    /// The default curated catalog. Heights and lengths are wavelength-relative so they apply
    /// across bands. Magnetic loops, random wires, and "other" designs are intentionally absent —
    /// they have no trustworthy generic library model and route to Option B (custom modeling).
    /// </summary>
    public static AntennaLibrary Default { get; } = new(
    [
        // Center-fed half-wave dipoles at a few standard heights (nearest-height selection).
        new("Half-wave dipole @ 0.25λ", AntennaCategory.Dipole, FeedPointType.CenterFed, "default/dipole", 0.25, 0.5),
        new("Half-wave dipole @ 0.5λ", AntennaCategory.Dipole, FeedPointType.CenterFed, "default/dipole", 0.5, 0.5),
        new("Half-wave dipole @ 1.0λ", AntennaCategory.Dipole, FeedPointType.CenterFed, "default/dipole", 1.0, 0.5),

        // Off-center-fed dipole.
        new("Off-center-fed dipole @ 0.5λ", AntennaCategory.Dipole, FeedPointType.OffCenterFed, "default/ocfd", 0.5, 0.5),

        // End-fed half-wave (portable, low).
        new("End-fed half-wave @ 0.25λ", AntennaCategory.EndFedHalfWave, FeedPointType.EndFedHalfWave, "default/efhw", 0.25, 0.5),

        // Ground-mounted verticals (base height ~0).
        new("Quarter-wave vertical (ground)", AntennaCategory.Vertical, FeedPointType.BaseFed, "default/vert_quarter", 0.0, 0.25),
        new("Half-wave vertical (ground)", AntennaCategory.Vertical, FeedPointType.BaseFed, "default/vert_half", 0.0, 0.5),

        // Short whip (mobile/portable), treated as a short vertical.
        new("Short whip (ground)", AntennaCategory.Whip, FeedPointType.BaseFed, "default/whip", 0.0, 0.1),
    ]);
}

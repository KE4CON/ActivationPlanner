using ActivationPlanner.PropagationModel.Antennas;
using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Gear;
using static ActivationPlanner.Services.Antennas.AntennaModelingThresholds;

namespace ActivationPlanner.Services.Antennas;

/// <summary>
/// Decides, per antenna and per band, whether a community-library pattern is trustworthy
/// (Option A) or the antenna needs custom NEC2++ modeling (Option B). This is the antenna
/// half of the GearInventoryService remit: category-mapping plus the Option A/B trigger
/// logic, all wavelength-relative and evaluated per band.
/// <para>
/// The decision is intentionally per (antenna, band): the same physical antenna can need a
/// library model on one band and custom modeling on another, since electrical height and
/// length are per-band questions. Whether to later collapse this to one decision per antenna
/// for a whole session is deferred pending the empirical dipole study (CLAUDE.md).
/// </para>
/// <para>Layer-3 service: pure decision logic over PropagationModel types. No VOACAP/NEC2++
/// shell-out here (that is ProcessEngine, Phase 8) and no UI.</para>
/// </summary>
public sealed class AntennaModelingEvaluator
{
    private readonly AntennaLibrary _library;

    public AntennaModelingEvaluator(AntennaLibrary? library = null)
    {
        _library = library ?? AntennaLibrary.Default;
    }

    /// <summary>Evaluate one antenna at one frequency (MHz).</summary>
    public AntennaModelingDecision Evaluate(AntennaProfile antenna, double frequencyMhz)
    {
        ArgumentNullException.ThrowIfNull(antenna);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frequencyMhz);

        double heightWl = Wavelength.InWavelengths(antenna.HeightFeet, frequencyMhz);
        double lengthWl = Wavelength.InWavelengths(antenna.LengthFeet, frequencyMhz);

        return antenna.Category switch
        {
            AntennaCategory.Vertical or AntennaCategory.Whip
                => EvaluateVertical(antenna, frequencyMhz, heightWl, lengthWl),
            AntennaCategory.Dipole or AntennaCategory.EndFedHalfWave
                => EvaluateHorizontalLike(antenna, frequencyMhz, heightWl, lengthWl),
            AntennaCategory.NvisCrossedDipole
                => CustomModeling(antenna, frequencyMhz, heightWl, lengthWl,
                    "An NVIS crossed dipole has no community-library equivalent; custom NEC modeling required."),
            _ // MagneticLoop, Other — no trustworthy generic library model
                => CustomModeling(antenna, frequencyMhz, heightWl, lengthWl,
                    $"No community-library model for a {antenna.Category} antenna; custom modeling required."),
        };
    }

    /// <summary>Evaluate one antenna across a set of bands.</summary>
    public IReadOnlyList<AntennaModelingDecision> EvaluateBands(AntennaProfile antenna, IEnumerable<HamBand> bands)
    {
        ArgumentNullException.ThrowIfNull(antenna);
        ArgumentNullException.ThrowIfNull(bands);
        return bands
            .Select(b => Evaluate(antenna, HamBands.RepresentativeFrequencyMhz(b)))
            .ToList();
    }

    // ---- verticals / whips ----

    private AntennaModelingDecision EvaluateVertical(
        AntennaProfile antenna, double freq, double heightWl, double lengthWl)
    {
        // Documented distortion zone: an elevated vertical whose BASE height sits within
        // 0.25λ–1.25λ is modeled poorly by library patterns — always custom, regardless of a
        // length match.
        if (heightWl is >= VerticalDistortionLowWavelengths and <= VerticalDistortionHighWavelengths)
            return CustomModeling(antenna, freq, heightWl, lengthWl,
                $"Vertical base height {heightWl:0.00}λ is inside the {VerticalDistortionLowWavelengths:0.##}λ–" +
                $"{VerticalDistortionHighWavelengths:0.##}λ distortion zone; custom modeling required.");

        // Otherwise select the library vertical by radiator length and apply the length test.
        AntennaLibraryEntry? entry = _library.NearestByLength(antenna.Category, antenna.FeedPoint, lengthWl);
        if (entry is null)
            return CustomModeling(antenna, freq, heightWl, lengthWl,
                "No community-library vertical model for this configuration; custom modeling required.");

        double lengthDelta = Math.Abs(lengthWl - entry.ExpectedLengthWavelengths);
        if (lengthDelta > LengthDeltaWavelengths)
            return CustomModeling(antenna, freq, heightWl, lengthWl,
                $"Radiator length {lengthWl:0.00}λ differs from the '{entry.DisplayName}' model " +
                $"({entry.ExpectedLengthWavelengths:0.00}λ) by {lengthDelta:0.00}λ (> {LengthDeltaWavelengths:0.##}λ); custom modeling required.");

        return LibraryMatch(antenna, freq, heightWl, lengthWl, entry, heightDelta: null,
            $"Vertical matches the '{entry.DisplayName}' library model within tolerance.");
    }

    // ---- dipoles / end-fed half-wave (horizontal-like) ----

    private AntennaModelingDecision EvaluateHorizontalLike(
        AntennaProfile antenna, double freq, double heightWl, double lengthWl)
    {
        AntennaLibraryEntry? entry = _library.NearestByHeight(antenna.Category, antenna.FeedPoint, heightWl);
        if (entry is null)
            return CustomModeling(antenna, freq, heightWl, lengthWl,
                $"No community-library {antenna.Category} model for this feed point; custom modeling required.");

        // Length test first — an EFHW's electrical length varies by design band, so a wire cut
        // for a different band is a different antenna even under the same category name.
        double lengthDelta = Math.Abs(lengthWl - entry.ExpectedLengthWavelengths);
        if (lengthDelta > LengthDeltaWavelengths)
            return CustomModeling(antenna, freq, heightWl, lengthWl,
                $"Electrical length {lengthWl:0.00}λ differs from the '{entry.DisplayName}' model " +
                $"({entry.ExpectedLengthWavelengths:0.00}λ) by {lengthDelta:0.00}λ (> {LengthDeltaWavelengths:0.##}λ); custom modeling required.");

        // Height test against the library model's assumed height (provisional 0.05λ threshold).
        double heightDelta = Math.Abs(heightWl - entry.AssumedHeightWavelengths);
        if (heightDelta > DipoleHeightDeltaWavelengths)
            return CustomModeling(antenna, freq, heightWl, lengthWl,
                $"Height {heightWl:0.00}λ differs from the '{entry.DisplayName}' model's assumed " +
                $"{entry.AssumedHeightWavelengths:0.00}λ by {heightDelta:0.00}λ (> {DipoleHeightDeltaWavelengths:0.##}λ); custom modeling required.",
                heightDelta);

        return LibraryMatch(antenna, freq, heightWl, lengthWl, entry, heightDelta,
            $"Matches the '{entry.DisplayName}' library model within tolerance.");
    }

    // ---- decision builders ----

    private static AntennaModelingDecision LibraryMatch(
        AntennaProfile antenna, double freq, double heightWl, double lengthWl,
        AntennaLibraryEntry entry, double? heightDelta, string reason) => new()
        {
            AntennaId = antenna.Id,
            AntennaName = antenna.Name,
            FrequencyMhz = freq,
            Option = AntennaModelingOption.LibraryMatch,
            Reason = reason,
            HeightWavelengths = heightWl,
            LengthWavelengths = lengthWl,
            LibraryMatch = entry,
            HeightDeltaWavelengths = heightDelta,
        };

    private static AntennaModelingDecision CustomModeling(
        AntennaProfile antenna, double freq, double heightWl, double lengthWl,
        string reason, double? heightDelta = null) => new()
        {
            AntennaId = antenna.Id,
            AntennaName = antenna.Name,
            FrequencyMhz = freq,
            Option = AntennaModelingOption.CustomModeling,
            Reason = reason,
            HeightWavelengths = heightWl,
            LengthWavelengths = lengthWl,
            LibraryMatch = null,
            HeightDeltaWavelengths = heightDelta,
        };
}

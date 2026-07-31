namespace ActivationPlanner.Services.Antennas;

/// <summary>
/// Which modeling path an antenna takes at a given band.
/// </summary>
public enum AntennaModelingOption
{
    /// <summary>
    /// Option A — a community-library antenna pattern is a good enough match; use it directly
    /// with VOACAP. No custom modeling needed.
    /// </summary>
    LibraryMatch,

    /// <summary>
    /// Option B — the antenna falls outside where a library model is trustworthy at this band,
    /// so it needs custom NEC2++ modeling (Phase 8) to produce an accurate pattern.
    /// </summary>
    CustomModeling,
}

/// <summary>
/// Provisional thresholds for the Option A/B trigger logic (CLAUDE.md "Key Domain Rules").
/// <para>
/// The vertical distortion zone is documented; the dipole/length deltas are marked
/// <b>provisional</b> and are to be validated empirically against real VOACAP output using
/// the Phase 2 shell-out (the dipole comparison harness) and tuned before being treated as
/// final. They live here as named constants so that tuning is a one-line change.
/// </para>
/// </summary>
public static class AntennaModelingThresholds
{
    /// <summary>Lower bound of the vertical base-height distortion zone, in wavelengths.</summary>
    public const double VerticalDistortionLowWavelengths = 0.25;

    /// <summary>Upper bound of the vertical base-height distortion zone, in wavelengths.</summary>
    public const double VerticalDistortionHighWavelengths = 1.25;

    /// <summary>
    /// Max height difference from a dipole library model's assumed height before Option B, in
    /// wavelengths. <b>Provisional</b> — validate and tune against real VOACAP output.
    /// </summary>
    public const double DipoleHeightDeltaWavelengths = 0.05;

    /// <summary>
    /// Max electrical-length difference from a library model before Option B, in wavelengths.
    /// The same wavelength-relative treatment as height (an EFHW's electrical length varies by
    /// design band). <b>Provisional</b> — validate and tune against real VOACAP output.
    /// </summary>
    public const double LengthDeltaWavelengths = 0.05;
}

/// <summary>
/// A community-library antenna model available for Option A: a VOACAP pattern file plus the
/// physical assumptions baked into it (the height it models and the radiator's electrical
/// length), both wavelength-relative so they apply per band.
/// </summary>
/// <param name="DisplayName">Human-readable model name, e.g. "Half-wave dipole @ 0.5λ".</param>
/// <param name="Category">Antenna family this model represents.</param>
/// <param name="FeedPoint">Feed arrangement this model assumes (distinguishes same-category antennas).</param>
/// <param name="VoacapFile">
/// Pattern file relative to the VOACAP <c>antennas</c> directory in the user's install. The
/// catalog names community-library files; the user's install provides the actual patterns.
/// </param>
/// <param name="AssumedHeightWavelengths">Height above ground the model assumes, in wavelengths.</param>
/// <param name="ExpectedLengthWavelengths">Radiator electrical length the model assumes, in wavelengths.</param>
public sealed record AntennaLibraryEntry(
    string DisplayName,
    PropagationModel.Gear.AntennaCategory Category,
    PropagationModel.Gear.FeedPointType FeedPoint,
    string VoacapFile,
    double AssumedHeightWavelengths,
    double ExpectedLengthWavelengths);

/// <summary>
/// The result of evaluating one antenna at one band: whether a library model suffices
/// (Option A) or custom modeling is required (Option B), with the wavelength-relative
/// figures and the human-readable reason behind the decision.
/// </summary>
public sealed record AntennaModelingDecision
{
    /// <summary>Identity of the evaluated antenna.</summary>
    public required Guid AntennaId { get; init; }

    /// <summary>Name of the evaluated antenna.</summary>
    public required string AntennaName { get; init; }

    /// <summary>Frequency the decision was made at, MHz.</summary>
    public required double FrequencyMhz { get; init; }

    /// <summary>Chosen modeling path.</summary>
    public required AntennaModelingOption Option { get; init; }

    /// <summary>Plain-language explanation of why this option was chosen.</summary>
    public required string Reason { get; init; }

    /// <summary>Antenna base height expressed in wavelengths at <see cref="FrequencyMhz"/>.</summary>
    public required double HeightWavelengths { get; init; }

    /// <summary>Antenna radiator length expressed in wavelengths at <see cref="FrequencyMhz"/>.</summary>
    public required double LengthWavelengths { get; init; }

    /// <summary>The library model considered, if any (present when <see cref="Option"/> is LibraryMatch).</summary>
    public AntennaLibraryEntry? LibraryMatch { get; init; }

    /// <summary>Height difference from the considered model's assumed height, in wavelengths (if applicable).</summary>
    public double? HeightDeltaWavelengths { get; init; }

    /// <summary>Convenience: true when custom NEC2++ modeling is required.</summary>
    public bool RequiresCustomModeling => Option == AntennaModelingOption.CustomModeling;
}

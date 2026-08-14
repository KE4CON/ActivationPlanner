using System.Collections.Generic;

namespace ActivationPlanner.PropagationModel.Bands;

/// <summary>One sub-segment of a band: a frequency range, the modes allowed, and who may use it.</summary>
public sealed record BandPlanSegment(string Range, string Modes, string Licenses);

/// <summary>
/// A band in the reference: name, full range, a plain-language one-liner, and its sub-segments.
/// </summary>
public sealed record BandPlanBand(string Name, string Range, string Summary, IReadOnlyList<BandPlanSegment> Segments);

/// <summary>
/// A plain-language US amateur band-privileges reference, built from the FCC Part 97 allocations
/// (US-government facts — not the ARRL's copyrighted band-plan chart). A quick "where can I operate,
/// and what may I run there" guide; it summarizes and is not a substitute for the current FCC rules.
/// Populated in <see cref="UsBandPlanData"/> so the data is easy to review in one place.
/// </summary>
public static class UsBandPlan
{
    /// <summary>All bands in the reference, lowest frequency first.</summary>
    public static IReadOnlyList<BandPlanBand> Bands => UsBandPlanData.Bands;
}

namespace ActivationPlanner.PropagationModel.Bands;

/// <summary>The amateur HF bands the planner reasons about.</summary>
public enum HamBand
{
    M160,
    M80,
    M60,
    M40,
    M30,
    M20,
    M17,
    M15,
    M12,
    M10,
}

/// <summary>
/// Static catalog mapping each <see cref="HamBand"/> to a representative frequency and a
/// display name. VOACAP predicts at a discrete frequency, so each band is queried at a
/// representative point roughly central to its common HF activity.
/// </summary>
public static class HamBands
{
    private static readonly IReadOnlyDictionary<HamBand, (string Name, double FreqMhz)> Table =
        new Dictionary<HamBand, (string, double)>
        {
            [HamBand.M160] = ("160m", 1.900),
            [HamBand.M80] = ("80m", 3.600),
            [HamBand.M60] = ("60m", 5.350),
            [HamBand.M40] = ("40m", 7.100),
            [HamBand.M30] = ("30m", 10.125),
            [HamBand.M20] = ("20m", 14.100),
            [HamBand.M17] = ("17m", 18.100),
            [HamBand.M15] = ("15m", 21.100),
            [HamBand.M12] = ("12m", 24.940),
            [HamBand.M10] = ("10m", 28.300),
        };

    /// <summary>All HF bands, lowest frequency first.</summary>
    public static IReadOnlyList<HamBand> All { get; } =
        [HamBand.M160, HamBand.M80, HamBand.M60, HamBand.M40, HamBand.M30,
         HamBand.M20, HamBand.M17, HamBand.M15, HamBand.M12, HamBand.M10];

    /// <summary>Representative frequency for the band, in MHz — the point VOACAP is queried at.</summary>
    public static double RepresentativeFrequencyMhz(HamBand band) => Table[band].FreqMhz;

    /// <summary>Human-readable band name, e.g. "40m".</summary>
    public static string DisplayName(HamBand band) => Table[band].Name;

    /// <summary>
    /// The band whose representative frequency is closest to <paramref name="frequencyMhz"/>.
    /// Used to align VOACAP's rounded output frequencies back to requested bands.
    /// </summary>
    public static HamBand Nearest(double frequencyMhz)
    {
        HamBand best = HamBand.M20;
        double bestDelta = double.MaxValue;
        foreach (HamBand band in All)
        {
            double delta = Math.Abs(RepresentativeFrequencyMhz(band) - frequencyMhz);
            if (delta < bestDelta)
            {
                bestDelta = delta;
                best = band;
            }
        }
        return best;
    }

    // Amateur HF band edges in MHz (region-agnostic outer bounds), for mapping an actual
    // operating frequency (e.g. a POTA spot) to the band it falls in.
    private static readonly IReadOnlyDictionary<HamBand, (double Low, double High)> Edges =
        new Dictionary<HamBand, (double, double)>
        {
            [HamBand.M160] = (1.80, 2.00),
            [HamBand.M80] = (3.50, 4.00),
            [HamBand.M60] = (5.25, 5.45),
            [HamBand.M40] = (7.00, 7.30),
            [HamBand.M30] = (10.10, 10.15),
            [HamBand.M20] = (14.00, 14.35),
            [HamBand.M17] = (18.06, 18.17),
            [HamBand.M15] = (21.00, 21.45),
            [HamBand.M12] = (24.89, 24.99),
            [HamBand.M10] = (28.00, 29.70),
        };

    /// <summary>
    /// The HF band that <paramref name="frequencyMhz"/> falls within, or null if it is outside
    /// the amateur HF bands (e.g. a VHF/UHF frequency). Unlike <see cref="Nearest"/>, this uses
    /// real band edges, so an out-of-band frequency is reported as no band rather than snapped.
    /// </summary>
    public static HamBand? BandForFrequencyMhz(double frequencyMhz)
    {
        foreach (var (band, edges) in Edges)
        {
            if (frequencyMhz >= edges.Low && frequencyMhz <= edges.High)
                return band;
        }
        return null;
    }
}

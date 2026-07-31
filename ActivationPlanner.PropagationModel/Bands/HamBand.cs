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
}

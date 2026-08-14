using System.Collections.Generic;
using System.Globalization;

namespace ActivationPlanner.PropagationModel.Bands;

/// <summary>
/// Common calling / activity frequencies (MHz) per band — the SSB, CW, and FT8 spots portable
/// operators (POTA / SOTA / QRP) typically call on. Reference data to make a band recommendation
/// immediately actionable ("go here"); always defer to the current band plan and your license.
/// </summary>
public static class CallingFrequencies
{
    private static readonly IReadOnlyDictionary<HamBand, (double? Ssb, double? Cw, double? Ft8)> Table =
        new Dictionary<HamBand, (double?, double?, double?)>
        {
            [HamBand.M160] = (1.910, 1.810, 1.840),
            [HamBand.M80] = (3.985, 3.560, 3.573),
            [HamBand.M60] = (null, null, 5.357),   // 60m is channelized; FT8 on channel 1
            [HamBand.M40] = (7.185, 7.030, 7.074),
            [HamBand.M30] = (null, 10.116, 10.136), // 30m is CW/data only — no phone
            [HamBand.M20] = (14.285, 14.060, 14.074),
            [HamBand.M17] = (18.155, 18.086, 18.100),
            [HamBand.M15] = (21.285, 21.060, 21.074),
            [HamBand.M12] = (24.955, 24.906, 24.915),
            [HamBand.M10] = (28.400, 28.060, 28.074),
        };

    /// <summary>A one-line summary like "SSB 14.285 · CW 14.060 · FT8 14.074" (omits modes N/A on the band).</summary>
    public static string Summary(HamBand band)
    {
        if (!Table.TryGetValue(band, out var f))
            return string.Empty;

        var parts = new List<string>(3);
        if (f.Ssb is { } ssb) parts.Add($"SSB {ssb.ToString("0.000", CultureInfo.InvariantCulture)}");
        if (f.Cw is { } cw) parts.Add($"CW {cw.ToString("0.000", CultureInfo.InvariantCulture)}");
        if (f.Ft8 is { } ft8) parts.Add($"FT8 {ft8.ToString("0.000", CultureInfo.InvariantCulture)}");
        return string.Join("  ·  ", parts);
    }
}

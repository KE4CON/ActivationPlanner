using System.Globalization;

namespace ActivationPlanner.ProcessEngine.Nec;

/// <summary>
/// Parses NEC2 (NEC2++) text output into a <see cref="NecRawResult"/>: the feed-point impedance
/// from the ANTENNA INPUT PARAMETERS section and the gain samples from the RADIATION PATTERNS
/// section.
/// <para>
/// Pure and deterministic. The parser is section-header-driven and whitespace-tolerant rather than
/// fixed-column, so it survives the minor spacing differences between NEC ports. The exact
/// output layout is standard NEC2; validate against a real <c>nec2++</c> run once the user's
/// install is available (the shell-out makes this straightforward).
/// </para>
/// </summary>
public sealed class NecOutputParser
{
    private const string ImpedanceSection = "ANTENNA INPUT PARAMETERS";
    private const string PatternSection = "RADIATION PATTERNS";

    /// <summary>Parse a NEC output document.</summary>
    /// <exception cref="FormatException">No radiation-pattern section was found.</exception>
    public NecRawResult Parse(string output)
    {
        ArgumentNullException.ThrowIfNull(output);
        string[] lines = output.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        NecImpedance? impedance = ParseImpedance(lines);
        IReadOnlyList<NecRadiationSample> pattern = ParsePattern(lines);

        if (pattern.Count == 0)
            throw new FormatException(
                "No NEC RADIATION PATTERNS data found — not a pattern run or the output is truncated.");

        return new NecRawResult { Impedance = impedance, Pattern = pattern };
    }

    private static NecImpedance? ParseImpedance(string[] lines)
    {
        int start = FindSection(lines, ImpedanceSection);
        if (start < 0)
            return null;

        // Skip past the two-line column header to the first data row.
        for (int i = start + 1; i < lines.Length; i++)
        {
            string[] tokens = Tokenize(lines[i]);
            if (tokens.Length < 11)
                continue;

            // Data row: TAG SEG then nine real columns. Impedance real/imag are columns 7 & 8.
            if (!TryInt(tokens[0], out _) || !TryInt(tokens[1], out _))
                continue;
            if (TryDouble(tokens[6], out double r) && TryDouble(tokens[7], out double x))
                return new NecImpedance(r, x);
        }

        return null;
    }

    private static IReadOnlyList<NecRadiationSample> ParsePattern(string[] lines)
    {
        int start = FindSection(lines, PatternSection);
        if (start < 0)
            return [];

        var samples = new List<NecRadiationSample>();
        bool started = false;
        for (int i = start + 1; i < lines.Length; i++)
        {
            string[] tokens = Tokenize(lines[i]);

            // A data row begins with theta, phi and at least the three gain columns.
            bool isData = tokens.Length >= 5
                && TryDouble(tokens[0], out _) && TryDouble(tokens[1], out _)
                && TryDouble(tokens[2], out _) && TryDouble(tokens[3], out _)
                && TryDouble(tokens[4], out _);

            if (isData)
            {
                started = true;
                samples.Add(new NecRadiationSample(
                    ThetaDeg: double.Parse(tokens[0], CultureInfo.InvariantCulture),
                    PhiDeg: double.Parse(tokens[1], CultureInfo.InvariantCulture),
                    VerticalGainDb: double.Parse(tokens[2], CultureInfo.InvariantCulture),
                    HorizontalGainDb: double.Parse(tokens[3], CultureInfo.InvariantCulture),
                    TotalGainDb: double.Parse(tokens[4], CultureInfo.InvariantCulture)));
            }
            else if (started)
            {
                // Reached the end of the contiguous data block.
                break;
            }
        }

        return samples;
    }

    private static int FindSection(string[] lines, string title)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(title, StringComparison.Ordinal))
                return i;
        }
        return -1;
    }

    private static string[] Tokenize(string line) =>
        line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    private static bool TryDouble(string token, out double value) =>
        double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    private static bool TryInt(string token, out int value) =>
        int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
}

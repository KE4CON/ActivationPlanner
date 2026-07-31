using System.Globalization;
using System.Text.RegularExpressions;

namespace ActivationPlanner.ProcessEngine.Voacap;

/// <summary>
/// Parses raw VOACAP METHOD 30 (point-to-point) text output into
/// <see cref="VoacapRawPrediction"/>.
/// <para>
/// Pure and deterministic: text in, records out — no I/O, no domain knowledge.
/// The output is organized as one block per hour; each block opens with a FREQ row
/// (<c>hour, MUF, freq1..freqN</c>) followed by a fixed set of labeled parameter rows
/// (MODE, REL, SNR, S DBW, …). Every parameter row carries one leading MUF column and
/// then one value per frequency slot, with "-" for slots VOACAP did not evaluate.
/// Page headers repeat every LINEMAX lines and are skipped. Validated against the real
/// <c>tests/p2p/test01.out</c> fixture in the ProcessEngine tests.
/// </para>
/// </summary>
public sealed partial class VoacapOutputParser
{
    /// <summary>VOACAP parameter-row labels that follow a FREQ row within an hour block.</summary>
    private static readonly HashSet<string> RowLabels = new(StringComparer.Ordinal)
    {
        "MODE", "TANGLE", "DELAY", "V HITE", "MUFday", "LOSS", "DBU", "S DBW", "N DBW",
        "SNR", "RPWRG", "REL", "MPROB", "S PRB", "SIG LW", "SIG UP", "SNR LW", "SNR UP",
        "TGAIN", "RGAIN", "SNRxx",
    };

    /// <summary>Parse a full VOACAP output document.</summary>
    /// <exception cref="FormatException">No hour blocks were found (not a METHOD 30 run, or truncated output).</exception>
    public VoacapRawPrediction Parse(string output)
    {
        ArgumentNullException.ThrowIfNull(output);
        string[] lines = output.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

        var meta = ParseMetadata(lines);
        var hours = new List<VoacapHourBlock>();

        for (int i = 0; i < lines.Length; i++)
        {
            if (!IsFreqRow(lines[i]))
                continue;

            var block = ParseHourBlock(lines, ref i);
            if (block is not null)
                hours.Add(block);
        }

        if (hours.Count == 0)
            throw new FormatException(
                "No VOACAP hour blocks (FREQ rows) found — not a METHOD 30 run or the output is truncated.");

        return new VoacapRawPrediction
        {
            TransmitterLabel = meta.Tx,
            ReceiverLabel = meta.Rx,
            SunspotNumber = meta.Ssn,
            Hours = hours,
        };
    }

    /// <summary>A FREQ row ends in the literal "FREQ" and starts with the numeric hour.</summary>
    private static bool IsFreqRow(string line)
    {
        string t = line.TrimEnd();
        if (!t.EndsWith(" FREQ", StringComparison.Ordinal) && !t.Equals("FREQ", StringComparison.Ordinal))
            return false;
        string[] tokens = Tokenize(t);
        return tokens.Length >= 3
            && double.TryParse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out _);
    }

    /// <summary>
    /// Parse one hour block starting at the FREQ row at <paramref name="index"/>. Advances
    /// <paramref name="index"/> to the block's last consumed line. Returns null if the FREQ
    /// row is malformed.
    /// </summary>
    private static VoacapHourBlock? ParseHourBlock(string[] lines, ref int index)
    {
        string[] freqTokens = Tokenize(lines[index]);
        // Drop the trailing "FREQ" label; remaining = hour, MUF, then frequency slots.
        var cols = freqTokens[..^1];
        if (cols.Length < 3)
            return null;

        double hour = ParseDouble(cols[0]) ?? double.NaN;
        double muf = ParseDouble(cols[1]) ?? double.NaN;
        double?[] freqSlots = cols[2..].Select(ParseDouble).ToArray();
        int slotCount = freqSlots.Length;
        int columnCount = 1 + slotCount; // MUF column + one per frequency slot

        // Collect the labeled parameter rows that belong to this block.
        var rows = new Dictionary<string, string?[]>(StringComparer.Ordinal);
        int i = index + 1;
        for (; i < lines.Length; i++)
        {
            if (!TryParseParameterRow(lines[i], columnCount, out string? label, out string?[]? values))
                break;
            rows[label!] = values!;
        }

        index = i - 1; // leave index on the last consumed line; the outer loop's ++ moves on

        var samples = new List<VoacapFrequencySample>();
        for (int slot = 0; slot < slotCount; slot++)
        {
            if (freqSlots[slot] is not { } freq || freq <= 0.0)
                continue; // unused slot

            int col = 1 + slot; // column 0 is the MUF column
            samples.Add(BuildSample(freq, col, rows));
        }

        return new VoacapHourBlock { HourUtc = hour, MufMhz = muf, Samples = samples };
    }

    private static VoacapFrequencySample BuildSample(double freq, int col, Dictionary<string, string?[]> rows)
    {
        string? Raw(string label) => rows.TryGetValue(label, out var v) && col < v.Length ? v[col] : null;
        double? Num(string label) => ParseDouble(Raw(label));

        var rawRow = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var (label, values) in rows)
            rawRow[label] = col < values.Length ? values[col] : null;

        return new VoacapFrequencySample
        {
            FrequencyMhz = freq,
            Mode = Raw("MODE"),
            Reliability = Num("REL"),
            Snr = Num("SNR"),
            SignalPowerDbw = Num("S DBW"),
            NoisePowerDbw = Num("N DBW"),
            PathLossDb = Num("LOSS"),
            MufDays = Num("MUFday"),
            TakeoffAngleDeg = Num("TANGLE"),
            SnrAtRequiredReliabilityDb = Num("SNRxx"),
            RawRow = rawRow,
        };
    }

    /// <summary>
    /// A parameter row is <paramref name="columnCount"/> value tokens followed by a known
    /// label. Values that are "-" become null. Non-matching lines (blanks, page headers)
    /// end the block.
    /// </summary>
    private static bool TryParseParameterRow(string line, int columnCount, out string? label, out string?[]? values)
    {
        label = null;
        values = null;

        string t = line.TrimEnd();
        if (t.Length == 0)
            return false;

        string[] tokens = Tokenize(t);
        if (tokens.Length <= columnCount)
            return false;

        string candidateLabel = string.Join(' ', tokens[columnCount..]);
        if (!RowLabels.Contains(candidateLabel))
            return false;

        var cols = new string?[columnCount];
        for (int c = 0; c < columnCount; c++)
            cols[c] = tokens[c] == "-" ? null : tokens[c];

        label = candidateLabel;
        values = cols;
        return true;
    }

    private static (string? Tx, string? Rx, double? Ssn) ParseMetadata(string[] lines)
    {
        string? tx = null, rx = null;
        double? ssn = null;

        foreach (string line in lines)
        {
            if (ssn is null)
            {
                Match m = SsnRegex().Match(line);
                if (m.Success)
                    ssn = ParseDouble(m.Groups[1].Value);
            }

            // The circuit header line: "  <lat> N  <lon> W - <lat> N  <lon> E ..." is preceded
            // by the labels line: "  TANGIER, Morocco    BELGRADE    AZIMUTHS ...".
            if (tx is null && line.Contains("AZIMUTHS", StringComparison.Ordinal))
            {
                string labels = line[..line.IndexOf("AZIMUTHS", StringComparison.Ordinal)].Trim();
                // Split the two site labels on runs of 2+ spaces.
                string[] parts = MultiSpaceRegex().Split(labels);
                if (parts.Length >= 1) tx = parts[0].Trim();
                if (parts.Length >= 2) rx = parts[1].Trim();
            }

            if (ssn is not null && tx is not null)
                break;
        }

        return (tx, string.IsNullOrEmpty(rx) ? null : rx, ssn);
    }

    private static string[] Tokenize(string line) =>
        line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    private static double? ParseDouble(string? token) =>
        token is not null && token != "-"
            && double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double v)
            ? v
            : null;

    [GeneratedRegex(@"SSN\s*=\s*([0-9.]+)")]
    private static partial Regex SsnRegex();

    [GeneratedRegex(@"\s{2,}")]
    private static partial Regex MultiSpaceRegex();
}

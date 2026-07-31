using System.Globalization;

namespace ActivationPlanner.ProcessEngine.Nec;

/// <summary>
/// Card names and formatting for the NEC2 (NEC2++) antenna-geometry input deck.
/// <para>
/// Unlike the VOACAP deck, NEC2 input is free-format: the two-character card name is followed
/// by whitespace-separated fields, so the writer joins formatted fields with spaces rather than
/// placing them in fixed columns. Card names and default modeling parameters live here so the
/// deck writer contains no magic strings/numbers.
/// </para>
/// <para>This is Layer 1 (ProcessEngine): it knows the NEC wire format only — no antenna physics,
/// no planner domain. Validated against real example decks (necpp <c>example1.nec</c> /
/// <c>inverted_v.nec</c>).</para>
/// </summary>
public static class NecCardFormat
{
    // Card mnemonics.
    public const string CommentStart = "CM";
    public const string CommentEnd = "CE";
    public const string Wire = "GW";
    public const string GeometryEnd = "GE";
    public const string Ground = "GN";
    public const string Excitation = "EX";
    public const string Frequency = "FR";
    public const string RadiationPattern = "RP";
    public const string End = "EN";

    /// <summary>Excitation type 0 = applied-E-field voltage source (the usual feed).</summary>
    public const int VoltageSourceExcitation = 0;

    /// <summary>FR type 0 = linear frequency stepping.</summary>
    public const int LinearFrequencyStep = 0;

    /// <summary>RP mode 0 = normal radiation pattern in free space / over ground.</summary>
    public const int NormalRadiationPattern = 0;

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>Format a coordinate/length (metres) for a NEC field.</summary>
    public static string Coord(double metres) => metres.ToString("0.######", Inv);

    /// <summary>Format a wire radius (metres) in exponential form as NEC decks conventionally do.</summary>
    public static string Radius(double metres) => metres.ToString("0.#####E+00", Inv);

    /// <summary>Format a general real field (frequency MHz, ground constants, voltage, angles).</summary>
    public static string Real(double value) => value.ToString("0.######", Inv);

    /// <summary>Build a card line: mnemonic followed by space-separated fields.</summary>
    public static string Card(string mnemonic, params string[] fields) =>
        fields.Length == 0 ? mnemonic : mnemonic + " " + string.Join(' ', fields);

    /// <summary>Format an integer field.</summary>
    public static string Int(int value) => value.ToString(Inv);
}

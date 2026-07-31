using System.Globalization;
using System.Text;

namespace ActivationPlanner.ProcessEngine.Voacap;

/// <summary>
/// Fixed-column layout constants and low-level field formatters for the VOACAP
/// point-to-point ("circuit", METHOD 30) input deck.
/// <para>
/// The VOACAP engine reads its input deck as fixed-column Fortran records, so a
/// value in the wrong column silently produces a wrong (not failed) prediction —
/// the single most bug-prone part of the shell-out. Every column position and
/// field width lives here as a named constant, verified byte-for-byte against a
/// real known-good deck.
/// </para>
/// <para>
/// <b>Reference decks / formats</b> (validated against the jawatson/voacapl port,
/// the maintained Linux/macOS VOACAP that this project shells out to):
/// <list type="bullet">
/// <item>Sample deck: <c>tests/p2p/test01.dat</c></item>
/// <item>Keyword occupies columns 1-10; card data begins at column 11
///   (jawatson/voacapl wiki, "Input Cards").</item>
/// <item>ANTENNA Fortran format: <c>10X,4I5,f10.3,1x,a21,1x,f5.1,f10.4</c>.</item>
/// <item>MONTH: year in columns 11-14 (I4), values from column 15 (F5.2).</item>
/// <item>SYSTEM field order (from voacapl <c>read_asc.for</c>):
///   noise, min-angle, required-reliability, required-SNR,
///   multipath-power-tol, multipath-delay-tol.</item>
/// </list>
/// This is Layer 1 (ProcessEngine): it knows the VOACAP wire format only. It has
/// no planner-domain knowledge — callers hand it primitive numbers.
/// </para>
/// </summary>
public static class VoacapCardFormat
{
    /// <summary>Width of the leading keyword field. Card data starts at column 11.</summary>
    public const int KeywordWidth = 10;

    /// <summary>Width of a standard numeric sub-field (F5.x) on SYSTEM / FREQUENCY / coordinate cards.</summary>
    public const int NumericFieldWidth = 5;

    /// <summary>Width of an integer sub-field (I5) on the TIME card.</summary>
    public const int IntFieldWidth = 5;

    /// <summary>Maximum number of frequency slots on a FREQUENCY card.</summary>
    public const int MaxFrequencies = 11;

    /// <summary>The 21-character bracketed antenna-file field on the ANTENNA card (a21).</summary>
    public const int AntennaFileWidth = 21;

    /// <summary>Line separator VOACAP decks use. voacapl (Linux/macOS) reads either LF or CRLF; LF is the default we emit.</summary>
    public const string LineSeparator = "\n";

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>
    /// Left-justify a card keyword into the 10-column keyword field, so card data
    /// that follows begins at column 11. Throws if the keyword does not fit.
    /// </summary>
    public static string Keyword(string keyword)
    {
        ArgumentException.ThrowIfNullOrEmpty(keyword);
        if (keyword.Length > KeywordWidth)
            throw new ArgumentException($"Keyword '{keyword}' exceeds {KeywordWidth} columns.", nameof(keyword));
        return keyword.PadRight(KeywordWidth);
    }

    /// <summary>
    /// Format a real value right-justified into a fixed-width VOACAP numeric field,
    /// always containing a decimal point (Fortran F-format input). The most decimals
    /// that fit in <paramref name="width"/> are used, so e.g. 6.07 → " 6.07" and
    /// 11.85 → "11.85" in a 5-wide field — the exact packing VOACAP expects on the
    /// FREQUENCY card, where adjacent values legitimately run together.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The integer part alone (plus a decimal point) cannot fit in <paramref name="width"/>.
    /// </exception>
    public static string RealField(double value, int width = NumericFieldWidth)
    {
        for (int decimals = width; decimals >= 0; decimals--)
        {
            string s = decimals == 0
                ? Math.Round(value).ToString("0", Inv) + "."
                : value.ToString("F" + decimals.ToString(Inv), Inv);
            if (s.Length <= width)
                return s.PadLeft(width);
        }
        throw new ArgumentOutOfRangeException(
            nameof(value), value, $"Value does not fit in a {width}-column VOACAP field.");
    }

    /// <summary>
    /// Format a real value right-justified into <paramref name="width"/> columns with
    /// exactly <paramref name="decimals"/> decimal places (Fortran F<c>width</c>.<c>decimals</c>).
    /// Use this where VOACAP fixes the decimal count — the ANTENNA card's f10.3 / f5.1 /
    /// f10.4 fields and the MONTH / FPROB / FREQUENCY F5.2 fields.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The formatted value exceeds <paramref name="width"/>.</exception>
    public static string RealFieldFixed(double value, int width, int decimals)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(decimals);
        string s = value.ToString("F" + decimals.ToString(Inv), Inv);
        if (s.Length > width)
            throw new ArgumentOutOfRangeException(
                nameof(value), value,
                $"Value '{s}' does not fit in a {width}-column F{width}.{decimals} field.");
        return s.PadLeft(width);
    }

    /// <summary>Format an integer right-justified into a fixed-width field (Fortran I-format).</summary>
    public static string IntField(int value, int width = IntFieldWidth)
    {
        string s = value.ToString(Inv);
        if (s.Length > width)
            throw new ArgumentOutOfRangeException(
                nameof(value), value, $"Value does not fit in a {width}-column VOACAP field.");
        return s.PadLeft(width);
    }
}

/// <summary>
/// Mutable builder for a single fixed-column VOACAP card line. Fields are placed at
/// explicit 1-based columns and the builder pads with spaces in between, so callers
/// express the layout in the same coordinates as the format reference rather than by
/// counting spaces. Not thread-safe; intended for short-lived per-line use.
/// </summary>
public sealed class FixedColumnLine
{
    private readonly StringBuilder _sb = new();

    /// <summary>Place <paramref name="text"/> starting at 1-based <paramref name="column"/>, space-padding any gap.</summary>
    /// <exception cref="InvalidOperationException">The placement would overwrite already-written columns.</exception>
    public FixedColumnLine Place(int column, string text)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(column, 1);
        int start = column - 1;
        if (start < _sb.Length)
            throw new InvalidOperationException(
                $"Column {column} overlaps text already written to column {_sb.Length}.");
        if (start > _sb.Length)
            _sb.Append(' ', start - _sb.Length);
        _sb.Append(text);
        return this;
    }

    /// <summary>Append <paramref name="text"/> immediately after the current content (no gap).</summary>
    public FixedColumnLine Append(string text)
    {
        _sb.Append(text);
        return this;
    }

    /// <summary>The 1-based column the next appended character would occupy.</summary>
    public int NextColumn => _sb.Length + 1;

    /// <summary>Render the line. Trailing spaces are preserved (VOACAP reads by column, not token).</summary>
    public override string ToString() => _sb.ToString();
}

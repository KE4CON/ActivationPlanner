using System.Text;
using static ActivationPlanner.ProcessEngine.Nec.NecCardFormat;

namespace ActivationPlanner.ProcessEngine.Nec;

/// <summary>
/// Renders a <see cref="NecGeometryInput"/> into a NEC2 (NEC2++) input deck: comment cards, the
/// wire geometry, ground, excitation, frequency, and a radiation-pattern request.
/// <para>Pure and deterministic — no I/O, no antenna physics, no planner domain. Card order and
/// mnemonics follow the standard NEC2 deck (validated against real example decks).</para>
/// </summary>
public sealed class NecDeckWriter
{
    private const string LineSeparator = "\n";

    /// <summary>Render the deck as a single string.</summary>
    public string Write(NecGeometryInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Wires.Count == 0)
            throw new ArgumentException("At least one wire is required.", nameof(input));

        var sb = new StringBuilder();

        // Comments: CM lines, terminated by CE.
        if (input.Comments.Count == 0)
        {
            sb.Append(CommentEnd).Append(LineSeparator);
        }
        else
        {
            foreach (string comment in input.Comments)
                sb.Append(Card(CommentStart, comment)).Append(LineSeparator);
            sb.Append(CommentEnd).Append(LineSeparator);
        }

        // Geometry: one GW per wire, then GE (1 when a ground plane is present, else 0).
        foreach (NecWire w in input.Wires)
        {
            sb.Append(Card(Wire,
                Int(w.Tag), Int(w.Segments),
                Coord(w.X1), Coord(w.Y1), Coord(w.Z1),
                Coord(w.X2), Coord(w.Y2), Coord(w.Z2),
                Radius(w.RadiusMetres))).Append(LineSeparator);
        }
        sb.Append(Card(GeometryEnd, Int(input.Ground is null ? 0 : 1))).Append(LineSeparator);

        // Ground (GN) before the program cards.
        if (input.Ground is { } g)
        {
            sb.Append(Card(Ground,
                Int(g.Type), "0", "0", "0",
                Real(g.DielectricConstant), Real(g.ConductivitySm))).Append(LineSeparator);
        }

        // Frequency: single frequency (FR type 0, 1 step).
        sb.Append(Card(Frequency,
            Int(LinearFrequencyStep), "1", "0", "0",
            Real(input.FrequencyMhz), "0")).Append(LineSeparator);

        // Excitation: applied-E-field voltage source.
        NecExcitation ex = input.Excitation;
        sb.Append(Card(Excitation,
            Int(VoltageSourceExcitation), Int(ex.Tag), Int(ex.Segment), "0",
            Real(ex.VoltageReal), Real(ex.VoltageImag))).Append(LineSeparator);

        // Radiation pattern.
        NecRadiationPattern rp = input.RadiationPattern;
        sb.Append(Card(RadiationPattern,
            Int(NormalRadiationPattern), Int(rp.ThetaCount), Int(rp.PhiCount), "0",
            Real(rp.ThetaStartDeg), Real(rp.PhiStartDeg),
            Real(rp.ThetaStepDeg), Real(rp.PhiStepDeg))).Append(LineSeparator);

        sb.Append(End).Append(LineSeparator);
        return sb.ToString();
    }
}

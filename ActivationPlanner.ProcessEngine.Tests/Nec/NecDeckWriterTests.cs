using ActivationPlanner.ProcessEngine.Nec;

namespace ActivationPlanner.ProcessEngine.Tests.Nec;

public sealed class NecDeckWriterTests
{
    private static NecGeometryInput Dipole(NecGround? ground) => new()
    {
        Comments = ["DIPOLE TEST"],
        Wires = [new NecWire(Tag: 0, Segments: 21, -5.03, 0, 10, 5.03, 0, 10, RadiusMetres: 0.001)],
        FrequencyMhz = 14.1,
        Ground = ground,
        Excitation = new NecExcitation(Tag: 0, Segment: 11),
        RadiationPattern = new NecRadiationPattern(19, 1, 0, 0, 5, 0),
    };

    private static string[] Lines(NecGeometryInput input) =>
        new NecDeckWriter().Write(input).Split('\n').Select(l => l.TrimEnd()).ToArray();

    [Fact]
    public void Writes_the_expected_cards_for_a_grounded_dipole()
    {
        var lines = Lines(Dipole(new NecGround(2, 13.0, 0.005)));

        Assert.Contains("CM DIPOLE TEST", lines);
        Assert.Contains("CE", lines);
        Assert.Contains("GW 0 21 -5.03 0 10 5.03 0 10 1E-03", lines);
        Assert.Contains("GE 1", lines);          // ground plane present
        Assert.Contains("GN 2 0 0 0 13 0.005", lines);
        Assert.Contains("EX 0 0 11 0 1 0", lines);
        Assert.Contains("FR 0 1 0 0 14.1 0", lines);
        Assert.Contains("RP 0 19 1 0 0 0 5 0", lines);
        Assert.Contains("EN", lines);
    }

    [Fact]
    public void Free_space_model_uses_ge_zero_and_no_ground_card()
    {
        var lines = Lines(Dipole(ground: null));

        Assert.Contains("GE 0", lines);
        Assert.DoesNotContain(lines, l => l.StartsWith("GN", StringComparison.Ordinal));
    }

    [Fact]
    public void Deck_ends_with_en()
    {
        var lines = Lines(Dipole(null)).Where(l => l.Length > 0).ToArray();
        Assert.Equal("EN", lines[^1]);
    }

    [Fact]
    public void Rejects_a_deck_with_no_wires()
    {
        var writer = new NecDeckWriter();
        var input = Dipole(null) with { Wires = [] };
        Assert.Throws<ArgumentException>(() => writer.Write(input));
    }
}

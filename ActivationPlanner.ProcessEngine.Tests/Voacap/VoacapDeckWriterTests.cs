using ActivationPlanner.ProcessEngine.Tests.Fixtures;
using ActivationPlanner.ProcessEngine.Voacap;

namespace ActivationPlanner.ProcessEngine.Tests.Voacap;

/// <summary>
/// Pins the fixed-column deck writer to the real VOACAP wire format by reconstructing
/// the exact scenario in <c>tests/p2p/test01.dat</c> and comparing the bug-prone cards
/// byte-for-byte against that known-good deck.
/// </summary>
public sealed class VoacapDeckWriterTests
{
    /// <summary>The sample deck's scenario, expressed as a <see cref="VoacapDeckInput"/>.</summary>
    private static VoacapDeckInput SampleInput() => new()
    {
        // CIRCUIT: TANGIER 35.80N 5.90W  ->  BELGRADE 44.90N 20.50E, short path.
        TxLatitudeDeg = 35.80,
        TxLongitudeDeg = -5.90,
        RxLatitudeDeg = 44.90,
        RxLongitudeDeg = 20.50,
        Path = VoacapPath.Short,
        TxLabel = "TANGIER, Morocco",
        RxLabel = "BELGRADE",
        StartHourUtc = 1,
        StopHourUtc = 24,
        HourIncrement = 1,
        Year = 1994,
        MonthValue = 6.00,
        SunspotNumber = 100.0,
        Coefficients = VoacapCoefficients.Ccir,
        NoiseDbw = 145.0,
        MinTakeoffAngleDeg = 0.10,
        RequiredReliabilityPercent = 90.0,
        RequiredSnrDb = 73.0,
        MultipathPowerToleranceDb = 3.00,
        MultipathDelayToleranceMs = 0.10,
        LayerProbabilities = [1.00, 1.00, 1.00, 0.00],
        TxAntenna = new VoacapAntenna("samples/sample.00", BearingDeg: 90.0, PowerKw: 500.0,
            GainOffsetDbi: 10.000, MinFrequencyMhz: 2, MaxFrequencyMhz: 30),
        RxAntenna = new VoacapAntenna("samples/sample.00", BearingDeg: 270.0, PowerKw: 20.0,
            GainOffsetDbi: 0.000, MinFrequencyMhz: 2, MaxFrequencyMhz: 30),
        FrequenciesMhz = [6.07, 7.20, 9.70, 11.85, 13.70, 15.35, 17.73, 21.65, 25.89],
        Method = 30,
        LinesPerPage = 55,
    };

    private static string CardFromWriter(string keyword)
    {
        string deck = new VoacapDeckWriter().Write(SampleInput());
        return deck.Split('\n').Single(l => l.StartsWith(keyword, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("COEFFS")]
    [InlineData("TIME")]
    [InlineData("MONTH")]
    [InlineData("SUNSPOT")]
    [InlineData("LABEL")]
    [InlineData("CIRCUIT")]
    [InlineData("FPROB")]
    [InlineData("FREQUENCY")]
    [InlineData("METHOD")]
    public void Card_matches_known_good_deck_byte_for_byte(string keyword)
    {
        string expected = VoacapFixtures.DeckCard(keyword);
        string actual = CardFromWriter(keyword);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Both_antenna_cards_match_known_good_deck()
    {
        string[] expected = VoacapFixtures.SampleDeckLines()
            .Where(l => l.StartsWith("ANTENNA", StringComparison.Ordinal)).ToArray();
        string[] actual = new VoacapDeckWriter().Write(SampleInput()).Split('\n')
            .Where(l => l.StartsWith("ANTENNA", StringComparison.Ordinal)).ToArray();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Circuit_places_hemisphere_letters_at_columns_16_26_36_46()
    {
        string circuit = CardFromWriter("CIRCUIT");
        Assert.Equal('N', circuit[15]); // TX lat hemisphere, column 16 (0-based 15)
        Assert.Equal('W', circuit[25]); // TX lon hemisphere, column 26
        Assert.Equal('N', circuit[35]); // RX lat hemisphere, column 36
        Assert.Equal('E', circuit[45]); // RX lon hemisphere, column 46
    }

    [Fact]
    public void Longitude_over_100_degrees_still_fits_five_columns()
    {
        // A US west-coast circuit: 122.5W must not overflow the 5-column value field.
        var input = SampleInput() with { RxLongitudeDeg = -122.50 };
        string circuit = new VoacapDeckWriter().Write(input).Split('\n')
            .Single(l => l.StartsWith("CIRCUIT", StringComparison.Ordinal));
        Assert.Equal("122.5", circuit.Substring(40, 5)); // RX lon value, columns 41-45
        Assert.Equal('W', circuit[45]);                  // hemisphere still at column 46
    }

    [Fact]
    public void Deck_ends_with_execute_then_quit()
    {
        string[] lines = new VoacapDeckWriter().Write(SampleInput())
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal("QUIT", lines[^1]);
        Assert.Equal("EXECUTE", lines[^2]);
    }

    [Fact]
    public void Rejects_more_than_eleven_frequencies()
    {
        var input = SampleInput() with
        {
            FrequenciesMhz = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
        };
        Assert.Throws<ArgumentException>(() => new VoacapDeckWriter().Write(input));
    }
}

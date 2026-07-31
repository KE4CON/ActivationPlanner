namespace ActivationPlanner.ProcessEngine.Tests.Fixtures;

/// <summary>
/// Loads the real known-good VOACAP fixtures shipped with the jawatson/voacapl port
/// (<c>tests/p2p/test01.dat</c> and its <c>test01.out</c>). These pin the writer and
/// parser to actual VOACAP wire formats rather than our own assumptions.
/// </summary>
internal static class VoacapFixtures
{
    private static string Path(string name) =>
        System.IO.Path.Combine(AppContext.BaseDirectory, "Fixtures", "p2p", name);

    /// <summary>Sample point-to-point input deck, split into lines (line endings normalized to LF).</summary>
    public static string[] SampleDeckLines() => Normalize(File.ReadAllText(Path("test01.dat")));

    /// <summary>Raw sample VOACAP output, split into lines (line endings normalized to LF).</summary>
    public static string[] SampleOutputLines() => Normalize(File.ReadAllText(Path("test01.out")));

    /// <summary>Raw sample VOACAP output as a single LF-normalized string.</summary>
    public static string SampleOutputText() => string.Join('\n', SampleOutputLines());

    /// <summary>Find the sample-deck line beginning with the given card keyword.</summary>
    public static string DeckCard(string keyword) =>
        SampleDeckLines().Single(l => l.StartsWith(keyword, StringComparison.Ordinal));

    private static string[] Normalize(string text) =>
        text.Replace("\r\n", "\n").Replace('\r', '\n').TrimEnd('\n').Split('\n');
}

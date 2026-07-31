namespace ActivationPlanner.ProcessEngine.Tests.Fixtures;

/// <summary>
/// Loads the NEC output fixture — a <b>real nec2c run</b> of a 20m dipole at 10m over average
/// ground (14.1 MHz), captured to pin the parser to genuine NEC2 output (impedance + an
/// elevation-cut radiation pattern). nec2c labels the gain columns MAJOR/MINOR/TOTAL; the parser
/// reads them positionally and uses TOTAL.
/// </summary>
internal static class NecFixtures
{
    public static string DipoleOutputText() =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "nec", "dipole.out"));
}

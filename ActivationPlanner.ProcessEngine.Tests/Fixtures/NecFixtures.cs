namespace ActivationPlanner.ProcessEngine.Tests.Fixtures;

/// <summary>
/// Loads the NEC output fixture. It is a faithful reconstruction of the standard NEC2 output
/// (impedance + an elevation-cut radiation pattern); validate the parser against a real nec2++
/// run once that install is available.
/// </summary>
internal static class NecFixtures
{
    public static string DipoleOutputText() =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "nec", "dipole.out"));
}

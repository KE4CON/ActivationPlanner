namespace ActivationPlanner.PropagationModel.Voacap;

/// <summary>
/// Man-made noise environment at the receive site, mapped to VOACAP's 3 MHz man-made
/// noise figure (ITU-R P.372 categories). Values are the positive dBW magnitude VOACAP's
/// SYSTEM card expects; a quieter site (larger magnitude) improves predicted SNR.
/// </summary>
public enum NoiseEnvironment
{
    /// <summary>Noisy urban/industrial site (~-136 dBW).</summary>
    City,

    /// <summary>Typical suburban/residential site (~-145 dBW) — the common default.</summary>
    Residential,

    /// <summary>Rural site (~-150 dBW).</summary>
    Rural,

    /// <summary>Quiet rural site (~-155 dBW).</summary>
    QuietRural,

    /// <summary>Remote, RF-quiet site (~-164 dBW).</summary>
    Remote,
}

/// <summary>Maps a <see cref="NoiseEnvironment"/> to the VOACAP 3 MHz noise magnitude in dBW.</summary>
public static class NoiseEnvironments
{
    /// <summary>Positive dBW magnitude for the SYSTEM card's noise field.</summary>
    public static double NoiseDbw(this NoiseEnvironment environment) => environment switch
    {
        NoiseEnvironment.City => 136.0,
        NoiseEnvironment.Residential => 145.0,
        NoiseEnvironment.Rural => 150.0,
        NoiseEnvironment.QuietRural => 155.0,
        NoiseEnvironment.Remote => 164.0,
        _ => 145.0,
    };
}

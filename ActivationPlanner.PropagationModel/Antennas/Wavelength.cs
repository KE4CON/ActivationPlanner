namespace ActivationPlanner.PropagationModel.Antennas;

/// <summary>
/// Wavelength conversions used by the antenna Option A/B trigger math. Antenna dimensions
/// are entered in feet (see <see cref="Gear.AntennaProfile"/>); propagation reasoning is
/// wavelength-relative and per-band, so these helpers turn a physical length at a given
/// frequency into a fraction of a wavelength.
/// <para>
/// Pure domain math — no I/O, no VOACAP knowledge.
/// </para>
/// </summary>
public static class Wavelength
{
    /// <summary>Speed of light as the numerator of λ(m) = c / f, with c in metre·MHz (299.792458).</summary>
    public const double SpeedOfLightMHzMetres = 299.792458;

    /// <summary>Metres per foot (exact).</summary>
    public const double MetresPerFoot = 0.3048;

    /// <summary>Wavelength in metres for a frequency in MHz.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frequencyMhz"/> is not positive.</exception>
    public static double Metres(double frequencyMhz)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(frequencyMhz);
        return SpeedOfLightMHzMetres / frequencyMhz;
    }

    /// <summary>Convert feet to metres.</summary>
    public static double FeetToMetres(double feet) => feet * MetresPerFoot;

    /// <summary>
    /// Express a physical dimension (in feet) as a fraction of a wavelength at the given
    /// frequency — e.g. a 33 ft dipole leg at 14.1 MHz. Zero feet returns 0.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="feet"/> is negative or <paramref name="frequencyMhz"/> is not positive.
    /// </exception>
    public static double InWavelengths(double feet, double frequencyMhz)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(feet);
        return FeetToMetres(feet) / Metres(frequencyMhz);
    }
}

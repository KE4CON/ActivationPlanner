using ActivationPlanner.PropagationModel.Antennas;

namespace ActivationPlanner.PropagationModel.Tests.Antennas;

public sealed class WavelengthTests
{
    [Theory]
    [InlineData(14.1, 21.2618)]   // 20m
    [InlineData(7.1, 42.2243)]    // 40m
    [InlineData(28.3, 10.5934)]   // 10m
    public void Metres_computes_wavelength_from_frequency(double freqMhz, double expectedMetres)
    {
        Assert.Equal(expectedMetres, Wavelength.Metres(freqMhz), precision: 3);
    }

    [Fact]
    public void InWavelengths_half_wave_dipole_on_20m_is_about_half_a_wavelength()
    {
        // A 20m half-wave dipole is ~34.9 ft end to end.
        double wl = Wavelength.InWavelengths(feet: 34.88, frequencyMhz: 14.1);
        Assert.Equal(0.5, wl, precision: 2);
    }

    [Fact]
    public void InWavelengths_is_zero_for_zero_length()
    {
        Assert.Equal(0.0, Wavelength.InWavelengths(0.0, 14.1));
    }

    [Fact]
    public void Same_length_is_more_wavelengths_at_higher_frequency()
    {
        double onForty = Wavelength.InWavelengths(66.0, 7.1);
        double onTwenty = Wavelength.InWavelengths(66.0, 14.1);
        Assert.True(onTwenty > onForty); // shorter wavelength -> more wavelengths for the same wire
    }

    [Fact]
    public void Metres_rejects_nonpositive_frequency()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Wavelength.Metres(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => Wavelength.Metres(-1));
    }

    [Fact]
    public void InWavelengths_rejects_negative_length()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Wavelength.InWavelengths(-1, 14.1));
    }
}

using System.IO.Ports;
using ActivationPlanner.PropagationModel.Geo;

namespace ActivationPlanner.Services.Location;

/// <summary>
/// Reads a fix from an external hardware GPS receiver connected over USB/serial (NMEA 0183).
/// Auto-scans the available serial ports, listens briefly for a valid position sentence, and
/// returns the first fix found. Cross-platform via <see cref="SerialPort"/> — the same code works
/// on Windows (COMx), Linux, and Raspberry Pi (/dev/ttyUSB*, /dev/ttyACM*, /dev/serial0).
/// <para>NMEA parsing is delegated to the tested <see cref="NmeaLocationParser"/>; this class only
/// does the serial I/O. If no receiver is present it throws so the composite provider can fall back
/// to network geo-IP.</para>
/// </summary>
public sealed class SerialGpsLocationProvider : ILocationProvider
{
    private readonly IReadOnlyList<int> _baudRates;
    private readonly TimeSpan _perPortListen;

    /// <param name="baudRates">Baud rates to try per port (defaults to the common GPS rates).</param>
    /// <param name="perPortListen">How long to listen on each port/baud before moving on.</param>
    public SerialGpsLocationProvider(IReadOnlyList<int>? baudRates = null, TimeSpan? perPortListen = null)
    {
        _baudRates = baudRates ?? [9600, 4800];
        _perPortListen = perPortListen ?? TimeSpan.FromSeconds(2);
    }

    /// <inheritdoc />
    public string SourceLabel => "GPS (NMEA)";

    /// <inheritdoc />
    public async Task<LocationFix> GetCurrentAsync(CancellationToken ct = default)
    {
        string[] ports;
        try
        {
            ports = SerialPort.GetPortNames();
        }
        catch (Exception ex)
        {
            throw new LocationUnavailableException("Serial ports are not accessible on this system.", ex);
        }

        foreach (string port in ports)
        {
            foreach (int baud in _baudRates)
            {
                ct.ThrowIfCancellationRequested();
                GeoLocation? fix = await TryReadPortAsync(port, baud, ct).ConfigureAwait(false);
                if (fix is { } location)
                {
                    return new LocationFix
                    {
                        Location = location,
                        SourceLabel = SourceLabel,
                        PlaceName = null,
                        IsApproximate = false,
                    };
                }
            }
        }

        throw new LocationUnavailableException(
            "No GPS receiver found on any serial port. Connect a USB/serial NMEA GPS, or use network location.");
    }

    private Task<GeoLocation?> TryReadPortAsync(string port, int baud, CancellationToken ct) =>
        Task.Run(() =>
        {
            try
            {
                using var serial = new SerialPort(port, baud)
                {
                    ReadTimeout = 1000,
                    NewLine = "\r\n",
                };
                serial.Open();

                DateTime deadline = DateTime.UtcNow + _perPortListen;
                while (DateTime.UtcNow < deadline && !ct.IsCancellationRequested)
                {
                    string line;
                    try
                    {
                        line = serial.ReadLine();
                    }
                    catch (TimeoutException)
                    {
                        continue;
                    }

                    if (NmeaLocationParser.TryParse(line) is { } fix)
                        return (GeoLocation?)fix;
                }
            }
            catch
            {
                // Port busy, permission denied, not a GPS, etc. — treat as "no fix here".
            }

            return null;
        }, ct);
}

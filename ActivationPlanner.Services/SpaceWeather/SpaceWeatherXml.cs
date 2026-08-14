using System.Globalization;
using System.Xml.Linq;

namespace ActivationPlanner.Services.SpaceWeather;

/// <summary>
/// Pure parser for the N0NBH (hamqsl.com) solar XML feed — kept separate from the HTTP client so it
/// is fully unit-testable. Shape: &lt;solar&gt;&lt;solardata&gt;&lt;solarflux/&gt;&lt;sunspots/&gt;
/// &lt;aindex/&gt;&lt;kindex/&gt;&lt;updated/&gt;…&lt;/solardata&gt;&lt;/solar&gt;.
/// </summary>
public static class SpaceWeatherXml
{
    public static SolarConditions Parse(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        XDocument doc;
        try
        {
            doc = XDocument.Parse(xml);
        }
        catch (System.Xml.XmlException ex)
        {
            throw new SpaceWeatherFormatException("Solar-data XML was malformed.", ex);
        }

        XElement data = doc.Descendants("solardata").FirstOrDefault()
            ?? throw new SpaceWeatherFormatException("Solar-data XML did not contain a <solardata> element.");

        return new SolarConditions
        {
            SunspotNumber = ReadInt(data, "sunspots"),
            SolarFluxIndex = ReadInt(data, "solarflux"),
            AIndex = ReadInt(data, "aindex"),
            KIndex = ReadInt(data, "kindex"),
            UpdatedText = ReadString(data, "updated"),
        };
    }

    private static int? ReadInt(XElement parent, string name)
    {
        string? raw = parent.Element(name)?.Value?.Trim();
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : null;
    }

    private static string? ReadString(XElement parent, string name)
    {
        string? raw = parent.Element(name)?.Value?.Trim();
        return string.IsNullOrWhiteSpace(raw) ? null : raw;
    }
}

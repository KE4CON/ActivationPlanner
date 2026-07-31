using System;
using System.Globalization;
using System.Text;
using ActivationPlanner.PropagationModel.Gear;
using Avalonia.Data.Converters;

namespace ActivationPlanner.UI.Converters;

/// <summary>
/// Turns a gear/antenna category enum into a human-friendly label for display: known values get a
/// hand-tuned name (e.g. <c>DigitalInterface</c> -> "Digital Interface", <c>Emcomm</c> -> "EMCOMM",
/// <c>NvisCrossedDipole</c> -> "NVIS Crossed Dipole"); anything else falls back to splitting the
/// PascalCase enum name into spaced words. Display-only (no ConvertBack — pickers bind SelectedItem
/// to the underlying enum).
/// </summary>
public sealed class CategoryLabelConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        GearCategory.DigitalInterface => "Digital Interface",
        GearCategory.Emcomm => "EMCOMM",
        AntennaCategory.NvisCrossedDipole => "NVIS Crossed Dipole",
        AntennaCategory.EndFedHalfWave => "End-Fed Half-Wave",
        AntennaCategory.MagneticLoop => "Magnetic Loop",
        Enum e => Spaced(e.ToString()),
        _ => value?.ToString() ?? string.Empty,
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    /// <summary>Insert a space before each interior capital letter: "FooBar" -> "Foo Bar".</summary>
    private static string Spaced(string name)
    {
        var sb = new StringBuilder(name.Length + 4);
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
                sb.Append(' ');
            sb.Append(name[i]);
        }
        return sb.ToString();
    }
}

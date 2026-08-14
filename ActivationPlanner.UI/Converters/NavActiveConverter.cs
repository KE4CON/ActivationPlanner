using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ActivationPlanner.UI.Converters;

/// <summary>
/// True when the bound <see cref="ViewModels.NavPage"/> value matches the ConverterParameter name.
/// Drives the accent (blue) highlight on the active top-nav button; all others stay the default gray.
/// </summary>
public sealed class NavActiveConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is not null && parameter is not null
        && string.Equals(value.ToString(), parameter.ToString(), StringComparison.Ordinal);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

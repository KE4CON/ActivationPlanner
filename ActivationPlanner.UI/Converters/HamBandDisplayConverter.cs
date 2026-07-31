using System;
using System.Globalization;
using ActivationPlanner.PropagationModel.Bands;
using Avalonia.Data.Converters;

namespace ActivationPlanner.UI.Converters;

/// <summary>Maps a <see cref="HamBand"/> to its display name (e.g. "20m") for pickers.</summary>
public sealed class HamBandDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is HamBand band ? HamBands.DisplayName(band) : value?.ToString() ?? string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

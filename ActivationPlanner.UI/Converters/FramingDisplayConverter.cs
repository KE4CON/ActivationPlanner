using System;
using System.Globalization;
using ActivationPlanner.PropagationModel.Missions;
using Avalonia.Data.Converters;

namespace ActivationPlanner.UI.Converters;

/// <summary>Maps a <see cref="PropagationFraming"/> to a friendly label for pickers.</summary>
public sealed class FramingDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        PropagationFraming.RegionalNvis => "Regional / NVIS",
        PropagationFraming.DxPointToPoint => "DX / point-to-point",
        _ => value?.ToString() ?? string.Empty,
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

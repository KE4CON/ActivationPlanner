using Avalonia.Media;
using ActivationPlanner.Services.Planning;

namespace ActivationPlanner.UI.ViewModels.Planning;

/// <summary>
/// Display wrapper for an owned antenna's Option A/B status on a band. Immutable.
/// </summary>
public sealed class AntennaSuggestionViewModel
{
    private static readonly IBrush LibraryBrush = new SolidColorBrush(Color.FromRgb(0x3A, 0xA0, 0x50));
    private static readonly IBrush ModelingBrush = new SolidColorBrush(Color.FromRgb(0xC9, 0x9A, 0x2B));

    public AntennaSuggestionViewModel(AntennaSuggestion suggestion)
    {
        Name = suggestion.Antenna.Name;
        IsLibraryReady = suggestion.IsLibraryReady;
        StatusText = IsLibraryReady ? "Library-ready" : "Needs modeling";
        StatusBrush = IsLibraryReady ? LibraryBrush : ModelingBrush;
        Reason = suggestion.Modeling.Reason;
    }

    /// <summary>Antenna name.</summary>
    public string Name { get; }

    /// <summary>True when a community-library pattern suffices (Option A).</summary>
    public bool IsLibraryReady { get; }

    /// <summary>Short status label.</summary>
    public string StatusText { get; }

    /// <summary>Badge colour: green for library-ready, amber for needs-modeling.</summary>
    public IBrush StatusBrush { get; }

    /// <summary>Full explanation of the Option A/B decision.</summary>
    public string Reason { get; }
}

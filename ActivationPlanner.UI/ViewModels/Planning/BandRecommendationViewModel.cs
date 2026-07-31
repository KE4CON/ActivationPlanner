using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Media;
using ActivationPlanner.Services.Planning;

namespace ActivationPlanner.UI.ViewModels.Planning;

/// <summary>
/// Display model for one band in the plan: headline reliability, the 24-hour heatmap strip,
/// and the operator's matching antennas. Immutable — rebuilt each time a plan is generated.
/// </summary>
public sealed class BandRecommendationViewModel
{
    public BandRecommendationViewModel(BandRecommendation band)
    {
        BandName = band.BandName;
        FrequencyLabel = band.FrequencyMhz.ToString("0.0##", CultureInfo.InvariantCulture) + " MHz";

        AverageReliability = band.AverageReliability;
        AverageReliabilityPercent = (int)System.Math.Round(band.AverageReliability * 100);
        AverageReliabilityLabel = AverageReliabilityPercent + "%";
        AverageBrush = ReliabilityPalette.Brush(band.AverageReliability);

        BestHourLabel = band.BestHourUtc is { } h
            ? $"best {h:00}:00 UTC ({band.BestReliability * 100:0}%)"
            : "no opening";

        HourCells = band.Prediction.Hours
            .Select(s => new HourCellViewModel(s.HourUtc, s.Reliability, s.MufMhz))
            .ToList();

        Antennas = band.OwnedAntennas
            .Select(a => new AntennaSuggestionViewModel(a))
            .ToList();
    }

    public string BandName { get; }
    public string FrequencyLabel { get; }

    /// <summary>Mean reliability 0-1 (used for the bar width).</summary>
    public double AverageReliability { get; }

    public int AverageReliabilityPercent { get; }
    public string AverageReliabilityLabel { get; }
    public IBrush AverageBrush { get; }
    public string BestHourLabel { get; }

    public IReadOnlyList<HourCellViewModel> HourCells { get; }
    public IReadOnlyList<AntennaSuggestionViewModel> Antennas { get; }

    /// <summary>True when the operator owns no antenna for this band.</summary>
    public bool HasNoAntennas => Antennas.Count == 0;
}

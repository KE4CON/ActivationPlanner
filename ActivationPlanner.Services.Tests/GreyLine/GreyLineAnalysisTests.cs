using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Geo;
using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.Services.GreyLine;
using ActivationPlanner.Services.Planning;

namespace ActivationPlanner.Services.Tests.GreyLine;

public sealed class GreyLineAnalysisTests
{
    private static BandRecommendation Band(HamBand band, double freq, int hour, double rel) => new()
    {
        Band = band,
        FrequencyMhz = freq,
        OwnedAntennas = [],
        Prediction = new BandPrediction
        {
            Band = band,
            FrequencyMhz = freq,
            Hours = [new BandHourSample { HourUtc = hour, Reliability = rel }],
        },
    };

    // Bands all evaluated at hour 12 (the sunrise hour used in the tests).
    private static SessionPlan Plan() => new()
    {
        HoursUtc = [12],
        DistanceKm = 1000,
        Bands =
        [
            Band(HamBand.M40, 7.1, 12, 0.30),
            Band(HamBand.M20, 14.1, 12, 0.90),
            Band(HamBand.M15, 21.1, 12, 0.55),
        ],
    };

    [Fact]
    public void Analyze_builds_sunrise_and_sunset_windows()
    {
        var events = new SolarEvents(SunriseUtcHour: 11.8, SunsetUtcHour: 2.4);
        var report = GreyLineAnalysis.Analyze(Plan(), events);

        Assert.True(report.HasGreyLine);
        Assert.Equal(2, report.Windows.Count);
        Assert.Equal("Sunrise", report.Windows[0].Label);
        Assert.Equal("Sunset", report.Windows[1].Label);
        Assert.Equal(12, report.Windows[0].HourUtc); // 11.8 rounds to 12
    }

    [Fact]
    public void Analyze_ranks_bands_by_reliability_at_the_grey_line_hour()
    {
        var events = new SolarEvents(SunriseUtcHour: 12.0, SunsetUtcHour: 2.0);
        var report = GreyLineAnalysis.Analyze(Plan(), events);

        var bands = report.Windows.First(w => w.Label == "Sunrise").Bands;
        Assert.Equal(HamBand.M20, bands[0].Band); // 0.90 first
        Assert.Equal(HamBand.M15, bands[1].Band); // 0.55
        Assert.Equal(HamBand.M40, bands[2].Band); // 0.30
    }

    [Fact]
    public void Analyze_reports_no_grey_line_for_polar_day()
    {
        var events = new SolarEvents(null, null);
        var report = GreyLineAnalysis.Analyze(Plan(), events);
        Assert.False(report.HasGreyLine);
        Assert.Empty(report.Windows);
    }

    [Fact]
    public void Midnight_event_maps_to_hour_24()
    {
        var events = new SolarEvents(SunriseUtcHour: 0.1, SunsetUtcHour: 12.0);
        var report = GreyLineAnalysis.Analyze(Plan(), events);
        Assert.Equal(24, report.Windows.First(w => w.Label == "Sunrise").HourUtc);
    }
}

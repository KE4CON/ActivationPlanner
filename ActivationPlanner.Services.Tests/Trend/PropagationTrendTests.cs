using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.PropagationModel.Voacap;
using ActivationPlanner.Services.Planning;
using ActivationPlanner.Services.Trend;

namespace ActivationPlanner.Services.Tests.Trend;

public sealed class PropagationTrendTests
{
    private static TrendSnapshot Snap(DateTime at, double m20) => new()
    {
        CapturedAtUtc = at,
        HourUtc = at.Hour == 0 ? 24 : at.Hour,
        Reliability = new Dictionary<HamBand, double?> { [HamBand.M20] = m20 },
    };

    [Fact]
    public void Add_keeps_samples_in_order()
    {
        var trend = new PropagationTrend();
        var t0 = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);
        trend.Add(Snap(t0, 0.5));
        trend.Add(Snap(t0.AddMinutes(15), 0.6));

        Assert.Equal(2, trend.Snapshots.Count);
        Assert.Equal(0.6, trend.Latest!.Reliability[HamBand.M20]);
        Assert.Equal([0.5, 0.6], trend.SeriesFor(HamBand.M20));
    }

    [Fact]
    public void Add_drops_samples_older_than_the_window()
    {
        var trend = new PropagationTrend(TimeSpan.FromHours(1));
        var t0 = new DateTime(2026, 7, 31, 12, 0, 0, DateTimeKind.Utc);
        trend.Add(Snap(t0, 0.5));
        trend.Add(Snap(t0.AddMinutes(30), 0.6));
        trend.Add(Snap(t0.AddMinutes(90), 0.7)); // now the first (t0) is > 1h behind the newest

        Assert.Equal(2, trend.Snapshots.Count);
        Assert.DoesNotContain(trend.Snapshots, s => s.CapturedAtUtc == t0);
    }

    [Fact]
    public void SnapshotFrom_reads_reliability_at_the_capture_hour()
    {
        var plan = new SessionPlan
        {
            HoursUtc = [13, 14],
            DistanceKm = 1000,
            Bands =
            [
                new BandRecommendation
                {
                    Band = HamBand.M20,
                    FrequencyMhz = 14.1,
                    OwnedAntennas = [],
                    Prediction = new BandPrediction
                    {
                        Band = HamBand.M20,
                        FrequencyMhz = 14.1,
                        Hours =
                        [
                            new BandHourSample { HourUtc = 13, Reliability = 0.42 },
                            new BandHourSample { HourUtc = 14, Reliability = 0.88 },
                        ],
                    },
                },
            ],
        };

        var snap = PropagationTrend.SnapshotFrom(plan, new DateTime(2026, 7, 31, 14, 5, 0, DateTimeKind.Utc));

        Assert.Equal(14, snap.HourUtc);
        Assert.Equal(0.88, snap.Reliability[HamBand.M20]);
    }

    [Fact]
    public void SnapshotFrom_maps_midnight_to_hour_24()
    {
        var plan = new SessionPlan { HoursUtc = [24], DistanceKm = 0, Bands = [] };
        var snap = PropagationTrend.SnapshotFrom(plan, new DateTime(2026, 7, 31, 0, 30, 0, DateTimeKind.Utc));
        Assert.Equal(24, snap.HourUtc);
    }
}

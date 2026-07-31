using ActivationPlanner.PropagationModel.Bands;
using ActivationPlanner.Services.Pota;

namespace ActivationPlanner.Services.Tests.Pota;

public sealed class PotaJsonTests
{
    // Shapes captured from live api.pota.app responses.
    private const string SpotsJson = """
    [
      {"spotId":54282220,"activator":"WY7BW","frequency":"14295.0","mode":"SSB","reference":"US-4534",
       "parkName":null,"spotTime":"2026-07-31T01:21:36","spotter":"KQ4WFX","comments":"[57 north GA]",
       "source":"hunterlog","name":"Bighorn National Forest","locationDesc":"US-WY","grid4":"DN64",
       "grid6":"DN64mt","latitude":44.804,"longitude":-106.927,"count":22,"expire":1626},
      {"spotId":54282221,"activator":"K4ABC","frequency":"7032.0","mode":"CW","reference":"US-0001",
       "spotTime":"2026-07-31T01:22:00","spotter":"K4ABC","comments":"self spot","name":"Acadia NP",
       "locationDesc":"US-ME","grid6":"FN54","latitude":44.35,"longitude":-68.21},
      {"spotId":54282222,"activator":"N0VHF","frequency":"146520.0","mode":"FM","reference":"US-9999",
       "spotter":"N0VHF","name":"Some Park","latitude":40.0,"longitude":-105.0},
      {"spotId":54282223,"activator":"NOFREQ","mode":"SSB","reference":"US-1234"}
    ]
    """;

    private const string ParkJson = """
    {"parkId":4534,"reference":"US-4534","name":"Bighorn","latitude":44.804,"longitude":-106.927,
     "grid4":"DN64","grid6":"DN64mt","parktypeId":25,"active":1,
     "parkComments":"See the website for seasonal restrictions.","parktypeDesc":"National Forest",
     "locationDesc":"US-WY","locationName":"Wyoming","entityName":"United States of America",
     "website":"https://www.fs.usda.gov/bighorn","firstActivator":"AE7AP"}
    """;

    [Fact]
    public void ParseSpots_reads_core_fields_and_skips_malformed()
    {
        var spots = PotaJson.ParseSpots(SpotsJson);

        // Four in the payload, but the last has no frequency and is skipped.
        Assert.Equal(3, spots.Count);
        var first = spots[0];
        Assert.Equal("WY7BW", first.Activator);
        Assert.Equal(14295.0, first.FrequencyKhz);
        Assert.Equal("US-4534", first.Reference);
        Assert.Equal("Bighorn National Forest", first.ParkName);
        Assert.Equal("DN64mt", first.Grid);
    }

    [Fact]
    public void ParseSpots_maps_frequency_to_band_in_mhz()
    {
        var spots = PotaJson.ParseSpots(SpotsJson);
        Assert.Equal(14.295, spots[0].FrequencyMhz, precision: 3);
        Assert.Equal(HamBand.M20, spots[0].Band);
        Assert.Equal(HamBand.M40, spots[1].Band);
    }

    [Fact]
    public void ParseSpots_reports_no_band_for_vhf()
    {
        var vhf = PotaJson.ParseSpots(SpotsJson).Single(s => s.Activator == "N0VHF");
        Assert.Null(vhf.Band);
    }

    [Fact]
    public void ParseSpots_detects_self_spots()
    {
        var spots = PotaJson.ParseSpots(SpotsJson);
        Assert.False(spots.Single(s => s.Activator == "WY7BW").IsSelfSpot); // spotter KQ4WFX
        Assert.True(spots.Single(s => s.Activator == "K4ABC").IsSelfSpot);  // spotter K4ABC
    }

    [Fact]
    public void ParseSpots_parses_spot_time_as_utc()
    {
        var first = PotaJson.ParseSpots(SpotsJson)[0];
        Assert.NotNull(first.SpotTimeUtc);
        Assert.Equal(DateTimeKind.Utc, first.SpotTimeUtc!.Value.Kind);
    }

    [Fact]
    public void ParseSpots_throws_on_non_array()
    {
        Assert.Throws<PotaFormatException>(() => PotaJson.ParseSpots("{\"not\":\"an array\"}"));
    }

    [Fact]
    public void ParsePark_reads_fields()
    {
        var park = PotaJson.ParsePark(ParkJson);
        Assert.Equal(4534, park.ParkId);
        Assert.Equal("US-4534", park.Reference);
        Assert.Equal("Bighorn", park.Name);
        Assert.Equal("National Forest", park.ParkType);
        Assert.Equal("Wyoming", park.LocationName);
        Assert.True(park.Active);
        Assert.Equal("https://www.fs.usda.gov/bighorn", park.Website);
    }

    [Fact]
    public void ParsePark_throws_on_missing_required_fields()
    {
        Assert.Throws<PotaFormatException>(() => PotaJson.ParsePark("{\"latitude\":1}"));
    }
}

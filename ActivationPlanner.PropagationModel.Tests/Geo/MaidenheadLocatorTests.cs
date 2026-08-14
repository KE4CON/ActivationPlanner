using ActivationPlanner.PropagationModel.Geo;

namespace ActivationPlanner.PropagationModel.Tests.Geo;

public sealed class MaidenheadLocatorTests
{
    [Fact]
    public void Encodes_newington_ct_to_fn31()
    {
        // ARRL HQ, Newington CT (~41.714, -72.727) is famously FN31.
        string grid = MaidenheadLocator.ToGrid(new GeoLocation(41.714, -72.727));
        Assert.StartsWith("FN31", grid);
        Assert.Equal(6, grid.Length);
    }

    [Fact]
    public void Parses_a_4char_grid_to_its_center()
    {
        Assert.True(MaidenheadLocator.TryParse("EM29", out GeoLocation loc));
        // EM29 spans 96-94W, 39-40N; center is 95W, 39.5N.
        Assert.Equal(39.5, loc.LatitudeDeg, 3);
        Assert.Equal(-95.0, loc.LongitudeDeg, 3);
    }

    [Fact]
    public void Round_trips_within_grid_resolution()
    {
        var start = new GeoLocation(39.83, -98.58);
        string grid = MaidenheadLocator.ToGrid(start);
        Assert.True(MaidenheadLocator.TryParse(grid, out GeoLocation back));

        // A 6-char grid is ~5' lon x 2.5' lat, so the center is within ~0.06 deg of the start.
        Assert.True(System.Math.Abs(start.LatitudeDeg - back.LatitudeDeg) < 0.06);
        Assert.True(System.Math.Abs(start.LongitudeDeg - back.LongitudeDeg) < 0.06);
    }

    [Fact]
    public void Parse_is_case_insensitive()
    {
        Assert.True(MaidenheadLocator.TryParse("em29OK", out GeoLocation a));
        Assert.True(MaidenheadLocator.TryParse("EM29ok", out GeoLocation b));
        Assert.Equal(a.LatitudeDeg, b.LatitudeDeg, 6);
        Assert.Equal(a.LongitudeDeg, b.LongitudeDeg, 6);
    }

    [Theory]
    [InlineData("ZZ99")]   // field letters out of A-R range
    [InlineData("EM2")]    // too short
    [InlineData("hello")]  // not a grid
    [InlineData("")]       // empty
    [InlineData("EMXY")]   // squares must be digits
    public void Rejects_invalid_grids(string bad)
    {
        Assert.False(MaidenheadLocator.TryParse(bad, out _));
    }
}

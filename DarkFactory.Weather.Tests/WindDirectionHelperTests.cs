using DarkFactory.Weather.Services;

namespace DarkFactory.Weather.Tests;

public class WindDirectionHelperTests
{
    [Theory]
    [InlineData(0.0, "N")]
    [InlineData(45.0, "NE")]
    [InlineData(90.0, "E")]
    [InlineData(135.0, "SE")]
    [InlineData(180.0, "S")]
    [InlineData(225.0, "SW")]
    [InlineData(270.0, "W")]
    [InlineData(315.0, "NW")]
    public void DegreesToCompass_CorrectlyMapsAllOctants(double degrees, string expected)
    {
        var result = WindDirectionHelper.DegreesToCompass(degrees);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void DegreesToCompass_360DegreesReturnsNorth()
    {
        var result = WindDirectionHelper.DegreesToCompass(360.0);

        Assert.Equal("N", result);
    }

    [Fact]
    public void DegreesToCompass_MidpointRoundsToNE()
    {
        // 22.5 degrees → rounds to index 1 → NE
        var result = WindDirectionHelper.DegreesToCompass(22.5);

        Assert.Equal("NE", result);
    }

    [Fact]
    public void RandomCompass_ReturnsValidCompassPoint()
    {
        var validPoints = new[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

        for (int i = 0; i < 50; i++)
        {
            var result = WindDirectionHelper.RandomCompass();
            Assert.Contains(result, validPoints);
        }
    }
}

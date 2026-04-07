using DarkFactory.Weather.Models;
using DarkFactory.Weather.Services;

namespace DarkFactory.Weather.Tests;

public class WeatherServiceTests
{
    private readonly WeatherService _sut = new();

    [Theory]
    [InlineData("tropical")]
    [InlineData("arid")]
    [InlineData("temperate")]
    [InlineData("continental")]
    [InlineData("polar")]
    [InlineData("unknown")]
    public void GetForecast_ReturnsFiveDays(string region)
    {
        var result = _sut.GetForecast(region).ToList();

        Assert.Equal(5, result.Count);
    }

    [Theory]
    [InlineData("tropical")]
    [InlineData("TROPICAL")]
    [InlineData("Tropical")]
    public void GetForecast_IsCaseInsensitive(string region)
    {
        var result = _sut.GetForecast(region).ToList();

        Assert.All(result, f => Assert.Equal(region, f.Region));
    }

    [Fact]
    public void GetForecast_ForecastDatesAreConsecutiveFutureDays()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = _sut.GetForecast("temperate").ToList();

        for (int i = 0; i < result.Count; i++)
        {
            Assert.Equal(today.AddDays(i + 1), result[i].Date);
        }
    }

    [Fact]
    public void GetForecast_TemperatureFIsDerivedFromTemperatureC()
    {
        var result = _sut.GetForecast("temperate").First();

        int expected = 32 + (int)(result.TemperatureC * 9.0 / 5);
        Assert.Equal(expected, result.TemperatureF);
    }

    [Fact]
    public void GetForecast_SummaryIsNeverNull()
    {
        var result = _sut.GetForecast("temperate").ToList();

        Assert.All(result, f => Assert.NotNull(f.Summary));
    }

    [Fact]
    public void GetForecast_HumidityIsInValidRange()
    {
        var result = _sut.GetForecast("temperate").ToList();

        Assert.All(result, f => Assert.InRange(f.Humidity, 0, 100));
    }

    [Fact]
    public void GetForecast_WindSpeedIsNonNegative()
    {
        var result = _sut.GetForecast("temperate").ToList();

        Assert.All(result, f => Assert.InRange(f.WindSpeed, 0, 120));
    }

    [Fact]
    public void GetForecast_EachForecastDayHasValidWindDirection()
    {
        var validDirections = new[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };

        var result = _sut.GetForecast("temperate").ToList();

        Assert.All(result, f => Assert.Contains(f.WindDirection, validDirections));
    }

    [Fact]
    public void GetForecast_WindDirectionIsNonNullString()
    {
        var result = _sut.GetForecast("temperate").ToList();

        Assert.All(result, f => Assert.False(string.IsNullOrEmpty(f.WindDirection)));
    }
}

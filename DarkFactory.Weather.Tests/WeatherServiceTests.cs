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
}

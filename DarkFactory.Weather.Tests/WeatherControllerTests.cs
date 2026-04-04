using DarkFactory.Weather.Controllers;
using DarkFactory.Weather.Dtos;
using DarkFactory.Weather.Models;
using DarkFactory.Weather.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DarkFactory.Weather.Tests;

public class WeatherControllerTests
{
    private readonly Mock<IWeatherService> _serviceMock = new();
    private readonly WeatherController _sut;

    public WeatherControllerTests()
    {
        _sut = new WeatherController(_serviceMock.Object);
    }

    [Fact]
    public void GetWeather_ValidRegion_ReturnsOkWithDtos()
    {
        var forecasts = new[]
        {
            new WeatherForecast(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), 20, "Warm", "temperate", 60, 15.5),
        };
        _serviceMock.Setup(s => s.GetForecast("temperate")).Returns(forecasts);

        var result = _sut.GetWeather("temperate");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<WeatherForecastDto>>(ok.Value).ToList();
        Assert.Single(dtos);
        var dto = dtos[0];
        Assert.Equal(forecasts[0].Date, dto.Date);
        Assert.Equal(forecasts[0].TemperatureC, dto.TemperatureC);
        Assert.Equal(forecasts[0].TemperatureF, dto.TemperatureF);
        Assert.Equal(forecasts[0].Summary, dto.Summary);
        Assert.Equal(forecasts[0].Humidity, dto.Humidity);
        Assert.Equal(forecasts[0].WindSpeed, dto.WindSpeed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GetWeather_EmptyOrWhitespaceRegion_ReturnsBadRequest(string region)
    {
        var result = _sut.GetWeather(region);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void GetWeather_DelegatesToService()
    {
        _serviceMock.Setup(s => s.GetForecast("polar")).Returns([]);

        _sut.GetWeather("polar");

        _serviceMock.Verify(s => s.GetForecast("polar"), Times.Once);
    }
}

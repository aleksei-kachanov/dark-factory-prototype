using System.Collections.Generic;
using System.Threading.Tasks;
using DarkFactory.Weather.Controllers;
using DarkFactory.Weather.Dtos;
using DarkFactory.Weather.Models;
using DarkFactory.Weather.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DarkFactory.Weather.Tests;

public class WeatherControllerTests
{
    private static readonly IReadOnlyList<string> ValidRegions =
        new[] { "tropical", "arid", "temperate", "continental", "polar" };

    private static WeatherForecastDto MakeDto(string date = "2026-04-07") =>
        new(date, 22, 72, "Partly cloudy", 65, 8, "SE");

    private static WeatherController BuildController(
        Mock<IWeatherService> weatherMock,
        Mock<IAustinWeatherService>? austinMock = null,
        Mock<ICityWeatherService>? cityMock = null)
    {
        return new WeatherController(
            weatherMock.Object,
            (austinMock ?? new Mock<IAustinWeatherService>()).Object,
            (cityMock ?? new Mock<ICityWeatherService>()).Object);
    }

    [Theory]
    [InlineData("tropical")]
    [InlineData("arid")]
    [InlineData("temperate")]
    [InlineData("continental")]
    [InlineData("polar")]
    public void GetRegionForecast_ValidRegion_Returns200(string region)
    {
        var forecasts = new List<WeatherForecastDto> { MakeDto(), MakeDto(), MakeDto(), MakeDto(), MakeDto() };
        var mockWeather = new Mock<IWeatherService>();
        mockWeather.Setup(s => s.GetForecast(region)).Returns(forecasts);

        var controller = BuildController(mockWeather);
        var result = controller.GetRegionForecast(region);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<WeatherForecastDto>>(ok.Value);
        Assert.Equal(5, dtos.Count());
    }

    [Fact]
    public void GetRegionForecast_InvalidRegion_Returns404()
    {
        var mockWeather = new Mock<IWeatherService>();
        mockWeather.Setup(s => s.GetForecast("atlantis")).Throws<ArgumentException>();

        var controller = BuildController(mockWeather);
        var result = controller.GetRegionForecast("atlantis");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetAustinForecast_Returns200WithForecasts()
    {
        var forecasts = new List<WeatherForecastDto> { MakeDto(), MakeDto(), MakeDto(), MakeDto(), MakeDto() };
        var mockAustin = new Mock<IAustinWeatherService>();
        mockAustin.Setup(s => s.GetForecastAsync()).ReturnsAsync(forecasts);

        var mockWeather = new Mock<IWeatherService>();
        var controller = BuildController(mockWeather, mockAustin);

        var result = await controller.GetAustinForecast();

        var ok = Assert.IsType<OkObjectResult>(result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<WeatherForecastDto>>(ok.Value);
        Assert.Equal(5, dtos.Count());
    }
}

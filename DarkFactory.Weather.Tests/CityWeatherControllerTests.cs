using System.Collections.Generic;
using System.Threading.Tasks;
using DarkFactory.Weather.Controllers;
using DarkFactory.Weather.Dtos;
using DarkFactory.Weather.Models;
using DarkFactory.Weather.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DarkFactory.Weather.Tests;

public class CityWeatherControllerTests
{
    private static WeatherForecastDto MakeDto(string date = "2026-04-07") =>
        new(date, 25, 77, "Sunny", 60, 10, "N");

    [Fact]
    public async Task GetCityForecast_KnownSlug_Returns200WithForecasts()
    {
        var mockCityService = new Mock<ICityWeatherService>();
        mockCityService
            .Setup(s => s.GetForecastAsync("dallas"))
            .ReturnsAsync(new List<WeatherForecastDto> { MakeDto(), MakeDto(), MakeDto(), MakeDto(), MakeDto() });

        var mockWeatherService = new Mock<IWeatherService>();
        var mockAustinService = new Mock<IAustinWeatherService>();
        var controller = new WeatherController(mockWeatherService.Object, mockAustinService.Object, mockCityService.Object);

        var result = await controller.GetCityForecast("dallas");

        var ok = Assert.IsType<OkObjectResult>(result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<WeatherForecastDto>>(ok.Value);
        Assert.Equal(5, dtos.Count());
    }

    [Fact]
    public async Task GetCityForecast_UnknownSlug_Returns404()
    {
        var mockCityService = new Mock<ICityWeatherService>();
        mockCityService
            .Setup(s => s.GetForecastAsync("atlantis"))
            .ReturnsAsync((IEnumerable<WeatherForecastDto>?)null);

        var mockWeatherService = new Mock<IWeatherService>();
        var mockAustinService = new Mock<IAustinWeatherService>();
        var controller = new WeatherController(mockWeatherService.Object, mockAustinService.Object, mockCityService.Object);

        var result = await controller.GetCityForecast("atlantis");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void GetSupportedCities_ReturnsListFromService()
    {
        var cities = new List<TexasCityConfig>
        {
            new() { Slug = "austin",      Name = "Austin",      Latitude = 30.2672, Longitude = -97.7431 },
            new() { Slug = "dallas",      Name = "Dallas",       Latitude = 32.7767, Longitude = -96.7970 },
            new() { Slug = "houston",     Name = "Houston",      Latitude = 29.7604, Longitude = -95.3698 },
            new() { Slug = "san-antonio", Name = "San Antonio",  Latitude = 29.4241, Longitude = -98.4936 },
            new() { Slug = "fort-worth",  Name = "Fort Worth",   Latitude = 32.7555, Longitude = -97.3308 }
        };
        var mockCityService = new Mock<ICityWeatherService>();
        mockCityService.Setup(s => s.GetSupportedCities()).Returns(cities);

        var mockWeatherService = new Mock<IWeatherService>();
        var mockAustinService = new Mock<IAustinWeatherService>();
        var controller = new WeatherController(mockWeatherService.Object, mockAustinService.Object, mockCityService.Object);

        var result = controller.GetSupportedCities();

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsAssignableFrom<IReadOnlyList<TexasCityConfig>>(ok.Value);
        Assert.Equal(5, returned.Count);
    }
}

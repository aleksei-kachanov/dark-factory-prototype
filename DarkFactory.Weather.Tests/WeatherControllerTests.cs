using DarkFactory.Weather.Controllers;
using DarkFactory.Weather.Dtos;
using DarkFactory.Weather.Models;
using DarkFactory.Weather.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DarkFactory.Weather.Tests;

public class WeatherControllerTests
{
    private readonly Mock<IWeatherService> _serviceMock = new();
    private readonly Mock<IAustinWeatherService> _austinServiceMock = new();
    private readonly WeatherController _sut;

    public WeatherControllerTests()
    {
        _sut = new WeatherController(_serviceMock.Object, _austinServiceMock.Object);
    }

    [Fact]
    public async Task GetWeather_ValidRegion_ReturnsOkWithDtos()
    {
        var forecasts = new[]
        {
            new WeatherForecast(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), 20, "Warm", "temperate", 60, 15.5, "N"),
        };
        _serviceMock.Setup(s => s.GetForecast("temperate")).Returns(forecasts);

        var result = await _sut.GetWeather("temperate", CancellationToken.None);

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
        Assert.Equal("N", dto.WindDirection);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetWeather_EmptyOrWhitespaceRegion_ReturnsBadRequest(string region)
    {
        var result = await _sut.GetWeather(region, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetWeather_DelegatesToService()
    {
        _serviceMock.Setup(s => s.GetForecast("polar")).Returns([]);

        await _sut.GetWeather("polar", CancellationToken.None);

        _serviceMock.Verify(s => s.GetForecast("polar"), Times.Once);
    }

    [Fact]
    public async Task GetWeather_ValidRegion_WindDirectionMappedToDto()
    {
        var forecasts = new[]
        {
            new WeatherForecast(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), 25, "Warm", "tropical", 70, 20.0, "NE"),
        };
        _serviceMock.Setup(s => s.GetForecast("tropical")).Returns(forecasts);

        var result = await _sut.GetWeather("tropical", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<WeatherForecastDto>>(ok.Value).ToList();
        Assert.Equal("NE", dtos[0].WindDirection);
    }

    [Fact]
    public async Task GetWeather_AustinRegion_RoutesToAustinService()
    {
        var austinForecasts = new[]
        {
            new WeatherForecast(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), 28, "Warm", "austin", 55, 18.0, "S"),
        };
        _austinServiceMock
            .Setup(s => s.GetForecastAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(austinForecasts);

        var result = await _sut.GetWeather("austin", CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
        _austinServiceMock.Verify(s => s.GetForecastAsync(It.IsAny<CancellationToken>()), Times.Once);
        _serviceMock.Verify(s => s.GetForecast(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetWeather_AustinRegion_CaseInsensitive_RoutesToAustinService()
    {
        _austinServiceMock
            .Setup(s => s.GetForecastAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        await _sut.GetWeather("AUSTIN", CancellationToken.None);

        _austinServiceMock.Verify(s => s.GetForecastAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetWeather_AustinRegion_ServiceThrows_Returns502()
    {
        _austinServiceMock
            .Setup(s => s.GetForecastAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var result = await _sut.GetWeather("austin", CancellationToken.None);

        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status502BadGateway, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetWeather_SimulatedRegionThrows_DoesNotReturn502()
    {
        _serviceMock
            .Setup(s => s.GetForecast("temperate"))
            .Throws(new InvalidOperationException("unexpected"));

        // Simulated path has no catch — exception propagates (500)
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GetWeather("temperate", CancellationToken.None));
    }
}

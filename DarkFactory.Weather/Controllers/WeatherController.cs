using DarkFactory.Weather.Dtos;
using DarkFactory.Weather.Models;
using DarkFactory.Weather.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarkFactory.Weather.Controllers;

[ApiController]
[Route("weather")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly IAustinWeatherService _austinWeatherService;
    private readonly ICityWeatherService _cityWeatherService;

    public WeatherController(
        IWeatherService weatherService,
        IAustinWeatherService austinWeatherService,
        ICityWeatherService cityWeatherService)
    {
        _weatherService = weatherService;
        _austinWeatherService = austinWeatherService;
        _cityWeatherService = cityWeatherService;
    }

    [HttpGet("{region}")]
    public IActionResult GetRegionForecast([FromRoute] string region)
    {
        try
        {
            var forecast = _weatherService.GetForecast(region);
            return Ok(forecast.Select(f => new WeatherForecastDto(f.Date, f.TemperatureC, f.TemperatureF, f.Summary, f.Humidity, f.WindSpeed, f.WindDirection)));
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpGet("austin")]
    public async Task<IActionResult> GetAustinForecast()
    {
        var forecast = await _austinWeatherService.GetForecastAsync();
        return Ok(forecast.Select(f => new WeatherForecastDto(f.Date, f.TemperatureC, f.TemperatureF, f.Summary, f.Humidity, f.WindSpeed, f.WindDirection)));
    }

    [HttpGet("city/{citySlug}")]
    public async Task<IActionResult> GetCityForecast([FromRoute] string citySlug)
    {
        try
        {
            var forecast = await _cityWeatherService.GetForecastAsync(citySlug);
            if (forecast is null) return NotFound();
            return Ok(forecast);
        }
        catch (HttpRequestException ex)
        {
            return Problem(
                title: "Upstream weather service unavailable",
                detail: ex.Message,
                statusCode: 502);
        }
    }

    [HttpGet("cities")]
    public IActionResult GetSupportedCities()
    {
        var cities = _cityWeatherService.GetSupportedCities();
        return Ok(cities);
    }
}

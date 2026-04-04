using DarkFactory.Weather.Dtos;
using DarkFactory.Weather.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarkFactory.Weather.Controllers;

[ApiController]
[Route("api/v1/weather")]
public class WeatherController(IWeatherService weatherService) : ControllerBase
{
    private readonly IWeatherService _weatherService = weatherService;

    /// <summary>Returns a 5-day weather forecast for the given region.</summary>
    /// <param name="region">
    /// The climate region to forecast. Supported values: tropical, arid,
    /// temperate, continental, polar. Defaults to "temperate".
    /// </param>
    [HttpGet("{region}")]
    [ProducesResponseType(typeof(IEnumerable<WeatherForecastDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<IEnumerable<WeatherForecastDto>> GetWeather(string region)
    {
        if (string.IsNullOrWhiteSpace(region))
        {
            return BadRequest("Region must not be empty.");
        }

        var forecast = _weatherService.GetForecast(region)
            .Select(f => new WeatherForecastDto(f.Date, f.TemperatureC, f.TemperatureF, f.Summary, f.Humidity, f.WindSpeed));
        return Ok(forecast);
    }
}

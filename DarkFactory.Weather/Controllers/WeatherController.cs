using Asp.Versioning;
using DarkFactory.Weather.Dtos;
using DarkFactory.Weather.Services;
using Microsoft.AspNetCore.Mvc;

namespace DarkFactory.Weather.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/weather")]
public class WeatherController(IWeatherService weatherService, IAustinWeatherService austinWeatherService) : ControllerBase
{
    private readonly IWeatherService _weatherService = weatherService;
    private readonly IAustinWeatherService _austinWeatherService = austinWeatherService;

    /// <summary>Returns a 5-day weather forecast for the given region or Austin, TX.</summary>
    /// <param name="region">
    /// The climate region or location. Supported values: tropical, arid,
    /// temperate, continental, polar, austin.
    /// </param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{region}")]
    [ProducesResponseType(typeof(IEnumerable<WeatherForecastDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<IEnumerable<WeatherForecastDto>>> GetWeather(
        string region, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(region))
        {
            return BadRequest("Region must not be empty.");
        }

        if (region.Equals("austin", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var austinForecasts = await _austinWeatherService.GetForecastAsync(cancellationToken);
                return Ok(austinForecasts.Select(f => new WeatherForecastDto(
                    f.Date, f.TemperatureC, f.TemperatureF, f.Summary, f.Humidity, f.WindSpeed, f.WindDirection)));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    $"Upstream weather service unavailable: {ex.Message}");
            }
        }

        var forecast = _weatherService.GetForecast(region)
            .Select(f => new WeatherForecastDto(f.Date, f.TemperatureC, f.TemperatureF, f.Summary, f.Humidity, f.WindSpeed, f.WindDirection));
        return Ok(forecast);
    }
}

using DarkFactory.Weather.Models;

namespace DarkFactory.Weather.Services;

public interface IAustinWeatherService
{
    Task<IEnumerable<WeatherForecast>> GetForecastAsync(CancellationToken cancellationToken = default);
}

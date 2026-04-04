using DarkFactory.Weather.Models;

namespace DarkFactory.Weather.Services;

public interface IWeatherService
{
    IEnumerable<WeatherForecast> GetForecast(string region);
}

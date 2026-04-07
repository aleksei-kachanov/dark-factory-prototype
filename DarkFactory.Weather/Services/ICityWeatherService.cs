using DarkFactory.Weather.Dtos;
using DarkFactory.Weather.Models;

namespace DarkFactory.Weather.Services;

public interface ICityWeatherService
{
    Task<IEnumerable<WeatherForecastDto>?> GetForecastAsync(string citySlug);
    IReadOnlyList<TexasCityConfig> GetSupportedCities();
}

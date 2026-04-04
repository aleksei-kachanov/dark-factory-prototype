using DarkFactory.Weather.Models;

namespace DarkFactory.Weather.Services;

public class WeatherService : IWeatherService
{
    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    // Region-based temperature offsets (in Celsius)
    private static readonly Dictionary<string, int> RegionOffsets = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tropical"]   =  15,
        ["arid"]       =  20,
        ["temperate"]  =   0,
        ["continental"] = -5,
        ["polar"]      = -25,
    };

    public IEnumerable<WeatherForecast> GetForecast(string region)
    {
        int offset = RegionOffsets.TryGetValue(region, out int value) ? value : 0;

        return Enumerable.Range(1, 5).Select(index =>
        {
            int baseTemp = Random.Shared.Next(-20, 35);
            int temp = Math.Clamp(baseTemp + offset, -60, 60);
            int humidity = Random.Shared.Next(0, 101);
            double windSpeed = Math.Round(Random.Shared.NextDouble() * 120, 1);
            return new WeatherForecast(
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(index)),
                temp,
                Summaries[Random.Shared.Next(Summaries.Length)],
                region,
                humidity,
                windSpeed);
        });
    }
}

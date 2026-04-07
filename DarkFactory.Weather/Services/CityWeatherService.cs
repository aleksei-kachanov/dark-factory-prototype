using System.Text.Json;
using System.Text.Json.Serialization;
using DarkFactory.Weather.Dtos;
using DarkFactory.Weather.Models;
using Microsoft.Extensions.Options;

namespace DarkFactory.Weather.Services;

public sealed class CityWeatherService : ICityWeatherService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IReadOnlyList<TexasCityConfig> _cities;

    public CityWeatherService(
        IHttpClientFactory httpClientFactory,
        IOptions<List<TexasCityConfig>> options)
    {
        _httpClientFactory = httpClientFactory;
        _cities = options.Value.AsReadOnly();
    }

    public IReadOnlyList<TexasCityConfig> GetSupportedCities() => _cities;

    public async Task<IEnumerable<WeatherForecastDto>?> GetForecastAsync(string citySlug)
    {
        var city = _cities.FirstOrDefault(
            c => c.Slug.Equals(citySlug, StringComparison.OrdinalIgnoreCase));

        if (city is null) return null;

        var url = BuildUrl(city.Latitude, city.Longitude);
        var client = _httpClientFactory.CreateClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var data = JsonSerializer.Deserialize<OpenMeteoResponse>(json, options);

        if (data?.Daily is null) return Enumerable.Empty<WeatherForecastDto>();

        return MapToForecast(data);
    }

    private static string BuildUrl(double lat, double lon) =>
        $"https://api.open-meteo.com/v1/forecast" +
        $"?latitude={lat:F4}&longitude={lon:F4}" +
        "&daily=weathercode,temperature_2m_max,temperature_2m_min,windspeed_10m_max,winddirection_10m_dominant,precipitation_sum" +
        "&hourly=relativehumidity_2m" +
        "&timezone=auto&forecast_days=5";

    private static IEnumerable<WeatherForecastDto> MapToForecast(OpenMeteoResponse data)
    {
        var d = data.Daily!;
        var hourlyHumidity = data.Hourly?.Relativehumidity2m ?? Array.Empty<int>();

        for (int i = 0; i < d.Time.Length; i++)
        {
            double maxTemp = d.Temperature2mMax?[i] ?? 0;
            double minTemp = d.Temperature2mMin?[i] ?? 0;
            int tempC = (int)Math.Round((maxTemp + minTemp) / 2.0);
            int tempF = (int)Math.Round(tempC * 9.0 / 5 + 32);
            int humidity = hourlyHumidity.Length > i * 24 + 11
                ? hourlyHumidity[i * 24 + 11]
                : 0;
            int windSpeed = (int)Math.Round(d.WindspeedMax?[i] ?? 0);
            string windDir = WindDirectionHelper.GetCompassDirection(d.WinddirectionDominant?[i] ?? 0);
            string? summary = GetSummary(d.Weathercode?[i] ?? 0);

            yield return new WeatherForecastDto(
                d.Time[i],
                tempC,
                tempF,
                summary,
                humidity,
                windSpeed,
                windDir);
        }
    }

    private static string? GetSummary(int code) => code switch
    {
        0 => "Clear sky",
        1 => "Mainly clear",
        2 => "Partly cloudy",
        3 => "Overcast",
        45 or 48 => "Fog",
        51 or 53 or 55 => "Drizzle",
        61 or 63 or 65 => "Rain",
        71 or 73 or 75 => "Snow",
        77 => "Snow grains",
        80 or 81 or 82 => "Rain showers",
        85 or 86 => "Snow showers",
        95 => "Thunderstorm",
        96 or 99 => "Thunderstorm with hail",
        _ => "Unknown"
    };

    private sealed record OpenMeteoResponse(
        [property: JsonPropertyName("daily")] OpenMeteoDailyData? Daily,
        [property: JsonPropertyName("hourly")] OpenMeteoHourlyData? Hourly);

    private sealed record OpenMeteoDailyData(
        [property: JsonPropertyName("time")]                         string[]  Time,
        [property: JsonPropertyName("weathercode")]                  int[]?    Weathercode,
        [property: JsonPropertyName("temperature_2m_max")]           double[]? Temperature2mMax,
        [property: JsonPropertyName("temperature_2m_min")]           double[]? Temperature2mMin,
        [property: JsonPropertyName("windspeed_10m_max")]            double[]? WindspeedMax,
        [property: JsonPropertyName("winddirection_10m_dominant")]   double[]? WinddirectionDominant,
        [property: JsonPropertyName("precipitation_sum")]            double[]? PrecipitationSum);

    private sealed record OpenMeteoHourlyData(
        [property: JsonPropertyName("relativehumidity_2m")] int[] Relativehumidity2m);
}

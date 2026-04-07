using System.Net.Http.Json;
using DarkFactory.Weather.Models;

namespace DarkFactory.Weather.Services;

public class AustinWeatherService(IHttpClientFactory httpClientFactory) : IAustinWeatherService
{
    private const string OpenMeteoUrl =
        "v1/forecast?latitude=30.2672&longitude=-97.7431" +
        "&daily=temperature_2m_max,temperature_2m_min,windspeed_10m_max,winddirection_10m_dominant,weathercode" +
        "&hourly=relativehumidity_2m" +
        "&forecast_days=5" +
        "&timezone=America%2FChicago";

    public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("OpenMeteo");
        var response = await client.GetAsync(OpenMeteoUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<OpenMeteoResponse>(cancellationToken)
                   ?? throw new InvalidOperationException("Open-Meteo returned null response.");

        return Enumerable.Range(0, data.Daily.Time.Length).Select(i =>
        {
            var date = DateOnly.Parse(data.Daily.Time[i]);
            var tempC = (int)Math.Round((data.Daily.Temperature2mMax[i] + data.Daily.Temperature2mMin[i]) / 2.0);
            var summary = WeatherSummaryMapper.FromWeatherCode(data.Daily.Weathercode[i]);
            var humidity = data.Hourly.RelativeHumidity2m[i * 24 + 12];
            var windSpeed = Math.Round(data.Daily.Windspeed10mMax[i], 1);
            var windDirection = WindDirectionHelper.DegreesToCompass(data.Daily.Winddirection10mDominant[i]);

            return new WeatherForecast(date, tempC, summary, "austin", humidity, windSpeed, windDirection);
        }).ToList();
    }

    private static class WeatherSummaryMapper
    {
        public static string FromWeatherCode(int code) => code switch
        {
            0 => "Balmy",
            1 or 2 => "Warm",
            3 => "Mild",
            45 or 48 => "Bracing",
            51 or 53 or 55 => "Mild",
            56 or 57 => "Chilly",
            61 or 63 => "Chilly",
            65 => "Bracing",
            66 or 67 => "Freezing",
            71 or 73 or 75 or 77 => "Freezing",
            80 or 81 or 82 => "Cool",
            85 or 86 => "Freezing",
            95 => "Sweltering",
            96 or 99 => "Scorching",
            _ => "Mild",
        };
    }
}

using System.Text.Json.Serialization;

namespace DarkFactory.Weather.Models;

internal sealed record OpenMeteoHourlyData(
    [property: JsonPropertyName("relativehumidity_2m")] int[] RelativeHumidity2m);

internal sealed record OpenMeteoDailyData(
    [property: JsonPropertyName("time")] string[] Time,
    [property: JsonPropertyName("temperature_2m_max")] double[] Temperature2mMax,
    [property: JsonPropertyName("temperature_2m_min")] double[] Temperature2mMin,
    [property: JsonPropertyName("windspeed_10m_max")] double[] Windspeed10mMax,
    [property: JsonPropertyName("winddirection_10m_dominant")] double[] Winddirection10mDominant,
    [property: JsonPropertyName("weathercode")] int[] Weathercode);

internal sealed record OpenMeteoResponse(
    [property: JsonPropertyName("daily")] OpenMeteoDailyData Daily,
    [property: JsonPropertyName("hourly")] OpenMeteoHourlyData Hourly);

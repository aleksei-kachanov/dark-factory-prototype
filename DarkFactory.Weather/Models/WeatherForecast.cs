namespace DarkFactory.Weather.Models;

public record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary,
    string Region,
    int Humidity,
    double WindSpeed)
{
    public int TemperatureF => 32 + (int)(TemperatureC * 9.0 / 5);
}

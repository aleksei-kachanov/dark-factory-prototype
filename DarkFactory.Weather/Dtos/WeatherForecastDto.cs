namespace DarkFactory.Weather.Dtos;

public record WeatherForecastDto(
    DateOnly Date,
    int TemperatureC,
    int TemperatureF,
    string? Summary,
    int Humidity,
    double WindSpeed,
    string WindDirection);

namespace DarkFactory.Weather.Models;

public sealed class TexasCityConfig
{
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}

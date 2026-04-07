namespace DarkFactory.Weather.Services;

internal static class WindDirectionHelper
{
    private static readonly string[] CompassPoints = ["N", "NE", "E", "SE", "S", "SW", "W", "NW"];

    public static string DegreesToCompass(double degrees)
    {
        int index = (int)Math.Round(degrees / 45.0, MidpointRounding.AwayFromZero) % 8;
        return CompassPoints[index];
    }

    public static string RandomCompass() =>
        CompassPoints[Random.Shared.Next(CompassPoints.Length)];
}

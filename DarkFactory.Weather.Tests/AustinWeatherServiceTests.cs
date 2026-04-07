using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DarkFactory.Weather.Services;
using Moq;
using Moq.Protected;

namespace DarkFactory.Weather.Tests;

public class AustinWeatherServiceTests
{
    private static HttpClient BuildHttpClient(string jsonResponse, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json"),
            });

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.open-meteo.com/"),
        };
        return client;
    }

    private static IHttpClientFactory BuildFactory(HttpClient client)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("OpenMeteo")).Returns(client);
        return factory.Object;
    }

    private static string BuildValidResponse(int days = 5)
    {
        // Build a minimal valid Open-Meteo response
        var times = Enumerable.Range(0, days)
            .Select(i => DateTime.UtcNow.AddDays(i + 1).ToString("yyyy-MM-dd"))
            .ToArray();
        var maxTemps = Enumerable.Repeat(30.0, days).ToArray();
        var minTemps = Enumerable.Repeat(20.0, days).ToArray();
        var windSpeeds = Enumerable.Repeat(15.0, days).ToArray();
        var windDirs = Enumerable.Repeat(180.0, days).ToArray();   // 180 degrees = South
        var weatherCodes = Enumerable.Repeat(0, days).ToArray();

        // hourly: 24 * days values; noon index for day i = i * 24 + 12
        var humidity = Enumerable.Repeat(65, days * 24).ToArray();

        return JsonSerializer.Serialize(new
        {
            daily = new
            {
                time = times,
                temperature_2m_max = maxTemps,
                temperature_2m_min = minTemps,
                windspeed_10m_max = windSpeeds,
                winddirection_10m_dominant = windDirs,
                weathercode = weatherCodes,
            },
            hourly = new
            {
                relativehumidity_2m = humidity,
            },
        });
    }

    [Fact]
    public async Task GetForecastAsync_ValidResponse_ReturnsExactlyFiveForecasts()
    {
        var sut = new AustinWeatherService(BuildFactory(BuildHttpClient(BuildValidResponse())));

        var result = (await sut.GetForecastAsync()).ToList();

        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task GetForecastAsync_ValidResponse_MapsTemperatureCorrectly()
    {
        // max=30, min=20 → average=25
        var sut = new AustinWeatherService(BuildFactory(BuildHttpClient(BuildValidResponse())));

        var result = (await sut.GetForecastAsync()).ToList();

        Assert.Equal(25, result[0].TemperatureC);
    }

    [Fact]
    public async Task GetForecastAsync_ValidResponse_MapsWindDirectionFromDegrees()
    {
        // 180 degrees = South
        var sut = new AustinWeatherService(BuildFactory(BuildHttpClient(BuildValidResponse())));

        var result = (await sut.GetForecastAsync()).ToList();

        Assert.Equal("S", result[0].WindDirection);
    }

    [Fact]
    public async Task GetForecastAsync_ValidResponse_MapsHumidityFromNoonHourly()
    {
        // hourly array: all 65; noon index for day 0 = 0 * 24 + 12 = 12
        var sut = new AustinWeatherService(BuildFactory(BuildHttpClient(BuildValidResponse())));

        var result = (await sut.GetForecastAsync()).ToList();

        Assert.Equal(65, result[0].Humidity);
    }

    [Fact]
    public async Task GetForecastAsync_ValidResponse_RegionIsAustin()
    {
        var sut = new AustinWeatherService(BuildFactory(BuildHttpClient(BuildValidResponse())));

        var result = (await sut.GetForecastAsync()).ToList();

        Assert.All(result, f => Assert.Equal("austin", f.Region));
    }

    [Fact]
    public async Task GetForecastAsync_HttpError_ThrowsHttpRequestException()
    {
        var sut = new AustinWeatherService(BuildFactory(BuildHttpClient("{}", HttpStatusCode.InternalServerError)));

        await Assert.ThrowsAsync<HttpRequestException>(() => sut.GetForecastAsync());
    }
}

using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using DarkFactory.Weather.Dtos;
using DarkFactory.Weather.Models;
using DarkFactory.Weather.Services;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace DarkFactory.Weather.Tests;

public class CityWeatherServiceTests
{
    private static IOptions<List<TexasCityConfig>> BuildOptions(IEnumerable<TexasCityConfig> cities)
    {
        var mock = new Mock<IOptions<List<TexasCityConfig>>>();
        mock.Setup(o => o.Value).Returns(new List<TexasCityConfig>(cities));
        return mock.Object;
    }

    private static IHttpClientFactory BuildFactory(HttpResponseMessage response)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("https://api.open-meteo.com")
        };

        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        return factory.Object;
    }

    private static string BuildOpenMeteoJson(int days = 5)
    {
        var times = Enumerable.Range(0, days).Select(i => DateTime.UtcNow.AddDays(i).ToString("yyyy-MM-dd")).ToArray();
        var hourlyTimes = Enumerable.Range(0, days * 24)
            .Select(i => DateTime.UtcNow.Date.AddHours(i).ToString("yyyy-MM-ddTHH:00")).ToArray();
        var humidity = Enumerable.Range(0, days * 24).Select(i => 60).ToArray();

        return JsonSerializer.Serialize(new
        {
            daily = new
            {
                time = times,
                weathercode = Enumerable.Range(0, days).Select(_ => 1).ToArray(),
                temperature_2m_max = Enumerable.Range(0, days).Select(i => 30.0 + i).ToArray(),
                temperature_2m_min = Enumerable.Range(0, days).Select(i => 20.0 + i).ToArray(),
                windspeed_10m_max = Enumerable.Range(0, days).Select(i => 10.0 + i).ToArray(),
                winddirection_10m_dominant = Enumerable.Range(0, days).Select(i => (double)(i * 45)).ToArray(),
                precipitation_sum = Enumerable.Range(0, days).Select(_ => 0.0).ToArray()
            },
            hourly = new
            {
                time = hourlyTimes,
                relativehumidity_2m = humidity
            }
        });
    }

    [Fact]
    public async Task GetForecastAsync_KnownCity_Returns5DayForecast()
    {
        var cities = new[]
        {
            new TexasCityConfig { Slug = "dallas", Name = "Dallas", Latitude = 32.7767, Longitude = -96.7970 }
        };
        var json = BuildOpenMeteoJson();
        var factory = BuildFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var svc = new CityWeatherService(factory, BuildOptions(cities));

        var result = await svc.GetForecastAsync("dallas");

        Assert.NotNull(result);
        Assert.Equal(5, result!.Count());
    }

    [Fact]
    public async Task GetForecastAsync_KnownCity_DtoFieldsPopulated()
    {
        var cities = new[]
        {
            new TexasCityConfig { Slug = "houston", Name = "Houston", Latitude = 29.7604, Longitude = -95.3698 }
        };
        var json = BuildOpenMeteoJson(5);
        var factory = BuildFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var svc = new CityWeatherService(factory, BuildOptions(cities));

        var result = (await svc.GetForecastAsync("houston"))!.ToList();

        var first = result[0];
        Assert.NotEqual(default(DateOnly), first.Date);
        Assert.True(first.TemperatureC > -273);
        Assert.True(first.TemperatureF > -459);
        Assert.InRange(first.Humidity, 0, 100);
        Assert.True(first.WindSpeed >= 0);
        Assert.False(string.IsNullOrWhiteSpace(first.WindDirection));
    }

    [Fact]
    public async Task GetForecastAsync_UnknownSlug_ReturnsNull()
    {
        var cities = new[]
        {
            new TexasCityConfig { Slug = "austin", Name = "Austin", Latitude = 30.2672, Longitude = -97.7431 }
        };
        var factory = BuildFactory(new HttpResponseMessage(HttpStatusCode.OK));
        var svc = new CityWeatherService(factory, BuildOptions(cities));

        var result = await svc.GetForecastAsync("unknown-city");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetForecastAsync_UpstreamFailure_ThrowsHttpRequestException()
    {
        var cities = new[]
        {
            new TexasCityConfig { Slug = "austin", Name = "Austin", Latitude = 30.2672, Longitude = -97.7431 }
        };
        var factory = BuildFactory(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var svc = new CityWeatherService(factory, BuildOptions(cities));

        await Assert.ThrowsAsync<HttpRequestException>(() => svc.GetForecastAsync("austin"));
    }

    [Fact]
    public void GetSupportedCities_ReturnsCitiesFromOptions()
    {
        var cities = new[]
        {
            new TexasCityConfig { Slug = "austin",     Name = "Austin",       Latitude = 30.2672, Longitude = -97.7431 },
            new TexasCityConfig { Slug = "dallas",     Name = "Dallas",        Latitude = 32.7767, Longitude = -96.7970 },
            new TexasCityConfig { Slug = "houston",    Name = "Houston",       Latitude = 29.7604, Longitude = -95.3698 },
            new TexasCityConfig { Slug = "san-antonio",Name = "San Antonio",   Latitude = 29.4241, Longitude = -98.4936 },
            new TexasCityConfig { Slug = "fort-worth", Name = "Fort Worth",    Latitude = 32.7555, Longitude = -97.3308 }
        };
        var factory = BuildFactory(new HttpResponseMessage(HttpStatusCode.OK));
        var svc = new CityWeatherService(factory, BuildOptions(cities));

        var result = svc.GetSupportedCities();

        Assert.Equal(5, result.Count);
        Assert.Contains(result, c => c.Slug == "dallas");
        Assert.Contains(result, c => c.Slug == "san-antonio");
    }

    [Fact]
    public async Task GetForecastAsync_WindDirectionMapped_ToCompassString()
    {
        var cities = new[]
        {
            new TexasCityConfig { Slug = "dallas", Name = "Dallas", Latitude = 32.7767, Longitude = -96.7970 }
        };
        var json = BuildOpenMeteoJson(1);
        var factory = BuildFactory(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        });
        var svc = new CityWeatherService(factory, BuildOptions(cities));

        var result = (await svc.GetForecastAsync("dallas"))!.ToList();

        // WindDirection should be a compass string, not a number string
        var validDirections = new[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
        Assert.Contains(result[0].WindDirection, validDirections);
    }
}

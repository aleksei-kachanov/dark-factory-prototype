using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DarkFactory.Weather;

/// <summary>
/// Configures a Swagger document for every discovered API version.
/// </summary>
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = "DarkFactory Weather API",
            Version = description.ApiVersion.ToString(),
        };

        if (description.IsDeprecated)
        {
            info.Description = "This API version has been deprecated.";
        }

        return info;
    }
}

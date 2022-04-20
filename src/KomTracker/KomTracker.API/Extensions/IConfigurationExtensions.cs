using KomTracker.API.Models.Configuration;
using KomTracker.Application.Models.Configuration;

namespace KomTracker.API.Extensions;

public static class IConfigurationExtensions
{
    private const string ApiConfiguration = "ApiConfiguration";
    private const string ApplicationConfiguration = "ApplicationConfiguration";

    public static ApiConfiguration GetApiConfiguration(this IConfiguration configuration)
    {
        return configuration
            .GetSection(IConfigurationExtensions.ApiConfiguration)
            .Get<ApiConfiguration>();
    }

    public static ApplicationConfiguration GetApplicationConfiguration(this IConfiguration configuration)
    {
        return configuration
            .GetSection(IConfigurationExtensions.ApplicationConfiguration)
            .Get<ApplicationConfiguration>();
    }
}

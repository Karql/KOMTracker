using KomTracker.Application.Models.Configuration;

namespace KomTracker.API.Extensions;

public static class IConfigurationExtensions
{
    private const string ApplicationConfiguration = "ApplicationConfiguration";

    public static ApplicationConfiguration GetApplicationConfiguration(this IConfiguration configuration)
    {
        return configuration
            .GetSection(IConfigurationExtensions.ApplicationConfiguration)
            .Get<ApplicationConfiguration>();
    }
}

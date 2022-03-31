using KomTracker.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KomTracker.Infrastructure.Identity;
using KomTracker.Infrastructure.Strava;
using Microsoft.AspNetCore.Builder;
using KomTracker.Infrastructure.Mail;

namespace KomTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddIdentity(configuration);
        services.AddStrava(configuration);
        services.AddMail(configuration);

        return services;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app)
    {
        app.UseIdentity();

        return app;
    }
}

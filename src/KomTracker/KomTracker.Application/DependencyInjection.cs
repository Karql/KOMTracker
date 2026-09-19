using FluentValidation;
using KomTracker.Application.Behaviors;
using KomTracker.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Reflection;

namespace KomTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Repo convention is English regardless of the server's OS culture.
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("en");

        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddTransient<IAthleteService, AthleteService>();
        services.AddTransient<IClubService, ClubService>();
        services.AddTransient<ISegmentService, SegmentService>();
        services.AddTransient<IStravaBikeSyncService, StravaBikeSyncService>();

        // Per-request so its bike-gear / parent-window caches are reused across a single list/recompute pass.
        services.AddScoped<ComponentMileageService>();

        return services;
    }
}

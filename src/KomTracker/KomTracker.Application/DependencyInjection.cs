using KomTracker.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace KomTracker.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            // TODO: Behaviors
            // cfg.AddBehavior<IPipelineBehavior<Ping, Pong>, PingPongBehavior>();
        });

        services.AddTransient<IAthleteService, AthleteService>();
        services.AddTransient<IClubService, ClubService>();
        services.AddTransient<ISegmentService, SegmentService>();

        return services;
    }
}

using KomTracker.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

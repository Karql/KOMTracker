using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Strava.API.Client;
using Strava.API.Client.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using IStravaTokenService = KomTracker.Application.Interfaces.Services.Strava.ITokenService;
using IStravaAthleteService = KomTracker.Application.Interfaces.Services.Strava.IAthleteService;
using IStravaSegmentService = KomTracker.Application.Interfaces.Services.Strava.ISegmentService;
using StravaTokenService = KomTracker.Infrastructure.Strava.Services.TokenService;
using StravaAthleteService = KomTracker.Infrastructure.Strava.Services.AthleteService;
using StravaSegmentService = KomTracker.Infrastructure.Strava.Services.SegmentService;
using static KomTracker.Infrastructure.Strava.Constants;

namespace KomTracker.Infrastructure.Strava;

public static class StravaDependencyInjection
{
    public static IServiceCollection AddStrava(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        services.AddTransient<IStravaTokenService, StravaTokenService>();
        services.AddTransient<IStravaAthleteService, StravaAthleteService>();
        services.AddTransient<IStravaSegmentService, StravaSegmentService>();

        // Strava.API.Client
        services.AddStravaApiClient();
        services.AddTransient(sp =>
        {
            return configuration
                .GetSection(ConfigurationSections.StravaApiClientConfiguration)
                .Get<StravaApiClientConfiguration>();
        });

        return services;
    }
}

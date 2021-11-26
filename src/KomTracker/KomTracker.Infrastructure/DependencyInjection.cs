using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Infrastructure.Persistence;
using KomTracker.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using IStravaTokenService = KomTracker.Application.Interfaces.Services.Strava.ITokenService;
using IStravaAthleteService = KomTracker.Application.Interfaces.Services.Strava.IAthleteService;
using IIdentityUserService = KomTracker.Application.Interfaces.Services.Identity.IUserService;
using StravaTokenService = KomTracker.Infrastructure.Services.Strava.TokenService;
using StravaAthleteService = KomTracker.Infrastructure.Services.Strava.AthleteService;
using IdentityUserService = KomTracker.Infrastructure.Services.Identity.UserService;
using Microsoft.EntityFrameworkCore;
using KomTracker.Infrastructure.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Strava.API.Client.Extensions;
using KomTracker.Infrastructure.Services.Identity;

namespace KomTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddScoped<IKOMUnitOfWork, EFKOMUnitOfWork>();
        services.AddScoped<IAthleteRepository, EFAthleteRepository>();
        services.AddScoped<ISegmentRepository, EFSegmentRepository>();
        services.AddTransient<IStravaTokenService, StravaTokenService>();
        services.AddTransient<IStravaAthleteService, StravaAthleteService>();
        services.AddTransient<IIdentityUserService, IdentityUserService>();

        services.AddDbContext<KOMDBContext>(options => options.UseNpgsql(configuration.GetConnectionString("DB")));
        services.AddIdentity<UserEntity, RoleEntity>()
            .AddEntityFrameworkStores<KOMDBContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<ClaimsFactory<UserEntity>>();

        // Strava.API.Client
        services.AddStravaApiClient();
        services.AddTransient<Strava.API.Client.Model.Config.ConfigModel>(sp =>
        {
            return configuration.GetSection("StravaApiClientConfig").Get<Strava.API.Client.Model.Config.ConfigModel>();
        });

        return services;
    }
}

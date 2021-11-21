using KomTracker.API.Infrastructure.Jobs;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Infrastructure.Entities.Identity;
using KomTracker.Infrastructure.Mappings;
using KomTracker.Infrastructure.Persistence;
using KomTracker.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Quartz;
using Strava.API.Client.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.AspNetCore.Extensions;

using IStravaTokenService = KomTracker.Application.Interfaces.Services.Strava.ITokenService;
using IStravaAthleteService = KomTracker.Application.Interfaces.Services.Strava.IAthleteService;
using IIdentityUserService = KomTracker.Application.Interfaces.Services.Identity.IUserService;
using StravaTokenService = KomTracker.Infrastructure.Services.Strava.TokenService;
using StravaAthleteService = KomTracker.Infrastructure.Services.Strava.AthleteService;
using IdentityUserService = KomTracker.Infrastructure.Services.Identity.UserService;
using KomTracker.Application.Services;

namespace KomTracker.API;

public class Startup
{
    private static readonly string AppName = "KOM Tracker";

    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = AppName,
                Version = "1.0.0"
            });
        });

        // KomTracker.Application
        services.AddTransient<IAccountService, AccountService>();
        services.AddTransient<IAthleteService, AthleteService>();        
        services.AddTransient<IKomService, KomService>();

        // KomTracker.Infrastructure
        services.AddAutoMapper(typeof(StravaApiClientProfile));
        services.AddScoped<IKOMUnitOfWork, EFKOMUnitOfWork>();
        services.AddScoped<IAthleteRepository, EFAthleteRepository>();
        services.AddScoped<ISegmentRepository, EFSegmentRepository>();
        services.AddTransient<IStravaTokenService, StravaTokenService>();
        services.AddTransient<IStravaAthleteService, StravaAthleteService>();
        services.AddTransient<IIdentityUserService, IdentityUserService>();

        services.AddDbContext<KOMDBContext>(options => options.UseNpgsql(_configuration.GetConnectionString("DB")));
        services.AddIdentity<UserEntity, IdentityRole>()
            .AddEntityFrameworkStores<KOMDBContext>()
            .AddDefaultTokenProviders();

        // Strava.API.Client
        services.AddStravaApiClient();
        services.AddTransient<Strava.API.Client.Model.Config.ConfigModel>(sp =>
        {
            return _configuration.GetSection("StravaApiClientConfig").Get<Strava.API.Client.Model.Config.ConfigModel>();
        });

        // Jobs
        services.AddTransient<TrackKomsJob>();

        services.AddQuartz(q =>
        {
            q.InterruptJobsOnShutdownWithWait = true;

            q.UseMicrosoftDependencyInjectionJobFactory();            

            q.ScheduleJob<TrackKomsJob>(trigger => trigger
                .WithCronSchedule("0 0 * * * ?")); // every hour
        });

        services.AddQuartzHostedService(options =>
        {
            // when shutting down we want jobs to complete gracefully
            options.WaitForJobsToComplete = true;


        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.ApplicationServices.MigrateDbContext<KOMDBContext>();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"v1/swagger.json", AppName);
        });

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
        });
    }
}

using KOMTracker.API.DAL;
using KOMTracker.API.DAL.Repositories;
using KOMTracker.API.Infrastructure.Jobs;
using KOMTracker.API.Infrastructure.Services;
using KOMTracker.API.Mappings;
using KOMTracker.API.Models.Identity;
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

namespace KOMTracker.API;

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

        services.AddAutoMapper(typeof(StravaApiClientProfile));

        services.AddDbContext<KOMDBContext>(options => options.UseNpgsql(_configuration.GetConnectionString("DB")));

        services.AddScoped<IKOMUnitOfWork, EFKOMUnitOfWork>();
        services.AddScoped<IAthleteRepository, EFAthleteRepository>();
        services.AddScoped<ISegmentRepository, EFSegmentRepository>();

        services.AddIdentity<UserModel, IdentityRole>()
            .AddEntityFrameworkStores<KOMDBContext>()
            .AddDefaultTokenProviders();

        services.AddTransient<ITokenService, TokenService>();
        services.AddTransient<IAthleteService, AthleteService>();
        services.AddTransient<IAccountService, AccountService>();
        services.AddTransient<IKomService, KomService>();
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

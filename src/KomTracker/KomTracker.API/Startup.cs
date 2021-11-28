using KomTracker.API.Infrastructure.Jobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.AspNetCore.Extensions;
using KomTracker.Application;
using KomTracker.Infrastructure;
using KomTracker.Infrastructure.Persistence;
using KomTracker.Infrastructure.Identity.Configurations;

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
        AddSwagger(services);

        services.AddApplication();
        services.AddInfrastructure(_configuration);

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

        

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
        });

        ConfigureSwagger(app);

        // ------
        app.UseInfrastructure();
    }

    private void AddSwagger(IServiceCollection services)
    {
        var identityConfiguration = _configuration.GetIdentityConfiguration();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = AppName,
                Version = "1.0.0"
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2
            };

            var identityUrl = identityConfiguration.IdentityUrl;

            var flow = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{identityUrl}/connect/authorize"),
                TokenUrl = new Uri($"{identityUrl}/connect/token"),
                RefreshUrl = new Uri($"{identityUrl}/connect/token"),

                Scopes = new Dictionary<string, string>
                {
                    {
                        KomTracker.Infrastructure.Identity.Constants.OAuth2.ScopeApi,
                        KomTracker.Infrastructure.Identity.Constants.OAuth2.ScopeApi
                    }
                }
            };

            securityScheme.Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = flow
            };

            options.AddSecurityDefinition("OAuth2", securityScheme);
        });
    }

    private void ConfigureSwagger(IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"v1/swagger.json", AppName); // relative to swagger-ui (for reverse-proxy etc.)
            c.OAuthClientId(KomTracker.Infrastructure.Identity.Constants.OAuth2.ClientId);
            c.OAuthUsePkce();
        });
    }
}

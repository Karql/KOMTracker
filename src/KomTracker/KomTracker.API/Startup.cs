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


using KomTracker.Application.Services;
using KomTracker.Application;
using KomTracker.Infrastructure;
using KomTracker.Infrastructure.Persistence;
using IdentityServer4.Models;
using KomTracker.Infrastructure.Entities.Identity;

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

        // -----------------------
        var apiScopes = new List<ApiScope>
        {
            new ApiScope("api", "KOM Tracker API")
        };

        var identityResource = new IdentityResource[] {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResource("roles", new[] { "role" }) //Add this line
        };

        var clients = new List<Client>
        {
            new Client
            {
                ClientId = "www",
                ClientName = "KOM Tracker WWW",

                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RequirePkce = true,
                RequireConsent = false,
                AllowOfflineAccess = true,
                UpdateAccessTokenClaimsOnRefresh = true,
                AlwaysIncludeUserClaimsInIdToken = true,

                AllowedScopes = { "openid", "profile", "roles", "api" },
                RedirectUris = { "http://localhost"}
            }
        };

        var builder = services.AddIdentityServer(options =>
            {
                options.UserInteraction.LoginUrl = "/accounts/login";
            })
            .AddDeveloperSigningCredential()        //This is for dev only scenarios when you don’t have a certificate to use.
            .AddInMemoryIdentityResources(identityResource)
            .AddInMemoryApiScopes(apiScopes)
            .AddInMemoryClients(clients)
            .AddAspNetIdentity<UserEntity>();
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


        // ------
        app.UseIdentityServer();
    }
}

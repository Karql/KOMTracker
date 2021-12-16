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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace KomTracker.API;

public class Startup
{
    private static readonly string AppName = "KOM Tracker";

    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();
        services.AddControllers();
        AddAuthentication(services);
        AddAuthorization(services);
        AddSwagger(services);
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

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

        ConfigureCors(app);
        app.UseRouting();        
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapDefaultControllerRoute();
        });


        ConfigureForReverseProxy(app);
        ConfigureSwagger(app);        

        // ------
        app.UseInfrastructure();
    }

    private void AddAuthentication(IServiceCollection services)
    {
        var identityConfiguration = _configuration.GetIdentityConfiguration();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = !string.IsNullOrWhiteSpace(identityConfiguration.Authority) ?
                    identityConfiguration.Authority : identityConfiguration.IdentityUrl;
                options.RequireHttpsMetadata = identityConfiguration.RequireHttpsMetadata;
                options.SaveToken = true;

                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidIssuers = new[] { identityConfiguration.IdentityUrl },
                    AudienceValidator = (audiences, securityToken, validationParameters) => true // allow all aud
                };
#if DEBUG
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = (ctx) =>
                    {
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = (err) =>
                    {
                        return Task.CompletedTask;
                    }
                };
#endif
            });
    }

    private void AddAuthorization(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            
        });
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

            options.AddSecurityDefinition("oauth2", securityScheme);

            // https://mattfrear.com/2018/07/21/add-an-authorization-header-to-your-swagger-ui-with-swashbuckle-revisited/
            options.OperationFilter<SecurityRequirementsOperationFilter>();
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

    private void ConfigureForReverseProxy(IApplicationBuilder app)
    {
        var forwardingOptions = new ForwardedHeadersOptions()
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
        };

        // https://stackoverflow.com/questions/43749236/net-core-x-forwarded-proto-not-working
        // Clear those collections for accept all proxies
        forwardingOptions.KnownNetworks.Clear(); //its loopback by default
        forwardingOptions.KnownProxies.Clear();

        app.UseForwardedHeaders(forwardingOptions);

        app.Use(async (context, next) =>
        {
            var forwardedPrefix = context.Request.Headers["x-forwarded-prefix"];
            if (!StringValues.IsNullOrEmpty(forwardedPrefix))
            {
                context.Request.PathBase = new PathString(forwardedPrefix);
            }

            await next();
        });
    }

    private void ConfigureCors(IApplicationBuilder app)
    {
        if (_environment.IsDevelopment())
        {
            app.UseCors(x => x
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials());
        }
    }
}

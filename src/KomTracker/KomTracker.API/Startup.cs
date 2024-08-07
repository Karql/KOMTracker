using KomTracker.API.Infrastructure.Jobs;
using Microsoft.OpenApi.Models;
using Quartz;
using Utils.AspNetCore.Extensions;
using KomTracker.Application;
using KomTracker.Infrastructure;
using KomTracker.Infrastructure.Persistence;
using KomTracker.Infrastructure.Identity.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Primitives;
using System.Reflection;
using KomTracker.API.Extensions;
using Microsoft.AspNetCore.DataProtection;
using KomTracker.API.Models.Configuration;
using KomTracker.Application.Models.Configuration;
using Utils.Helpers;
using TimeZoneConverter;

namespace KomTracker.API;

public class Startup
{
    private static readonly string AppName = "KOM Tracker";

    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    private readonly ApiConfiguration _apiConfiguration;
    private readonly ApplicationConfiguration _applicationConfiguration;

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));

        _apiConfiguration = _configuration.GetApiConfiguration();
        _applicationConfiguration = _configuration.GetApplicationConfiguration();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors();
        services.AddControllers();
        AddDataProtection(services);
        AddAuthentication(services);
        AddAuthorization(services);
        AddSwagger(services);
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddSingleton(_apiConfiguration);
        services.AddSingleton(_applicationConfiguration);

        services.AddApplication();
        services.AddInfrastructure(_configuration);

        // Jobs
        services.AddTransient<TrackKomsJob>();
        services.AddTransient<RefreshSegmentsJob>();
        services.AddTransient<RefreshClubsJob>();
        services.AddTransient<RefreshStatsJob>();

        services.AddQuartz(q =>
        {
            var tz = TZConvert.GetTimeZoneInfo("Europe/Warsaw");
            q.InterruptJobsOnShutdownWithWait = true;

            q.UseMicrosoftDependencyInjectionJobFactory();

            if (_applicationConfiguration.TrackKomsJobEnabled)
            {
                q.ScheduleJob<TrackKomsJob>(trigger => trigger
                    .WithCronSchedule("0 0 * * * ?", action => action.InTimeZone(tz))); // every hour 
            }

            if (_applicationConfiguration.RefreshSegmentsJobEnabled)
            {
                q.ScheduleJob<RefreshSegmentsJob>(trigger => trigger
                    .WithCronSchedule("0 30 * * * ?", action => action.InTimeZone(tz))); // half past every hour
            }

            if (_applicationConfiguration.RefreshClubsJobEnabled)
            {               
                q.ScheduleJob<RefreshClubsJob>(trigger => trigger
                    .WithCronSchedule("0 45 0,12 * * ?", action => action.InTimeZone(tz))); // 45 past midnight and midday
            }

            if (_applicationConfiguration.RefreshStatsJobEnabled)
            {
                q.ScheduleJob<RefreshStatsJob>(trigger => trigger
                    .WithCronSchedule("0 55 23 * * ?", action => action.InTimeZone(tz))); // 23:55 Europe/Warsaw
            }
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

    private void AddDataProtection(IServiceCollection services)
    {
        var keysPath = IOHelper.ResolvePath(_apiConfiguration.KeysPath ?? throw new ArgumentException("KeysPath not defined", nameof(_apiConfiguration.KeysPath)));

        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));
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
                        KomTracker.Infrastructure.Shared.Identity.Constants.OAuth2.Scopes.Api,
                        KomTracker.Infrastructure.Shared.Identity.Constants.OAuth2.Scopes.Api
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
            c.OAuthClientId(KomTracker.Infrastructure.Shared.Identity.Constants.OAuth2.ClientId);
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

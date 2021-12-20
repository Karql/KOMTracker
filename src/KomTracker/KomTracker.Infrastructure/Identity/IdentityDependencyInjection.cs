using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using KomTracker.Application.Interfaces.Services.Identity;
using KomTracker.Infrastructure.Identity.Configurations;
using KomTracker.Infrastructure.Identity.Endpoints;
using KomTracker.Infrastructure.Identity.Entities;
using KomTracker.Infrastructure.Identity.Services;
using KomTracker.Infrastructure.Identity.Validation;
using KomTracker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Helpers;
using static KomTracker.Infrastructure.Shared.Identity.Constants;

namespace KomTracker.Infrastructure.Identity;

public static class IdentityDependencyInjection
{
    public static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IIdentityService, IdentityService>();

        services.AddSingleton<IdentityConfiguration>(configuration.GetIdentityConfiguration());

        services.AddIdentity<UserEntity, RoleEntity>()
            .AddEntityFrameworkStores<KOMDBContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<ClaimsFactory<UserEntity>>();

        services.AddTransient<IRedirectUriValidator, StartWithRedirectUriValidator>();

        var keyPath = IOHelper.ResolvePath("~/rsa/key.jwk");
        IOHelper.CreateDirectoryForFile(keyPath);

        var builder = services.AddIdentityServer()
            .AddDeveloperSigningCredential(filename: keyPath) // For now should be enough
            .AddInMemoryIdentityResources(GetIdentityResources())
            .AddInMemoryApiScopes(GetApiScopes())
            .AddInMemoryClients(GetClients(configuration))
            .AddAspNetIdentity<UserEntity>()
            .AddEndpoint<LoginEndpoint>(EndpointNames.Login, ProtocolRoutePaths.Loing)
            .AddEndpoint<ConnectEndpoint>(EndpointNames.Connect, ProtocolRoutePaths.Connect);

        // Remove BasicAuthenticationSecretParser
        // With this swagger protected by basic auth not working.
        var basicAuthenticationSecretParser = services.FirstOrDefault(x => x.ImplementationType == typeof(BasicAuthenticationSecretParser));
        if (basicAuthenticationSecretParser != null)
        {
            services.Remove(basicAuthenticationSecretParser);
        }

        return services;
    }

    public static IApplicationBuilder UseIdentity(this IApplicationBuilder app)
    {
        // Add IdentityServer as Area identity
        // https://stackoverflow.com/a/51197799/11391667
        app.Map("/identity", builder =>
        {
            builder.UseIdentityServer();
        });

        return app;
    }

    private static IdentityResource[] GetIdentityResources()
    {
        var profile = new IdentityResources.Profile();
        profile.UserClaims.Add(Claims.AthleteId);

        return new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            profile,
        };
    }

    private static ApiScope[] GetApiScopes()
    {
        return new []
        {
            new ApiScope(OAuth2.ScopeApi, "KOM Tracker API", new [] { JwtClaimTypes.Role, Claims.AthleteId })
        };
    }

    private static Client[] GetClients(IConfiguration configuration)
    {
        var identityConfiguration = configuration.GetIdentityConfiguration();

        return new[]
        {
            new Client
            {
                ClientId = OAuth2.ClientId,
                ClientName = "KOM Tracker",

                AllowedGrantTypes = GrantTypes.Code,
                RequireClientSecret = false,
                RequirePkce = true,
                RequireConsent = false,
                AllowOfflineAccess = true,
                UpdateAccessTokenClaimsOnRefresh = true,
                AlwaysIncludeUserClaimsInIdToken = true,

                AllowedScopes = { "openid", "profile", OAuth2.ScopeApi },
                RedirectUris = identityConfiguration.RedirectUris
            }
        };
    }
}

using Blazored.LocalStorage;
using FisSst.BlazorMaps.DependencyInjection;
using KomTracker.WEB.Infrastructure.Services.Preference;
using KomTracker.WEB.Infrastructure.Services.User;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using IdentityConstants = KomTracker.Infrastructure.Shared.Identity.Constants;

namespace KomTracker.WEB;

public static class DependencyInjection
{
    private const string HttpClientName = "KomTracker.API";
    private const string ConfigurationKey_IdentityConfiguration = "IdentityConfiguration";
    private const string ConfigurationKey_KomTrackerApiUrl = "KomTrackerApiUrl";

    public static WebAssemblyHostBuilder AddClientServices(this WebAssemblyHostBuilder builder)
    {
        // TODO: ConfigurationService
        var komTrackerApiUrl = builder.Configuration[ConfigurationKey_KomTrackerApiUrl];

        if (komTrackerApiUrl == null)
        {
            throw new Exception($"Invalid configuration \"{ConfigurationKey_KomTrackerApiUrl}\" should not be null!");
        }

        var services = builder.Services;

        services.AddScoped<IPreferenceService, PreferenceService>();
        services.AddScoped<IUserService, UserService>();

        services.AddBlazoredLocalStorage();

        services.AddOidcAuthentication(options =>
        {
            builder.Configuration.Bind(ConfigurationKey_IdentityConfiguration, options.ProviderOptions);

            options.ProviderOptions.ResponseType = IdentityConstants.OAuth2.CodeFlow;
            options.ProviderOptions.DefaultScopes.Add(IdentityConstants.OAuth2.Scopes.OpenId);
            options.ProviderOptions.DefaultScopes.Add(IdentityConstants.OAuth2.Scopes.Profile);
            options.ProviderOptions.DefaultScopes.Add(IdentityConstants.OAuth2.Scopes.Api);
            options.ProviderOptions.DefaultScopes.Add(IdentityConstants.OAuth2.Scopes.OfflineAccess);
        });

        services.AddHttpClient(HttpClientName, client =>
        {
            client.BaseAddress = new Uri(komTrackerApiUrl);
        })
        .AddHttpMessageHandler(sp =>
        {
            var handler = sp.GetService<AuthorizationMessageHandler>()!
            .ConfigureHandler(
                authorizedUrls: new[] { komTrackerApiUrl },
                scopes: new[] { IdentityConstants.OAuth2.Scopes.Api }
            );

            return handler;
        });

        services.AddScoped(sp => sp.GetService<IHttpClientFactory>()!.CreateClient(HttpClientName));

        services.AddMudServices();

        services.AddBlazorLeafletMaps();

        return builder;
    }
}

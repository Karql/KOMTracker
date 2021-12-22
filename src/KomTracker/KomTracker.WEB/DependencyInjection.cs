using Blazored.LocalStorage;
using KomTracker.WEB.Infrastructure.Services.Preference;
using KomTracker.WEB.Infrastructure.Services.User;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using IdentityConstants = KomTracker.Infrastructure.Shared.Identity.Constants;

namespace KomTracker.WEB;

public static class DependencyInjection
{
    private const string ClientName = "KomTracker.API";
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
            builder.Configuration.Bind("IdentityConfiguration", options.ProviderOptions);

            options.ProviderOptions.ResponseType = "code";
            options.ProviderOptions.DefaultScopes.Add("openid");
            options.ProviderOptions.DefaultScopes.Add("profile");
            options.ProviderOptions.DefaultScopes.Add(IdentityConstants.OAuth2.ScopeApi);
            options.ProviderOptions.DefaultScopes.Add("offline_access");
        });

        services.AddHttpClient(ClientName, client =>
        {
            client.BaseAddress = new Uri(komTrackerApiUrl);
        })
        .AddHttpMessageHandler(sp =>
        {
            var handler = sp.GetService<AuthorizationMessageHandler>()!
            .ConfigureHandler(
                authorizedUrls: new[] { komTrackerApiUrl },
                scopes: new[] { IdentityConstants.OAuth2.ScopeApi }
            );

            return handler;
        });

        services.AddScoped(sp => sp.GetService<IHttpClientFactory>()!.CreateClient(ClientName));

        services.AddMudServices();

        return builder;
    }
}

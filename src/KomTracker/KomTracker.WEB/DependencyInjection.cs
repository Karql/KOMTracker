using Blazored.LocalStorage;
using KomTracker.WEB.Infrastructure.Services.Preference;
using KomTracker.WEB.Infrastructure.Services.User;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace KomTracker.WEB;

public static class DependencyInjection
{
    private const string ClientName = "KomTracker.API";

    public static WebAssemblyHostBuilder AddClientServices(this WebAssemblyHostBuilder builder)
    {
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
            options.ProviderOptions.DefaultScopes.Add("api");
            options.ProviderOptions.DefaultScopes.Add("offline_access");
        });

        services.AddHttpClient(ClientName, client =>
        {
            client.BaseAddress = new Uri("https://localhost:9999/kom-tracker-api/");
        })
        .AddHttpMessageHandler(sp =>
        {
            var handler = sp.GetService<AuthorizationMessageHandler>()!
            .ConfigureHandler(
                authorizedUrls: new[] { "https://localhost:9999/kom-tracker-api/identity" },
                scopes: new[] { "api" }
            );

            return handler;
        });

        services.AddScoped(sp => sp.GetService<IHttpClientFactory>()!.CreateClient(ClientName));

        services.AddMudServices();

        return builder;
    }
}

using Blazored.LocalStorage;
using KomTracker.WEB.Services.Preference;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace KomTracker.WEB;

public static class DependencyInjection
{
    public static WebAssemblyHostBuilder AddClientServices(this WebAssemblyHostBuilder builder)
    {
        var services = builder.Services;

        services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

        services.AddScoped<IPreferenceService, PreferenceService>();

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

        services.AddMudServices();

        return builder;
    }
}

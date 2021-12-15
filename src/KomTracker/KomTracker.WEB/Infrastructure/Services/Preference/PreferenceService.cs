using Blazored.LocalStorage;
using KomTracker.WEB.Models.Preference;
using KomTracker.WEB.Settings;
using MudBlazor;

namespace KomTracker.WEB.Infrastructure.Services.Preference;

public class PreferenceService : IPreferenceService
{
    private readonly ILocalStorageService _localStorageService;

    public PreferenceService(ILocalStorageService localStorageService)
    {
        _localStorageService = localStorageService;
    }
    public async Task<MudTheme> GetCurrentThemeAsync()
    {
        var preference = await GetPreferenceAsync();

        if (preference.DarkMode == true)
        {
            return Theme.DarkTheme;
        }

        return Theme.DefaultTheme;
    }

    public async Task ToggleDarkModeAsync()
    {
        var preference = await GetPreferenceAsync();
        preference.DarkMode = !preference.DarkMode;
        await SetPreference(preference);
    }

    private async Task<PreferenceModel> GetPreferenceAsync()
    {
        return await _localStorageService.GetItemAsync<PreferenceModel>(Constants.Storage.Preference) ?? new PreferenceModel();
    }

    private async Task SetPreference(PreferenceModel preference)
    {
        await _localStorageService.SetItemAsync(Constants.Storage.Preference, preference);
    }
}

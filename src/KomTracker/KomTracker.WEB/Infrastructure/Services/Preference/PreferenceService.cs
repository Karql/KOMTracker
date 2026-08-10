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
    public async Task<bool> IsDarkModeAsync()
    {
        var preference = await GetPreferenceAsync();

        return preference.DarkMode;
    }

    public async Task ToggleDarkModeAsync()
    {
        var preference = await GetPreferenceAsync();
        preference.DarkMode = !preference.DarkMode;
        await SetPreference(preference);
    }

    public async Task<ListViewMode> GetListViewAsync(string key, ListViewMode fallback = ListViewMode.Card)
    {
        var preference = await GetPreferenceAsync();

        return preference.ListViews.TryGetValue(key, out var mode) ? mode : fallback;
    }

    public async Task SetListViewAsync(string key, ListViewMode mode)
    {
        var preference = await GetPreferenceAsync();
        preference.ListViews[key] = mode;
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

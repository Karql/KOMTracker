using MudBlazor;

namespace KomTracker.WEB.Services.Preference;

public interface IPreferenceService
{
    Task<MudTheme> GetCurrentThemeAsync();
    Task ToggleDarkModeAsync();
}

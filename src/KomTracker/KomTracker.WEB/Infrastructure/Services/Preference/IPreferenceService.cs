using MudBlazor;

namespace KomTracker.WEB.Infrastructure.Services.Preference;

public interface IPreferenceService
{
    Task<MudTheme> GetCurrentThemeAsync();
    Task ToggleDarkModeAsync();
}

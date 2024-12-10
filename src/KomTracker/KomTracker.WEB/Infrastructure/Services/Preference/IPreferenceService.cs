using MudBlazor;

namespace KomTracker.WEB.Infrastructure.Services.Preference;

public interface IPreferenceService
{
    Task<bool> IsDarkModeAsync();
    Task ToggleDarkModeAsync();
}

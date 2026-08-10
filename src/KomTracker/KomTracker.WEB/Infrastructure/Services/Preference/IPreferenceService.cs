using KomTracker.WEB.Models.Preference;

namespace KomTracker.WEB.Infrastructure.Services.Preference;

public interface IPreferenceService
{
    Task<bool> IsDarkModeAsync();
    Task ToggleDarkModeAsync();

    /// <summary>Get the saved card/table view for a page/list (falls back if none saved). Reusable across views by key.</summary>
    Task<ListViewMode> GetListViewAsync(string key, ListViewMode fallback = ListViewMode.Card);

    /// <summary>Persist the card/table view for a page/list.</summary>
    Task SetListViewAsync(string key, ListViewMode mode);
}

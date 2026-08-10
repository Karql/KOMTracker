namespace KomTracker.WEB.Models.Preference;

public class PreferenceModel
{
    public bool DarkMode { get; set; } = false;

    /// <summary>Per-page card/table view preference, keyed by a page/list id (e.g. "bikes").</summary>
    public Dictionary<string, ListViewMode> ListViews { get; set; } = new();
}

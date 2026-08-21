using System;

namespace KomTracker.API.Shared.ViewModels.Installation;

/// <summary>Remove (uninstall) a component: closes the active Tracked window with a DateTo.</summary>
public class RemoveInstallationViewModel
{
    public DateTime DateTo { get; set; }
}

using System;
using KomTracker.Domain.Entities.Component;

namespace KomTracker.API.Shared.ViewModels.Installation;

/// <summary>Move a currently-installed component to another bike: closes the current window, opens a new one.</summary>
public class MoveInstallationViewModel
{
    public int NewBikeId { get; set; }
    public InstallationPosition? NewPosition { get; set; }
    public DateTime MoveDate { get; set; }
}

using System;
using KomTracker.Domain.Entities.Component;

namespace KomTracker.API.Shared.ViewModels.Component;

public class ChangeComponentLifecycleViewModel
{
    public ComponentLifecycle Lifecycle { get; set; }
    public DateTime? SaleDate { get; set; }
    public decimal? SalePrice { get; set; }

    /// <summary>Optional note to save with the transition. Null = leave the component's notes unchanged.</summary>
    public string? Notes { get; set; }
}

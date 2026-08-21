using System;
using KomTracker.Domain.Entities.Component;

namespace KomTracker.API.Shared.ViewModels.Installation;

/// <summary>An installation row (read model): a component mounted on a bike over a window, or a Manual historical entry.</summary>
public class InstallationViewModel
{
    public int Id { get; set; }

    public int ComponentId { get; set; }
    public string? ComponentName { get; set; }
    public ComponentCategory? ComponentCategory { get; set; }

    public int? BikeId { get; set; }
    public string? BikeName { get; set; }

    public ComponentInstallationType Type { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public InstallationPosition? Position { get; set; }

    public decimal? ManualDistanceKm { get; set; }
    public decimal? ManualMovingHours { get; set; }
    public decimal? ManualElevationM { get; set; }

    /// <summary>Currently installed = an active Tracked window (no DateTo). Manual is never current.</summary>
    public bool IsCurrent { get; set; }
}

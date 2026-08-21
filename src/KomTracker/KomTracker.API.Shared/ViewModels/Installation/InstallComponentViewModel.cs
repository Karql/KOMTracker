using System;
using KomTracker.Domain.Entities.Component;

namespace KomTracker.API.Shared.ViewModels.Installation;

/// <summary>Install a component on a bike — Tracked (dated) or Manual (dateless historical, static totals).</summary>
public class InstallComponentViewModel
{
    public int ComponentId { get; set; }
    public int BikeId { get; set; }
    public ComponentInstallationType Type { get; set; }

    public DateTime? DateFrom { get; set; }
    public InstallationPosition? Position { get; set; }

    // Manual only
    public decimal? ManualDistanceKm { get; set; }
    public decimal? ManualMovingHours { get; set; }
    public decimal? ManualElevationM { get; set; }
}

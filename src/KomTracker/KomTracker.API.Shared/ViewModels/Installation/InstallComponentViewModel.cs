using System;
using KomTracker.Domain.Entities.Component;

namespace KomTracker.API.Shared.ViewModels.Installation;

/// <summary>Install a component onto a bike (<see cref="BikeId"/>) XOR into a parent component (<see cref="ParentComponentId"/>) — Tracked or Manual.</summary>
public class InstallComponentViewModel
{
    public int ComponentId { get; set; }
    public int? BikeId { get; set; }
    public int? ParentComponentId { get; set; }
    public ComponentInstallationType Type { get; set; }

    public DateTime? DateFrom { get; set; }

    /// <summary>Optional (Tracked only): set to record an already-closed historical window in one step.</summary>
    public DateTime? DateTo { get; set; }

    public InstallationPosition? Position { get; set; }

    // Manual only
    public decimal? ManualDistanceKm { get; set; }
    public decimal? ManualMovingHours { get; set; }
    public decimal? ManualElevationM { get; set; }
}

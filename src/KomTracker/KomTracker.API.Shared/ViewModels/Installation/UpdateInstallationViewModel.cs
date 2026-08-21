using System;
using KomTracker.Domain.Entities.Component;

namespace KomTracker.API.Shared.ViewModels.Installation;

/// <summary>Edit an existing installation record (corrections). Type is immutable; fields apply per the row's type.</summary>
public class UpdateInstallationViewModel
{
    public int BikeId { get; set; }
    public InstallationPosition? Position { get; set; }

    // Tracked only
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    // Manual only
    public decimal? ManualDistanceKm { get; set; }
    public decimal? ManualMovingHours { get; set; }
    public decimal? ManualElevationM { get; set; }
}

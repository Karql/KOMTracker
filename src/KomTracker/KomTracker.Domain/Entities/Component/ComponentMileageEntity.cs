#nullable enable
using System;
using KomTracker.Domain.Contracts;

namespace KomTracker.Domain.Entities.Component;

/// <summary>
/// Stored mileage projection for a component (BikeTracker Phase 3). 1:1 with <see cref="ComponentEntity"/>
/// (PK == FK, cascade-deleted with it). Recompute-from-source: rebuilt by RecalculateComponentsMileageCommand on
/// installation/lifecycle changes and after activity sync, so the list/detail never group+sum over activities on read.
/// Table: bt.component_mileage.
/// </summary>
public class ComponentMileageEntity : BaseEntity
{
    /// <summary>The component this row belongs to (primary key == foreign key to bt.component).</summary>
    public int ComponentId { get; set; }

    /// <summary>Initial seed + Manual baselines + attributed ride distance (km).</summary>
    public decimal TotalDistanceKm { get; set; }

    public decimal TotalMovingHours { get; set; }

    public decimal TotalElevationM { get; set; }

    /// <summary>Number of Strava rides attributed through the installation chain (seed/manual excluded).</summary>
    public int AttributedActivityCount { get; set; }

    /// <summary>UTC timestamp of the last recompute.</summary>
    public DateTime ComputedAt { get; set; }
}

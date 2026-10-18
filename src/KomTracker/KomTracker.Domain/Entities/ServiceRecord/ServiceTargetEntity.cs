#nullable enable
using System;
using System.ComponentModel.DataAnnotations.Schema;
using KomTracker.Domain.Entities.Component;

namespace KomTracker.Domain.Entities.ServiceRecord;

/// <summary>
/// One thing a service covered — a bike <b>XOR</b> a component (bt.service_target). Also holds the stored projection of
/// that target's mileage <em>at the service date</em> (D-SRV-4): recomputed from source by
/// <c>RecalculateServiceMileageCommand</c>, never edited by hand. Null until first computed.
/// </summary>
public class ServiceTargetEntity
{
    public int Id { get; set; }

    public int ServiceId { get; set; }

    public int? BikeId { get; set; }

    public int? ComponentId { get; set; }

    /// <summary>Mileage of the target at the service date (seed + manual + rides before the date).</summary>
    public decimal? DistanceKm { get; set; }

    public decimal? MovingHours { get; set; }

    public decimal? ElevationM { get; set; }

    /// <summary>When the mileage above was last recomputed (UTC).</summary>
    public DateTime? ComputedAt { get; set; }

    public ServiceRecordEntity Service { get; set; } = default!;

    // Read model (resolved by the service queries, not persisted).

    /// <summary>The bike's / component's name.</summary>
    [NotMapped]
    public string? TargetName { get; set; }

    /// <summary>The component's category (null for a bike).</summary>
    [NotMapped]
    public ComponentCategory? TargetCategory { get; set; }
}

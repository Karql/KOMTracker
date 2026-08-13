#nullable enable
using KomTracker.Domain.Contracts;

namespace KomTracker.Domain.Entities.Bike;

/// <summary>
/// The single point where a BikeTracker bike is coupled to an external-service gear (D-2).
/// Table: bt.bike_link. 1 bike -> N links; unique per (ExternalService, ExternalId).
/// The bt.bike -> bike_link edge is a real same-schema FK; the bt <-> strava boundary stays a soft
/// match (this ExternalId == strava.activity.gear_id / strava.bike.id), resolved in app/job logic (D-10).
/// </summary>
public class BikeLinkEntity : BaseEntity
{
    public int Id { get; set; }

    /// <summary>FK -> bt.bike.</summary>
    public int BikeId { get; set; }

    public ExternalService ExternalService { get; set; }

    /// <summary>External gear id (Strava gear id, e.g. "b1234567").</summary>
    public string ExternalId { get; set; } = default!;
}

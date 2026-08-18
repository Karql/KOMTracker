using System;

namespace KomTracker.Application.Models.Strava;

/// <summary>A row for the Strava activities list, with its resolved bike link (if any).</summary>
public class ActivityListItemModel
{
    public long Id { get; set; }                 // Strava activity id
    public string? Name { get; set; }
    public string? SportType { get; set; }

    public double DistanceMeters { get; set; }
    public int MovingTimeSeconds { get; set; }
    public double AverageSpeedMps { get; set; }
    public double ElevationMeters { get; set; }

    public DateTime StartDateUtc { get; set; }
    public double UtcOffset { get; set; }         // seconds; local = StartDateUtc + UtcOffset

    public string? GearId { get; set; }
    public int? LinkedBikeId { get; set; }
    public string? LinkedBikeName { get; set; }

    /// <summary>The gear's name as it is on Strava (strava.bike), shown even when not linked to a bt.bike.</summary>
    public string? StravaBikeName { get; set; }
}

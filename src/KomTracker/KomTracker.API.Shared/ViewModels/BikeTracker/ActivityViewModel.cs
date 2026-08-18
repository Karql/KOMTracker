using System;

namespace KomTracker.API.Shared.ViewModels.BikeTracker;

/// <summary>A row on the Strava activities page (units already converted for display).</summary>
public class ActivityViewModel
{
    public long Id { get; set; }                 // Strava activity id (deep-link)
    public string? Name { get; set; }
    public string? SportType { get; set; }

    public double DistanceKm { get; set; }
    public int MovingTimeSeconds { get; set; }   // WEB formats h/m
    public double AverageSpeedKmh { get; set; }
    public double ElevationM { get; set; }

    /// <summary>Activity start in the athlete's local time (UTC + utc_offset).</summary>
    public DateTime StartDateLocal { get; set; }

    public string? GearId { get; set; }
    public int? LinkedBikeId { get; set; }
    public string? LinkedBikeName { get; set; }
    public string? StravaBikeName { get; set; }

    /// <summary>Whether this activity used a bike (Strava gear id starting with "b") — only then show a bike.</summary>
    public bool IsBikeRide => GearId is not null && GearId.StartsWith("b");
}

#nullable enable
using System;
using KomTracker.Domain.Contracts;

namespace KomTracker.Domain.Entities.Strava;

/// <summary>
/// A Strava activity, synced 1:1 from GET /athlete/activities (SummaryActivity). Table: strava.activity.
/// Keyed by the Strava activity id; carries Strava's own athlete_id + gear_id.
/// Dates are UTC (timestamptz); local = start_date + utc_offset (start_date_local is NOT stored — bogus Z, D-15).
/// </summary>
public class ActivityEntity : BaseEntity
{
    public long Id { get; set; }              // Strava activity id
    public int AthleteId { get; set; }        // Strava athlete id (FK -> athlete)
    // Strava gear id (e.g. "b1234567"), null when no gear was attached to the ride.
    // NOTE: this is Strava's own id — NOT a DB FK. Gear and activities sync independently, so the two can
    // be out of step (e.g. a gear delete/re-sync lands before activities refresh). Any bike ↔ gear linking
    // (1c) must tolerate a GearId that points at gear we don't currently have.
    public string? GearId { get; set; }

    public string Name { get; set; } = default!;
    public string? ExternalId { get; set; }
    public long? UploadId { get; set; }

    // Metrics (metres / seconds)
    public double Distance { get; set; }
    public int MovingTime { get; set; }
    public int ElapsedTime { get; set; }
    public double TotalElevationGain { get; set; }
    public double? ElevHigh { get; set; }
    public double? ElevLow { get; set; }

    // Type
    public string? Type { get; set; }         // deprecated by Strava
    public string SportType { get; set; } = default!;
    public int? WorkoutType { get; set; }

    // Dates
    public DateTime StartDate { get; set; }   // UTC
    public string? Timezone { get; set; }
    public double UtcOffset { get; set; }     // seconds, DST-correct

    // Flags
    public bool Trainer { get; set; }
    public bool Commute { get; set; }
    public bool Manual { get; set; }
    public bool Private { get; set; }
    public bool Flagged { get; set; }
    public string? Visibility { get; set; }

    // Speed / power / HR
    public double AverageSpeed { get; set; }
    public double MaxSpeed { get; set; }
    public double? AverageCadence { get; set; }
    public int? AverageTemp { get; set; }
    public double? AverageWatts { get; set; }
    public double? WeightedAverageWatts { get; set; }
    public double? MaxWatts { get; set; }
    public bool DeviceWatts { get; set; }
    public double? Kilojoules { get; set; }
    public bool HasHeartrate { get; set; }
    public double? AverageHeartrate { get; set; }
    public double? MaxHeartrate { get; set; }
    public double? SufferScore { get; set; }

    // Counts
    public int AchievementCount { get; set; }
    public int KudosCount { get; set; }
    public int CommentCount { get; set; }
    public int AthleteCount { get; set; }
    public int PhotoCount { get; set; }
    public int TotalPhotoCount { get; set; }
    public int PrCount { get; set; }

    // Map / geo / device
    public string? SummaryPolyline { get; set; }
    public double? StartLat { get; set; }
    public double? StartLng { get; set; }
    public double? EndLat { get; set; }
    public double? EndLng { get; set; }
    public string? DeviceName { get; set; }
}

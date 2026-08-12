using KomTracker.Domain.Entities.Strava;
using ApiModel = Strava.API.Client.Model;

namespace KomTracker.Infrastructure.Strava.Mappings;

/// <summary>Explicit Strava activity → entity mapping (no AutoMapper for new code — D-P0-13).</summary>
public static class ActivityMappings
{
    public static ActivityEntity ToEntity(this ApiModel.Activity.ActivitySummaryModel m, int athleteId)
    {
        return new ActivityEntity
        {
            Id = m.Id,
            AthleteId = athleteId,
            GearId = m.GearId,

            Name = m.Name,
            ExternalId = m.ExternalId,
            UploadId = m.UploadId,

            Distance = m.Distance,
            MovingTime = m.MovingTime,
            ElapsedTime = m.ElapsedTime,
            TotalElevationGain = m.TotalElevationGain,
            ElevHigh = m.ElevHigh,
            ElevLow = m.ElevLow,

            Type = m.Type,
            SportType = m.SportType,
            WorkoutType = m.WorkoutType,

            StartDate = m.StartDate,           // UTC (start_date_local intentionally not stored — D-15)
            Timezone = m.Timezone,
            UtcOffset = m.UtcOffset,

            Trainer = m.Trainer,
            Commute = m.Commute,
            Manual = m.Manual,
            Private = m.Private,
            Flagged = m.Flagged,
            Visibility = m.Visibility,

            AverageSpeed = m.AverageSpeed,
            MaxSpeed = m.MaxSpeed,
            AverageCadence = m.AverageCadence,
            AverageTemp = m.AverageTemp,
            AverageWatts = m.AverageWatts,
            WeightedAverageWatts = m.WeightedAverageWatts,
            MaxWatts = m.MaxWatts,
            DeviceWatts = m.DeviceWatts,
            Kilojoules = m.Kilojoules,
            HasHeartrate = m.HasHeartrate,
            AverageHeartrate = m.AverageHeartrate,
            MaxHeartrate = m.MaxHeartrate,
            SufferScore = m.SufferScore,

            AchievementCount = m.AchievementCount,
            KudosCount = m.KudosCount,
            CommentCount = m.CommentCount,
            AthleteCount = m.AthleteCount,
            PhotoCount = m.PhotoCount,
            TotalPhotoCount = m.TotalPhotoCount,
            PrCount = m.PrCount,

            SummaryPolyline = m.Map?.SummaryPolyline,
            StartLat = Coord(m.StartLatlng, 0),
            StartLng = Coord(m.StartLatlng, 1),
            EndLat = Coord(m.EndLatlng, 0),
            EndLng = Coord(m.EndLatlng, 1),
            DeviceName = m.DeviceName
        };
    }

    private static double? Coord(float[] latlng, int index)
        => latlng is not null && latlng.Length > index ? latlng[index] : null;
}

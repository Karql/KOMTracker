using KomTracker.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace KomTracker.Domain.Entities.Segment;

public class SegmentEffortEntity : BaseEntity
{
    public long Id { get; set; }
    /// <summary>
    /// No DB representation for now
    /// </summary>
    public long ActivityId { get; set; }
    public int AthleteId { get; set; }
    public long SegmentId { get; set; }
    public string Name { get; set; }
    public int ElapsedTime { get; set; }
    public int MovingTime { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime StartDateLocal { get; set; }
    public float Distance { get; set; }
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public float? AverageCadence { get; set; }
    public bool DeviceWatts { get; set; }
    public float? AverageWatts { get; set; }
    public float? AverageHeartrate { get; set; }
    public float? MaxHeartrate { get; set; }
    public int? PrRank { get; set; }
    public int? KomRank { get; set; }

    public List<KomsSummaryEntity> KomSummaries { get; set; }
}

public class SegmentEffortEntityComparer : EqualityComparer<SegmentEffortEntity>
{
    public override bool Equals(SegmentEffortEntity x, SegmentEffortEntity y)
    {
        return x.Id == y.Id;
    }

    public override int GetHashCode([DisallowNull] SegmentEffortEntity obj)
    {
        return obj.Id.GetHashCode();
    }
}
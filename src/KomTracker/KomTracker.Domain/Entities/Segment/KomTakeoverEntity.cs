using KomTracker.Domain.Contracts;
using System;

namespace KomTracker.Domain.Entities.Segment;

/// <summary>
/// A detected KOM takeover between two app users on a segment:
/// the <see cref="TakenSegmentEffortId"/> effort took the KOM that the
/// <see cref="LostSegmentEffortId"/> effort held.
/// Lean by design - athletes/segment/sex are derivable via the referenced
/// efforts/summaries and will be denormalized later if the ranking needs it.
/// </summary>
public class KomTakeoverEntity : BaseEntity
{
    public int Id { get; set; }

    /// <summary>Winning effort (took the KOM). Unique - one takeover per winning effort.</summary>
    public long TakenSegmentEffortId { get; set; }

    /// <summary>Beaten effort (lost the KOM).</summary>
    public long LostSegmentEffortId { get; set; }

    /// <summary>Summary in which the gain (NewKom) was detected.</summary>
    public int TakenKomsSummaryId { get; set; }

    /// <summary>Summary in which the loss (LostKom) was detected.</summary>
    public int LostKomsSummaryId { get; set; }

    /// <summary>
    /// When the takeover effectively happened - copied from the taking (gain) koms_summary's TrackDate.
    /// Denormalized for time-based ranking; audit_cd is just when the row hit the DB (detection/backfill time).
    /// </summary>
    public DateTime TrackDate { get; set; }

    /// <summary>
    /// True when the takeover was undone: the winning effort disappeared
    /// (activity flagged / deleted / set private) and the loser got the KOM back
    /// (ReturnedKom). audit_md holds when it was marked.
    /// </summary>
    public bool Reverted { get; set; }
}

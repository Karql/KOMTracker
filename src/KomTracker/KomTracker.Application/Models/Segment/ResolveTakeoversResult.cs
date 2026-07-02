using KomTracker.Domain.Entities.Segment;
using System.Collections.Generic;

namespace KomTracker.Application.Models.Segment;

public class ResolveTakeoversResult
{
    public List<KomTakeoverEntity> NewTakeovers { get; set; } = new();

    /// <summary>Ids of existing takeovers to mark as reverted.</summary>
    public List<int> RevertedTakeoverIds { get; set; } = new();
}

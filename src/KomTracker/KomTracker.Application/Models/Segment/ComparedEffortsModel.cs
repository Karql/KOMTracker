using KomTracker.Domain.Entities.Segment;
using System.Collections.Generic;
using System.Linq;

namespace KomTracker.Application.Models.Segment;

public class ComparedEffortsModel
{
    public int KomsCount { get; set; } = 0;
    public int NewKomsCount { get; set; } = 0;
    public int ImprovedKomsCount { get; set; } = 0;
    public int LostKomsCount { get; set; } = 0;

    public List<EffortModel> Efforts { get; set; } = new List<EffortModel>();

    public bool AnyChanges => NewKomsCount > 0 || ImprovedKomsCount > 0 || LostKomsCount > 0;
}

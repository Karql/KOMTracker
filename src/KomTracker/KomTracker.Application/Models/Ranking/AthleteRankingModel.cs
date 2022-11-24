using KomTracker.Application.Shared.Models.Segment;
using KomTracker.Domain.Entities.Athlete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Models.Ranking;
public class AthleteRankingModel
{
    public AthleteEntity Athlete { get; set; } = default!;

    public AthleteRankingTotalModel Total { get; set; } = new();
}

public class AthleteRankingTotalModel
{
    public int KomsCount { get; set; }
    public Dictionary<ExtendedCategoryEnum, int> KomsCountByCategory { get; set; } = new();
}
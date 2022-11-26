using KomTracker.API.Shared.ViewModels.Athlete;
using KomTracker.Application.Shared.Models.Segment;

namespace KomTracker.API.Shared.ViewModels.Ranking;
public class AthleteRankingViewModel
{
    public AthleteViewModel Athlete { get; set; } = default!;

    public AthleteRankingTotalViewModel Total { get; set; } = new();

    public AthleteRankingKomsChangesViewModel KomsChanges30Days { get; set; } = new();
    public AthleteRankingKomsChangesViewModel KomsChangesLastWeek { get; set; } = new();
    public AthleteRankingKomsChangesViewModel KomsChangesThisWeek { get; set; } = new();
}

public class AthleteRankingTotalViewModel
{
    public int KomsCount { get; set; }

    public Dictionary<ExtendedCategoryEnum, int> KomsCountByCategory { get; set; } = new();

    public int GetKomsCountByCategory(ExtendedCategoryEnum cat)
    {
        return KomsCountByCategory.TryGetValue(cat, out var count) ? count : 0;
    }
}

public class AthleteRankingKomsChangesViewModel
{
    public int NewKomsCount { get; set; }
    public int LostKomsCount { get; set; }
}
using KomTracker.API.Shared.ViewModels.Athlete;

namespace KomTracker.API.Shared.ViewModels.Ranking;
public class AthleteRankingViewModel
{
    public AthleteViewModel Athlete { get; set; } = default!;

    public AthleteRankingTotalViewModel Total { get; set; } = new();
}

public class AthleteRankingTotalViewModel
{
    public int KomsCount { get; set; }
}
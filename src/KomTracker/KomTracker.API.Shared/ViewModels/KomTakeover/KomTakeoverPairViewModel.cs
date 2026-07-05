using KomTracker.API.Shared.ViewModels.Athlete;

namespace KomTracker.API.Shared.ViewModels.KomTakeover;

public class KomTakeoverPairViewModel
{
    public AthleteViewModel WinnerAthlete { get; set; } = default!;
    public int WinnerKoms { get; set; }
    public int LoserKoms { get; set; }
    public AthleteViewModel LoserAthlete { get; set; } = default!;
}

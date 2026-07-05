using KomTracker.Domain.Entities.Athlete;

namespace KomTracker.Application.Models.Segment;

/// <summary>
/// A head-to-head KOM-takeover summary between two athletes, oriented so the winner
/// (more takeovers) is on the left. WinnerKoms = how many the winner took from the loser;
/// LoserKoms = the reverse direction.
/// </summary>
public class KomTakeoverPairModel
{
    public AthleteEntity WinnerAthlete { get; set; } = default!;
    public int WinnerKoms { get; set; }
    public int LoserKoms { get; set; }
    public AthleteEntity LoserAthlete { get; set; } = default!;
}

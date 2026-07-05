namespace KomTracker.Application.Models.Segment;

/// <summary>Directed takeover count from the DB: how many times TakenBy took a KOM from LostBy.</summary>
public class KomTakeoverCountModel
{
    public int TakenByAthleteId { get; set; }
    public int LostByAthleteId { get; set; }
    public int Count { get; set; }
}

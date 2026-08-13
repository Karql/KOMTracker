namespace KomTracker.API.Shared.ViewModels.BikeTracker;

/// <summary>Request body for linking a Strava bike to an existing bt.bike.</summary>
public class LinkStravaBikeViewModel
{
    public int BikeId { get; set; }
}

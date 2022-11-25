using KomTracker.Application.Shared.Models.Segment;
using MudBlazor;

namespace KomTracker.WEB.Helpers;

public static class ViewHelper
{
    public static string GetActivityTypeIcon(string activityType)
    {
        return activityType switch
        {
            "Ride" => Icons.Material.Filled.DirectionsBike,
            "Run" => Icons.Material.Filled.DirectionsRun,
            "Hike" => Icons.Material.Filled.Hiking,
            "NordicSki" => Icons.Material.Filled.NordicWalking,
            _ => Icons.Material.Filled.HelpCenter
        };
    }

    public static string GetExtendedCategoryColor(ExtendedCategoryEnum extendedCategory)
    {
        return extendedCategory switch
        {
            ExtendedCategoryEnum.D1 => "#000",
            ExtendedCategoryEnum.D2 => "#000",
            ExtendedCategoryEnum.SP => "#00d",
            ExtendedCategoryEnum.FL => "#0b0",
            ExtendedCategoryEnum.TTS => "#0b0",
            ExtendedCategoryEnum.TTL => "#0b0",
            ExtendedCategoryEnum.MC => "#db0",
            ExtendedCategoryEnum.WL => "#f40",
            ExtendedCategoryEnum.C4 => "#EB9138",
            ExtendedCategoryEnum.C3 => "#E47B34",
            ExtendedCategoryEnum.C2 => "#DC6531",
            ExtendedCategoryEnum.C1 => "#D34B2D",
            ExtendedCategoryEnum.HC => "#CA2A2A",
            _ => throw new ArgumentOutOfRangeException($"{nameof(extendedCategory)} should has value between -8 and 5 (without 0)"),
        };
    }

    public static string GetClubAvatar(string url)
    {
        return url.StartsWith("http") ?
            url
            : "/img/avatar-club-medium.png";
    }
}

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

    public static string GetClimbCategoryColor(int climbCategory)
    {
        return climbCategory switch
        {
            -1 => "#000",
            0 => "#F3A73B",
            1 => "#EB9138",
            2 => "#E47B34",
            3 => "#DC6531",
            4 => "#D34B2D",
            5 => "#CA2A2A",
            _ => throw new ArgumentOutOfRangeException($"{nameof(climbCategory)} should has value between -1 and 5"),
        };
    }
}

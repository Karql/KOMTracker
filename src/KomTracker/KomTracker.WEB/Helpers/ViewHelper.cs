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

    public static string GetExtendedCategoryColor(int climbCategory)
    {
        return climbCategory switch
        {
            -8 => "#000",
            -7 => "#000",
            -6 => "#00d",
            -5 => "#0b0",
            -4 => "#0b0",
            -3 => "#0b0",
            -2 => "#db0",
            -1 => "#f40",
            1 => "#EB9138",
            2 => "#E47B34",
            3 => "#DC6531",
            4 => "#D34B2D",
            5 => "#CA2A2A",
            _ => throw new ArgumentOutOfRangeException($"{nameof(climbCategory)} should has value between -8 and 5 (without 0)"),
        };
    }
}

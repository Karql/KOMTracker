using System.Globalization;
using KomTracker.API.Shared.ViewModels.ServiceRecord;
using KomTracker.Domain.Entities.Component;
using KomTracker.WEB.Models.Component;
using MudBlazor;

namespace KomTracker.WEB.Models.ServiceRecord;

/// <summary>A pickable service target in the add/edit dialog — one of the user's bikes or components.</summary>
public sealed record ServiceTargetOption(int? BikeId, int? ComponentId, string Name, ComponentCategory? Category, string Caption, bool IsActive)
{
    /// <summary>Built once (the category icon is a composed SVG string), not on every render of the suggestion list.</summary>
    public string Icon { get; } = ServiceTargetDisplay.Icon(BikeId, Category);
}

/// <summary>Shared rendering bits for service targets (icon, mileage-at-service text).</summary>
public static class ServiceTargetDisplay
{
    public static string Icon(int? bikeId, ComponentCategory? category)
        => bikeId is not null ? Icons.Material.Filled.PedalBike : ComponentCategoryIcons.Icon(category ?? ComponentCategory.Other);

    public static string Icon(ServiceTargetViewModel target) => Icon(target.BikeId, target.Category);

    public static string Href(ServiceTargetViewModel target)
        => target.BikeId is not null ? $"bikes/{target.BikeId}" : $"components/{target.ComponentId}";

    /// <summary>Mileage at the service date — distance · moving time · elevation, same format as the installation tables.</summary>
    public static string Mileage(ServiceTargetViewModel? target)
    {
        if (target?.DistanceKm is null)
        {
            return "—";
        }

        var parts = new List<string>();
        if (target.DistanceKm > 0) parts.Add($"{target.DistanceKm.Value.ToString("N0", CultureInfo.InvariantCulture)} km");
        if (target.MovingHours > 0) parts.Add($"{target.MovingHours.Value.ToString("N1", CultureInfo.InvariantCulture)} h");
        if (target.ElevationM > 0) parts.Add($"{target.ElevationM.Value.ToString("N0", CultureInfo.InvariantCulture)} m");
        return parts.Count == 0 ? "0 km" : string.Join(" · ", parts);
    }

    /// <summary>"yyyy-MM-dd", plus the time when it isn't midnight.</summary>
    public static string Date(DateTime date)
        => date.TimeOfDay == TimeSpan.Zero ? date.ToString("yyyy-MM-dd") : date.ToString("yyyy-MM-dd HH:mm");
}

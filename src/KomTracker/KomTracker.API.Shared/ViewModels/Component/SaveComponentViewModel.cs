using System;
using KomTracker.Domain.Entities.Component;

namespace KomTracker.API.Shared.ViewModels.Component;

/// <summary>
/// Request body for creating/updating a component (add + edit share this shape; WEB form binds to it).
/// Field names mirror <c>SaveComponentCommand</c> (guarded by a parity test). Server-owned fields
/// (id/owner/lifecycle/audit) are not here.
/// </summary>
public class SaveComponentViewModel
{
    public string Name { get; set; } = default!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public ComponentCategory Category { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Notes { get; set; }
    public decimal? Price { get; set; }
    public string? PurchasePlace { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal InitialDistanceKm { get; set; }
    public decimal? InitialMovingHours { get; set; }
    public decimal? InitialElevationM { get; set; }
    public int? WarehouseId { get; set; }
}

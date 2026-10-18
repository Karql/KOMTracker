using System;
using System.Collections.Generic;
using KomTracker.Domain.Entities.Component;

namespace KomTracker.API.Shared.ViewModels.ServiceRecord;

/// <summary>A service (maintenance record) with what it covered.</summary>
public class ServiceRecordViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public DateTime Date { get; set; }
    public string? Place { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public List<ServiceTargetViewModel> Targets { get; set; } = new();
}

/// <summary>One bike XOR component a service covered, with its mileage at the service date (stored projection).</summary>
public class ServiceTargetViewModel
{
    public int? BikeId { get; set; }
    public int? ComponentId { get; set; }
    public string? Name { get; set; }

    /// <summary>The component's category (null for a bike).</summary>
    public ComponentCategory? Category { get; set; }

    /// <summary>Mileage at the service date; null until computed.</summary>
    public decimal? DistanceKm { get; set; }
    public decimal? MovingHours { get; set; }
    public decimal? ElevationM { get; set; }
}

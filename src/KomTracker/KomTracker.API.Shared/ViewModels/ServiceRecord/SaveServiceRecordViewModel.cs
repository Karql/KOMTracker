using System;
using System.Collections.Generic;
using KomTracker.Domain.Entities.ServiceRecord;

namespace KomTracker.API.Shared.ViewModels.ServiceRecord;

/// <summary>Create/update body for a service (editable fields only). <see cref="Targets"/> replaces the whole set.</summary>
public class SaveServiceRecordViewModel
{
    public string Name { get; set; } = default!;
    public DateTime Date { get; set; }
    public string? Place { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public List<ServiceTargetRef> Targets { get; set; } = new();
}

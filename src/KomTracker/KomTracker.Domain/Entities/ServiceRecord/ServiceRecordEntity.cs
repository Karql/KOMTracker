#nullable enable
using System;
using System.Collections.Generic;
using KomTracker.Domain.Contracts;

namespace KomTracker.Domain.Entities.ServiceRecord;

/// <summary>
/// A maintenance / repair record (BikeTracker "Services") — a bike check-up, a shock service, a wheel rebuild… One
/// service covers <b>N targets</b> (bikes and/or components, D-SRV-1) and is never propagated along the install chain
/// (D-17 / D-SRV-2). Owned by the platform User. Table: bt.service. No lifecycle. Dates are UTC (timestamptz).
/// <para>Named <c>ServiceRecord</c> in code so it doesn't clash with application "services".</para>
/// </summary>
public class ServiceRecordEntity : BaseEntity
{
    public int Id { get; set; }

    /// <summary>Owner — FK to the identity user (AspNetUsers.Id). Scoping key.</summary>
    public string UserId { get; set; } = default!;

    public string Name { get; set; } = default!;

    /// <summary>When the service happened (date + optional time-of-day, like install dates). The cut-off for the targets' mileage.</summary>
    public DateTime Date { get; set; }

    /// <summary>Where it was done (free text, shared with the place suggestions).</summary>
    public string? Place { get; set; }

    /// <summary>What it cost for the whole visit (not split per target, D-SRV-7); null = unknown.</summary>
    public decimal? Cost { get; set; }

    public string? Notes { get; set; }

    /// <summary>What the service covered (at least one).</summary>
    public List<ServiceTargetEntity> Targets { get; set; } = new();
}

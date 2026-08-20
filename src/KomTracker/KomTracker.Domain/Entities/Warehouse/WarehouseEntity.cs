#nullable enable
using KomTracker.Domain.Contracts;

namespace KomTracker.Domain.Entities.Warehouse;

/// <summary>
/// A place that holds non-installed components (Home, Garage, a drawer…). Owned by the platform User.
/// Table: bt.warehouse. Minimal by design — just a name; a component points at its current warehouse.
/// </summary>
public class WarehouseEntity : BaseEntity
{
    public int Id { get; set; }

    /// <summary>Owner — FK to the identity user (AspNetUsers.Id). Scoping key.</summary>
    public string UserId { get; set; } = default!;

    public string Name { get; set; } = default!;
}

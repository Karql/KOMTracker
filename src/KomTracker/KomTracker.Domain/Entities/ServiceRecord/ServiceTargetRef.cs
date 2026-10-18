#nullable enable
namespace KomTracker.Domain.Entities.ServiceRecord;

/// <summary>
/// A reference to what a service covers — exactly one of <see cref="BikeId"/> / <see cref="ComponentId"/>. Shared by
/// the request body (API.Shared) and the command, like the domain enums, so both carry the same type.
/// </summary>
public class ServiceTargetRef
{
    public int? BikeId { get; set; }

    public int? ComponentId { get; set; }
}

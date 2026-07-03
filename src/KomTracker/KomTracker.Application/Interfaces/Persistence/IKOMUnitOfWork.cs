using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence;

public interface IKOMUnitOfWork : IUnitOfWork
{
    /// <summary>
    /// Detach all tracked entities. Used to give each athlete a clean slate in the
    /// long-lived per-run DbContext so residue from a failed athlete does not leak into the next.
    /// </summary>
    void ClearChangeTracker();
}

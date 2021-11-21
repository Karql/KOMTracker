using KomTracker.Application.Interfaces.Persistence;
using Utils.UnitOfWork.Concrete;

namespace KomTracker.Infrastructure.Persistence;

public class EFKOMUnitOfWork : EFUnitOfWork<KOMDBContext>, IKOMUnitOfWork
{
    public EFKOMUnitOfWork(KOMDBContext dbContext) : base(dbContext)
    {
    }
}

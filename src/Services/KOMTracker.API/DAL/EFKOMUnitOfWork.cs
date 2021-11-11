using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.UnitOfWork.Concrete;

namespace KOMTracker.API.DAL;

public class EFKOMUnitOfWork : EFUnitOfWork<KOMDBContext>, IKOMUnitOfWork
{
    public EFKOMUnitOfWork(KOMDBContext dbContext) : base(dbContext)
    {
    }
}

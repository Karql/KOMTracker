using KomTracker.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.UnitOfWork.Concrete;
using static MoreLinq.Extensions.ForEachExtension;

namespace KomTracker.Infrastructure.Persistence.Repositories;
public abstract class EFBaseRepository : EFRepositoryBase<KOMDBContext>
{
    protected void SetAuidtCD(IEnumerable<BaseEntity> entities)
    {
        entities.ForEach(x => x.AuditCD = DateTime.UtcNow);
    }

    protected void SetAuidtMD(IEnumerable<BaseEntity> entities)
    {
        entities.ForEach(x => x.AuditMD = DateTime.UtcNow);
    }
}

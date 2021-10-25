using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.UnitOfWork.Abstract;

namespace Utils.UnitOfWork.Concrete
{
    public abstract class EFRepositoryBase<TContext> : IRepository
    {
        protected TContext _context;

        internal void SetContext(TContext context)
        {
            _context = context;
        }
    }
}

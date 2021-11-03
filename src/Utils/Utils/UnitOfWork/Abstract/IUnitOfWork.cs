using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.UnitOfWork.Abstract
{
    public interface IUnitOfWork : IDisposable
    { 
        Task<int> SaveChangesAsync();

        public TRepository GetRepository<TRepository>() where TRepository : class, IRepository;
    }
}

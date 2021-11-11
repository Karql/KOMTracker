using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.UnitOfWork.Abstract;

namespace Utils.UnitOfWork.Concrete;

public class EFUnitOfWork<TContext> : IUnitOfWork
    where TContext : DbContext
{
    private bool disposed = false;
    protected TContext _context;
    protected Dictionary<Type, IRepository> _repositories = new Dictionary<Type, IRepository>();

    public EFUnitOfWork(TContext dbContext)
    {
        _context = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public TRepository GetRepository<TRepository>()
        where TRepository : class, IRepository
    {
        var repoType = typeof(TRepository);

        if (!_repositories.ContainsKey(repoType))
        {
            var repo = _context.GetService<TRepository>();

            if (!(repo is EFRepositoryBase<TContext>))
            {
                throw new Exception($"{repo.GetType()} is not EFRepositoryBase<TContext>");
            }

            var repoCasted = repo as EFRepositoryBase<TContext>;
            repoCasted.SetContext(_context);

            _repositories[repoType] = repo;

        }

        return (TRepository)_repositories[repoType];
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                if (_repositories != null)
                {
                    _repositories.Clear();
                }

                _context.Dispose();
            }
        }

        disposed = true;
    }
}

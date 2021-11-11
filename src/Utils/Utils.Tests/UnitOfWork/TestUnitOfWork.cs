using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.UnitOfWork.Abstract;

namespace Utils.Tests.UnitOfWork;

public class TestUnitOfWork : IUnitOfWork
{
    private readonly IUnitOfWork _mockUnitOfWork;
    private readonly IDictionary<Type, IRepository> _repos;

    public TestUnitOfWork()
    {
        _mockUnitOfWork = Substitute.For<IUnitOfWork>();
    }

    public TestUnitOfWork(IDictionary<Type, IRepository> repos)
        : base()
    {
        _repos = repos;
    }

    public void Dispose()
    {
        ;
    }

    public TRepository GetRepository<TRepository>()
        where TRepository : class, IRepository
    {
        if (_repos.TryGetValue(typeof(TRepository), out IRepository repo))
        {
            return (TRepository)repo;
        }

        return Substitute.For<TRepository>();
    }

    public Task<int> SaveChangesAsync()
    {
        return _mockUnitOfWork.SaveChangesAsync();
    }
}

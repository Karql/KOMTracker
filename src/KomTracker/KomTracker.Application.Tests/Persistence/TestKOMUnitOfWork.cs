using KomTracker.Application.Interfaces.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Tests.UnitOfWork;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Tests.Persistence;

public class TestKOMUnitOfWork : TestUnitOfWork, IKOMUnitOfWork
{
    public TestKOMUnitOfWork(IDictionary<Type, IRepository> repos) : base(repos)
    {
    }
}

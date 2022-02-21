using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Domain.Contracts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Utils.UnitOfWork.Concrete;

namespace KomTracker.Infrastructure.Persistence;

public class EFKOMUnitOfWork : EFUnitOfWork<KOMDBContext>, IKOMUnitOfWork
{
    public EFKOMUnitOfWork(KOMDBContext dbContext) : base(dbContext)
    {
    }

    public override Task<int> SaveChangesAsync()
    {
        foreach (var entry in _context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }


        return base.SaveChangesAsync();
    }
}

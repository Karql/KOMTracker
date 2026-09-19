#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Component;
using Microsoft.EntityFrameworkCore;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFComponentMileageRepository : EFBaseRepository, IComponentMileageRepository
{
    public Task<ComponentMileageEntity?> GetAsync(int componentId)
        => _context.ComponentMileage.AsNoTracking().FirstOrDefaultAsync(x => x.ComponentId == componentId);

    public async Task<IEnumerable<ComponentMileageEntity>> GetByComponentIdsAsync(IReadOnlyCollection<int> componentIds)
    {
        if (componentIds is null || componentIds.Count == 0)
        {
            return Enumerable.Empty<ComponentMileageEntity>();
        }

        return await _context.ComponentMileage.AsNoTracking()
            .Where(x => componentIds.Contains(x.ComponentId))
            .ToListAsync();
    }

    // Insert-or-replace the whole row. FlexLabs upsert (bypasses the change tracker), so audit is stamped by hand.
    public Task UpsertAsync(ComponentMileageEntity mileage)
    {
        mileage.AuditCD = DateTime.UtcNow;

        return _context.ComponentMileage
            .Upsert(mileage)
            .WhenMatched((db, incoming) => new ComponentMileageEntity
            {
                AuditMD = DateTime.UtcNow,
                TotalDistanceKm = incoming.TotalDistanceKm,
                TotalMovingHours = incoming.TotalMovingHours,
                TotalElevationM = incoming.TotalElevationM,
                AttributedActivityCount = incoming.AttributedActivityCount,
                ComputedAt = incoming.ComputedAt
            })
            .RunAsync();
    }
}

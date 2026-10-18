#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.ServiceRecord;
using Microsoft.EntityFrameworkCore;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFServiceRecordRepository : EFBaseRepository, IServiceRecordRepository
{
    public async Task<IEnumerable<ServiceRecordEntity>> GetServiceRecordsAsync(string userId)
    {
        return await _context.ServiceRecord
            .AsNoTracking()
            .Include(x => x.Targets)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<ServiceRecordEntity?> GetServiceRecordAsync(int id)
    {
        return await _context.ServiceRecord
            .Include(x => x.Targets)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IEnumerable<ServiceRecordEntity>> GetByBikeAsync(int bikeId)
    {
        return await _context.ServiceRecord
            .AsNoTracking()
            .Include(x => x.Targets)
            .Where(x => x.Targets.Any(t => t.BikeId == bikeId))
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceRecordEntity>> GetByComponentAsync(int componentId)
    {
        return await _context.ServiceRecord
            .AsNoTracking()
            .Include(x => x.Targets)
            .Where(x => x.Targets.Any(t => t.ComponentId == componentId))
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Id)
            .ToListAsync();
    }

    public void AddServiceRecord(ServiceRecordEntity service)
    {
        _context.ServiceRecord.Add(service);
    }

    public void DeleteServiceRecord(ServiceRecordEntity service)
    {
        _context.ServiceRecord.Remove(service);
    }

    public Task<bool> AnyByBikeAsync(int bikeId)
        => _context.ServiceTarget.AnyAsync(x => x.BikeId == bikeId);

    public Task<bool> AnyByComponentAsync(int componentId)
        => _context.ServiceTarget.AnyAsync(x => x.ComponentId == componentId);

    public async Task<IReadOnlyList<ServiceTargetEntity>> GetTargetsForRecalculationAsync(
        IReadOnlyCollection<int> serviceIds, IReadOnlyCollection<int> componentIds, IReadOnlyCollection<int> bikeIds)
    {
        return await _context.ServiceTarget
            .Include(x => x.Service)
            .Where(x => serviceIds.Contains(x.ServiceId)
                || (x.ComponentId != null && componentIds.Contains(x.ComponentId.Value))
                || (x.BikeId != null && bikeIds.Contains(x.BikeId.Value)))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ServiceTargetEntity>> GetAllTargetsAsync()
    {
        return await _context.ServiceTarget
            .Include(x => x.Service)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<string>> GetDistinctPlacesAsync(string userId)
    {
        return await _context.ServiceRecord
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Place != null && x.Place != "")
            .Select(x => x.Place!)
            .Distinct()
            .ToListAsync();
    }
}

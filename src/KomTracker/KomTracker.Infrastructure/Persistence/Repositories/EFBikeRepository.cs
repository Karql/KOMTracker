#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Bike;
using Microsoft.EntityFrameworkCore;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFBikeRepository : EFBaseRepository, IBikeRepository
{
    public async Task<IEnumerable<BikeEntity>> GetBikesAsync(string userId, bool includeInactive)
    {
        var query = _context.Bike
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (!includeInactive)
        {
            query = query.Where(x => x.Lifecycle == BikeLifecycle.Active);
        }

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<BikeEntity?> GetBikeAsync(int id)
    {
        return await _context.Bike.FirstOrDefaultAsync(x => x.Id == id);
    }

    public void AddBike(BikeEntity bike)
    {
        _context.Bike.Add(bike);
    }

    public void UpdateBike(BikeEntity bike)
    {
        _context.Bike.Update(bike);
    }

    public void DeleteBike(BikeEntity bike)
    {
        _context.Bike.Remove(bike);
    }
}

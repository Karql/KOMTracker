using KomTracker.Domain.Entities.Bike;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IBikeRepository : IRepository
{
    Task<IEnumerable<BikeEntity>> GetBikesAsync(string userId, bool includeInactive);
    Task<BikeEntity?> GetBikeAsync(int id);
    void AddBike(BikeEntity bike);
    void UpdateBike(BikeEntity bike);
    void DeleteBike(BikeEntity bike);
}

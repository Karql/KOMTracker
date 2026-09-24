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

    /// <summary>DB-side DISTINCT of the non-empty Brand / Model / Purchase place values for the user's bikes (autocomplete hints).</summary>
    Task<(IReadOnlyList<string> Brands, IReadOnlyList<string> Models, IReadOnlyList<string> PurchasePlaces)> GetDistinctPurchaseFieldsAsync(string userId);
}

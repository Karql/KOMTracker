using KomTracker.Domain.Entities.Club;

namespace KomTracker.Application.Services;
public interface IClubService
{
    Task AddOrUpdateClubsAsync(IEnumerable<ClubEntity> clubs);
}

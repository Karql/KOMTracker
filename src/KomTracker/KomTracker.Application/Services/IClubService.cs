using KomTracker.Domain.Entities.Club;

namespace KomTracker.Application.Services;
public interface IClubService
{
    Task AddOrUpdateClubsAsync(IEnumerable<ClubEntity> clubs);
    Task SyncAthleteClubsAsync(int athleteId, IEnumerable<ClubEntity> clubs);
    Task<IEnumerable<ClubEntity>> GetAthleteClubsAsync(int athleteId);
}

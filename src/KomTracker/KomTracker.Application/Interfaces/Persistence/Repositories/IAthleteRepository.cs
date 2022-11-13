using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Club;
using KomTracker.Domain.Entities.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IAthleteRepository : IRepository
{
    Task<bool> IsAthleteExistsAsync(int athleteId);
    Task<AthleteEntity> GetAthleteAsync(int athleteId);
    Task AddOrUpdateAthleteAsync(AthleteEntity athlete);
    Task DeactivateAthleteAsync(int athleteId);
    Task<TokenEntity> GetTokenAsync(int athleteId);
    Task<IEnumerable<AthleteEntity>> GetAllAthletesAsync();
    Task AddOrUpdateTokenAsync(TokenEntity token);
    Task SyncAthleteClubsAsync(int athleteId, IEnumerable<ClubEntity> clubs);
}

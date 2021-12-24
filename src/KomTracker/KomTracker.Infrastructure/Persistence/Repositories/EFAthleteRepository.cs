using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Token;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.UnitOfWork.Concrete;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFAthleteRepository : EFRepositoryBase<KOMDBContext>, IAthleteRepository
{
    public async Task<bool> IsAthleteExistsAsync(int athleteId)
    {
        return await _context.Athlete.AnyAsync(x => x.AthleteId == athleteId);
    }

    public async Task<AthleteEntity> GetAthleteAsync(int athleteId)
    {
        return await _context.Athlete.FirstOrDefaultAsync(x => x.AthleteId == athleteId);
    }

    public Task AddOrUpdateAthleteAsync(AthleteEntity athlete)
    {
        return _context
            .Athlete
            .Upsert(athlete)
            .RunAsync();
    }

    public Task DeactivateAthleteAsync(int athleteId)
    {
        // TODO
        return Task.CompletedTask;
    }

    public Task<TokenEntity> GetTokenAsync(int athleteId)
    {
        return _context.Token.FirstOrDefaultAsync(x => x.AthleteId == athleteId);
    }

    public async Task<IEnumerable<AthleteEntity>> GetAllAthletesAsync()
    {
        // TODO: only active
        return await _context.Athlete.ToListAsync();
    }

    public Task AddOrUpdateTokenAsync(TokenEntity token)
    {
        return _context
            .Token
            .Upsert(token)
            .RunAsync();
    }
}

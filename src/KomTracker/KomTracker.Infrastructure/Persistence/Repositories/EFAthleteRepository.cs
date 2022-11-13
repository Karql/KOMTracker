using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Club;
using KomTracker.Domain.Entities.Token;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.UnitOfWork.Concrete;
using static MoreLinq.Extensions.FullGroupJoinExtension;
using static MoreLinq.Extensions.ForEachExtension;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFAthleteRepository : EFBaseRepository, IAthleteRepository
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
        // TODO: find cleaner way
        athlete.AuditCD = DateTime.UtcNow;

        return _context
            .Athlete
            .Upsert(athlete)
            .WhenMatched((db, model) => new AthleteEntity
            {
                AuditMD = DateTime.UtcNow,
                Username = model.Username,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Bio = model.Bio,
                City = model.City,
                Country = model.Country,
                Sex = model.Sex,
                Weight = model.Weight,
                Profile = model.Profile,
                ProfileMedium = model.ProfileMedium
            })
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
        // TODO: find cleaner way
        token.AuditCD = DateTime.UtcNow;

        return _context
            .Token
            .Upsert(token)
            .WhenMatched((db, model) => new TokenEntity
            {
                AuditMD = DateTime.UtcNow,
                TokenType = model.TokenType,
                ExpiresAt = model.ExpiresAt,
                AccessToken = model.AccessToken,
                RefreshToken = model.RefreshToken,
                Scope = model.Scope
            })
            .RunAsync();
    }

    public async Task SyncAthleteClubsAsync(int athleteId, IEnumerable<ClubEntity> clubs)
    {
        var dbClubs = await _context.AthleteClub.Where(x => x.AthleteId == athleteId).ToListAsync();

        dbClubs.FullGroupJoin(clubs,
            x => x.ClubId,
            y => y.Id,
            (key, x, y) => new { dbClub = x.FirstOrDefault(), club = y.FirstOrDefault() }
        ).ForEach(x =>
        {
            if (x.dbClub == null)
            {
                _context.AthleteClub.Add(new AthleteClubEntity
                {
                    AthleteId = athleteId,
                    ClubId = x.club.Id
                });
            }

            else if (x.club == null)
            {
                _context.AthleteClub.Remove(x.dbClub);
            }
        });
    }
}
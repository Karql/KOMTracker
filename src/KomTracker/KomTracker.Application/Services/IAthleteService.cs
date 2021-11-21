using FluentResults;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Segment;
using KomTracker.Domain.Entities.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Application.Services;

public interface IAthleteService
{
    Task<bool> IsAthleteExistsAsync(int athleteId);

    /// <summary>
    /// Add or update athlete in DB
    /// </summary>
    /// <param name="athlete"></param>
    /// <returns></returns>
    Task AddOrUpdateAthleteAsync(AthleteEntity athlete);

    /// <summary>
    /// Add or update token in DB
    /// </summary>
    Task AddOrUpdateTokenAsync(TokenEntity token);

    /// <summary>
    /// Get all athletes from DB
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<AthleteEntity>> GetAllAthletesAsync();

    /// <summary>
    /// Get token from DB and refresh when needed
    /// <summary>
    Task<Result<TokenEntity>> GetValidTokenAsync(int athleteId);
}

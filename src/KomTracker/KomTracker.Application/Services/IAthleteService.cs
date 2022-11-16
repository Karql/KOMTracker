using FluentResults;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Club;
using KomTracker.Domain.Entities.Segment;
using KomTracker.Domain.Entities.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomTracker.Application.Services;

public interface IAthleteService
{
    /// <summary>
    /// Check if athlete exists in DB
    /// </summary>
    Task<bool> IsAthleteExistsAsync(int athleteId);

    /// <summary>
    /// Get athlete from DB
    /// </summary>
    Task<AthleteEntity> GetAthleteAsync(int athleteId);

    /// <summary>
    /// Add or update athlete in DB
    /// </summary>
    Task AddOrUpdateAthleteAsync(AthleteEntity athlete);

    /// <summary>
    /// Add or update token in DB
    /// </summary>
    Task AddOrUpdateTokenAsync(TokenEntity token);

    /// <summary>
    /// Get all athletes from DB
    /// </summary>
    Task<IEnumerable<AthleteEntity>> GetAllAthletesAsync();

    /// <summary>
    /// Get athletes by club from DB
    /// </summary>
    Task<IEnumerable<AthleteEntity>> GetAthletesByClubAsync(long clubId);

    /// <summary>
    /// Get token from DB and refresh when needed
    /// </summary>
    Task<Result<TokenEntity>> GetValidTokenAsync(int athleteId);
}

public class GetValidTokenError : FluentResults.Error
{
    public const string NoTokenInDB = "No token in DB!";
    public const string RefreshFailed = "Refresh failed!";

    public GetValidTokenError(string message)
        : base(message)
    {
    }
}
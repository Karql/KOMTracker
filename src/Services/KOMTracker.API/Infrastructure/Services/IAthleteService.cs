using FluentResults;
using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Segment;
using KOMTracker.API.Models.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.Infrastructure.Services
{
    public interface IAthleteService
    {
        Task<bool> IsAthleteExistsAsync(int athleteId);

        /// <summary>
        /// Add or update athlete in DB
        /// </summary>
        /// <param name="athlete"></param>
        /// <returns></returns>
        Task AddOrUpdateAthleteAsync(AthleteModel athlete);

        /// <summary>
        /// Add or update token in DB
        /// </summary>
        Task AddOrUpdateTokenAsync(TokenModel token);

        /// <summary>
        /// Get all athletes from DB
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<AthleteModel>> GetAllAthletesAsync();

        /// <summary>
        /// Get token from DB and refresh when needed
        /// <summary>
        Task<Result<TokenModel>> GetValidTokenAsync(int athleteId);

        /// <summary>
        /// Get actual koms from API
        /// </summary>
        Task<Result<IEnumerable<(SegmentEffortModel, SegmentModel)>>> GetAthleteKomsAsync(int athleteId, string token);
    }
}

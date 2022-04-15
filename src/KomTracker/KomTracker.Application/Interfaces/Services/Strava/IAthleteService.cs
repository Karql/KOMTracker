using FluentResults;
using KomTracker.Domain.Entities.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Interfaces.Services.Strava;

public interface IAthleteService
{
    /// <summary>
    /// Get actual koms from API
    /// </summary>
    Task<Result<IEnumerable<(SegmentEffortEntity, SegmentEntity)>>> GetAthleteKomsAsync(int athleteId, string token);
}

public class GetAthleteKomsError : FluentResults.Error
{
    public const string Unauthorized = "Unauthorized";
    public const string UnknownError = "UnknownError";

    public GetAthleteKomsError(string message)
        : base(message)
    {
    }
}

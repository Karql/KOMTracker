using FluentResults;
using KomTracker.Domain.Entities.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Interfaces.Services.Strava;

public interface ISegmentService
{
    /// <summary>
    /// Get segment details from API
    /// </summary>
    Task<Result<SegmentEntity>> GetSegmentAsync(long id, string token);
}

public class GetSegmentError : FluentResults.Error
{
    public const string Unauthorized = "Unauthorized";
    public const string TooManyRequests = "TooManyRequests";
    public const string NotFound = "NotFound";
    public const string UnknownError = "UnknownError";

    public GetSegmentError(string message)
    : base(message)
    {
    }
}


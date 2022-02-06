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

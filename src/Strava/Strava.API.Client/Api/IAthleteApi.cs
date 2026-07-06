using FluentResults;
using Strava.API.Client.Model.Athlete;
using Strava.API.Client.Model.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Api;

public interface IAthleteApi
{
    Task<Result<IEnumerable<SegmentEffortDetailedModel>>> GetKomsAsync(int athleteId, string token);

    /// <summary>Get the profile of the athlete that owns the token (Strava GET /athlete).</summary>
    Task<Result<AthleteSummaryModel>> GetAthleteAsync(string token);
}

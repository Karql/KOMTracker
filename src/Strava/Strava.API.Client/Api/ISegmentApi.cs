using FluentResults;
using Strava.API.Client.Model.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Api;

public interface ISegmentApi
{
    public Task<Result<SegmentDetailedModel>> GetSegmentAsync(long id, string token);
}

using AutoMapper;
using FluentResults;
using KomTracker.Application.Interfaces.Services.Strava;
using KomTracker.Domain.Entities.Segment;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApiModel = Strava.API.Client.Model;

namespace KomTracker.Infrastructure.Strava.Services;

public class AthleteService : IAthleteService
{
    private readonly IMapper _mapper;
    private readonly IAthleteApi _athleteApi;

    public AthleteService(IMapper mapper, IAthleteApi athleteApi)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _athleteApi = athleteApi ?? throw new ArgumentNullException(nameof(athleteApi));
    }

    public async Task<Result<IEnumerable<(SegmentEffortEntity, SegmentEntity)>>> GetAthleteKomsAsync(int athleteId, string token)
    {
        var getKomsRes = await _athleteApi.GetKomsAsync(athleteId, token);

        if (getKomsRes.IsSuccess)
        {
            return Result.Ok(getKomsRes.Value
                .Where(x => !x.Segment.Private)
                .Select(x => (
                    _mapper.Map<SegmentEffortEntity>(x),
                    _mapper.Map<SegmentEntity>(x.Segment)
                ))
                .ToList()
                .AsEnumerable()
            );
        }

        if (getKomsRes.HasError<ApiModel.Segment.Error.GetKomsError>(x => x.Message == ApiModel.Segment.Error.GetKomsError.Unauthorized))
        {
            return Result.Fail<IEnumerable<(SegmentEffortEntity, SegmentEntity)>>(new GetAthleteKomsError(GetAthleteKomsError.Unauthorized));
        }

        return Result.Fail<IEnumerable<(SegmentEffortEntity, SegmentEntity)>>(new GetAthleteKomsError(GetAthleteKomsError.UnknownError));
    }
}

using AutoMapper;
using FluentResults;
using KomTracker.Application.Interfaces.Services.Strava;
using KomTracker.Domain.Entities.Club;
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
    private readonly IClubApi _clubApi;

    public AthleteService(IMapper mapper, IAthleteApi athleteApi, IClubApi clubApi)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _athleteApi = athleteApi ?? throw new ArgumentNullException(nameof(athleteApi));
        _clubApi = clubApi ?? throw new ArgumentNullException(nameof(clubApi));
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

    public async Task<Result<IEnumerable<ClubEntity>>> GetAthleteClubsAsync(string token)
    {
        var getKomsRes = await _clubApi.GetClubsAsync(token);

        if (getKomsRes.IsSuccess)
        {
            return Result.Ok(getKomsRes.Value
                .Select(x => _mapper.Map<ClubEntity>(x))
                .ToList()
                .AsEnumerable()
            );
        }

        if (getKomsRes.HasError<ApiModel.Club.Error.GetClubsError>(x => x.Message == ApiModel.Club.Error.GetClubsError.Unauthorized))
        {
            return Result.Fail<IEnumerable<ClubEntity>>(new GetAthleteClubsError(GetAthleteClubsError.Unauthorized));
        }

        return Result.Fail<IEnumerable<ClubEntity>>(new GetAthleteClubsError(GetAthleteClubsError.UnknownError));
    }
}

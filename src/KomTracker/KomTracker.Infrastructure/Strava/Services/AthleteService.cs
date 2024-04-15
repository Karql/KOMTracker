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

        var mappedErrorMessage = getKomsRes.Errors.OfType<ApiModel.Segment.Error.GetKomsError>().FirstOrDefault()?.Message switch
        {
            ApiModel.Segment.Error.GetKomsError.Unauthorized => GetAthleteKomsError.Unauthorized,
            ApiModel.Segment.Error.GetKomsError.TooManyRequests => GetAthleteKomsError.TooManyRequests,
            _ => GetAthleteKomsError.UnknownError
        };

        return Result.Fail<IEnumerable<(SegmentEffortEntity, SegmentEntity)>>(new GetAthleteKomsError(mappedErrorMessage));
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

        var mappedErrorMessage = getKomsRes.Errors.OfType<ApiModel.Club.Error.GetClubsError>().FirstOrDefault()?.Message switch
        {
            ApiModel.Club.Error.GetClubsError.Unauthorized => GetAthleteClubsError.Unauthorized,
            ApiModel.Club.Error.GetClubsError.TooManyRequests => GetAthleteClubsError.TooManyRequests,
            _ => GetAthleteClubsError.UnknownError
        };

        return Result.Fail<IEnumerable<ClubEntity>>(new GetAthleteClubsError(mappedErrorMessage));
    }
}

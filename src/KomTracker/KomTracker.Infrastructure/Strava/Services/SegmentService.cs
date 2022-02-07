using AutoMapper;
using FluentResults;
using KomTracker.Application.Errors.Strava.Strava;
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

public class SegmentService : ISegmentService
{
    private readonly IMapper _mapper;
    private readonly ISegmentApi _segmentApi;

    private readonly IReadOnlyDictionary<string, string> _getSegmentErrorMap = new Dictionary<string, string>()
    {
        { ApiModel.Segment.Error.GetSegmentError.Unauthorized,  GetSegmentError.Unauthorized },
        { ApiModel.Segment.Error.GetSegmentError.NotFound,  GetSegmentError.NotFound },
    };

    public SegmentService(IMapper mapper, ISegmentApi segmentApi)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _segmentApi = segmentApi ?? throw new ArgumentNullException(nameof(segmentApi));
    }

    public async Task<Result<SegmentEntity>> GetSegmentAsync(long id, string token)
    {
        var getSegmentRes = await _segmentApi.GetSegmentAsync(id, token);

        if (getSegmentRes.IsSuccess == true)
        {
            return Result.Ok(_mapper.Map<SegmentEntity>(getSegmentRes.Value));
        }

        var error = getSegmentRes.Errors.OfType<ApiModel.Segment.Error.GetSegmentError>().FirstOrDefault();

        if (_getSegmentErrorMap.ContainsKey(error?.Message))
        {
            return Result.Fail<SegmentEntity>(new GetSegmentError(_getSegmentErrorMap[error.Message]));
        }

        return Result.Fail<SegmentEntity>(new GetSegmentError(GetSegmentError.UnknownError));
    }
}

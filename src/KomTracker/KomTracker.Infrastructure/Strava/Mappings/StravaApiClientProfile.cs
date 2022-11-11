using AutoMapper;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Club;
using KomTracker.Domain.Entities.Segment;
using KomTracker.Domain.Entities.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiModel = Strava.API.Client.Model;

namespace KomTracker.Infrastructure.Strava.Mappings;

public class StravaApiClientProfile : Profile
{
    public StravaApiClientProfile()
    {
        CreateMap<ApiModel.Athlete.AthleteSummaryModel, AthleteEntity>()
            .ForMember(dest => dest.AthleteId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username ?? src.Id.ToString())); // fix for null username

        CreateMap<ApiModel.Token.TokenModel, TokenEntity>();

        CreateMap<ApiModel.Token.TokenWithAthleteModel, TokenEntity>()
            .ForMember(dest => dest.AthleteId, opt => opt.MapFrom(src => src.Athlete.Id));

        CreateMap<ApiModel.Segment.SegmentEffortDetailedModel, SegmentEffortEntity>()
            .ForMember(dest => dest.AthleteId, opt => opt.MapFrom(src => src.Athlete.Id))
            .ForMember(dest => dest.SegmentId, opt => opt.MapFrom(src => src.Segment.Id))
            .ForMember(dest => dest.ActivityId, opt => opt.MapFrom(src => src.Activity.Id));

        CreateMap<ApiModel.Segment.SegmentSummaryModel, SegmentEntity>();
        CreateMap<ApiModel.Segment.SegmentDetailedModel, SegmentEntity>();

        CreateMap<ApiModel.Club.ClubSummaryModel, ClubEntity>();
    }
}

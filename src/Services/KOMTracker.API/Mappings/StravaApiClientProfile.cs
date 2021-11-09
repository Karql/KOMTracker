using AutoMapper;
using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Segment;
using KOMTracker.API.Models.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiModel = Strava.API.Client.Model;

namespace KOMTracker.API.Mappings
{
    public class StravaApiClientProfile : Profile
    {
        public StravaApiClientProfile()
        {
            CreateMap<ApiModel.Athlete.AthleteSummaryModel, AthleteModel>()
                .ForMember(dest => dest.AthleteId, opt => opt.MapFrom(src => src.Id));

            CreateMap<ApiModel.Token.TokenModel, TokenModel>();

            CreateMap<ApiModel.Token.TokenWithAthleteModel, TokenModel>()
                .ForMember(dest => dest.AthleteId, opt => opt.MapFrom(src => src.Athlete.Id));

            CreateMap<ApiModel.Segment.SegmentEffortDetailedModel, SegmentEffortModel>()
                .ForMember(dest => dest.AthleteId, opt => opt.MapFrom(src => src.Athlete.Id))
                .ForMember(dest => dest.SegmentId, opt => opt.MapFrom(src => src.Segment.Id))
                .ForMember(dest => dest.ActivityId, opt => opt.MapFrom(src => src.Activity.Id));

            CreateMap<ApiModel.Segment.SegmentSummaryModel, SegmentModel>();
        }
    }
}

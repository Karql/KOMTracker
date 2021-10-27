using AutoMapper;
using KOMTracker.API.Models.Strava;
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
            CreateMap<ApiModel.Athlete.AthleteModel, AthleteModel>()
                .ForMember(dest => dest.AthleteId, opt => opt.MapFrom(src => src.Id));

            CreateMap<ApiModel.Token.TokenWithAthleteModel, TokenModel>()
                .ForMember(dest => dest.AthleteId, opt => opt.MapFrom(src => src.Athlete.Id));
        }
    }
}

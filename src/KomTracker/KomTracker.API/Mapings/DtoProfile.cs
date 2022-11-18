using AutoMapper;
using KomTracker.API.Shared.ViewModels.Athlete;
using KomTracker.API.Shared.ViewModels.Club;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.API.Shared.ViewModels.Stats;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Models.Stats;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Club;
using KomTracker.Domain.Entities.Segment;

namespace KomTracker.API.Mapings;

public class DtoProfile : Profile
{
    public DtoProfile()
    {
        CreateMap<SegmentEffortEntity, SegmentEffortViewModel>();
        CreateMap<SegmentEntity, SegmentViewModel>();
        CreateMap<KomsSummarySegmentEffortEntity, KomsSummarySegmentEffortViewModel>()
            .ForMember(dest => dest.TrackDate, opt => opt.MapFrom(src => src.AuditCD)); 

        CreateMap<EffortModel, EffortViewModel>();
        CreateMap<KomsSummaryEntity, KomsSummaryViewModel>();
        CreateMap<AthleteEntity, AthleteViewModel>();
        CreateMap<EffortWithAthleteModel, EffortWithAthleteViewModel>();

        CreateMap<ClubEntity, ClubViewModel>();
    }
}

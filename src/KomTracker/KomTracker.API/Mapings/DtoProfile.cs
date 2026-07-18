using AutoMapper;
using KomTracker.API.Shared.ViewModels.Athlete;
using KomTracker.API.Shared.ViewModels.Club;
using KomTracker.API.Shared.ViewModels.KomTakeover;
using KomTracker.API.Shared.ViewModels.Ranking;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.API.Shared.ViewModels.Stats;
using KomTracker.Application.Models.Ranking;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Models.Stats;
using KomTracker.Application.Shared.Helpers;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Club;
using KomTracker.Domain.Entities.Segment;

namespace KomTracker.API.Mapings;

public class DtoProfile : Profile
{
    public DtoProfile()
    {
        CreateMap<SegmentEffortEntity, SegmentEffortViewModel>();
        CreateMap<SegmentEntity, SegmentViewModel>()
            .ForMember(dest => dest.Bearing, opt => opt.MapFrom(src =>
                GeoHelper.GetBearing(src.StartLatitude, src.StartLongitude, src.EndLatitude, src.EndLongitude)));
        CreateMap<KomsSummarySegmentEffortEntity, KomsSummarySegmentEffortViewModel>()
            .ForMember(dest => dest.TrackDate, opt => opt.MapFrom(src => src.AuditCD)); 

        CreateMap<EffortModel, EffortViewModel>();
        CreateMap<KomsSummaryEntity, KomsSummaryViewModel>();
        CreateMap<AthleteEntity, AthleteViewModel>();
        CreateMap<EffortWithAthleteModel, EffortWithAthleteViewModel>();

        CreateMap<ClubEntity, ClubViewModel>();

        CreateMap<AthleteRankingModel, AthleteRankingViewModel>();
        CreateMap<AthleteRankingTotalModel, AthleteRankingTotalViewModel>();
        CreateMap<AthleteRankingKomsChangesModel, AthleteRankingKomsChangesViewModel>();

        CreateMap<KomTakeoverPairModel, KomTakeoverPairViewModel>();
    }
}

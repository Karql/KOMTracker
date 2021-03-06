using AutoMapper;
using KomTracker.API.Shared.ViewModels.Segment;
using KomTracker.Application.Models.Segment;
using KomTracker.Domain.Entities.Segment;

namespace KomTracker.API.Mapings;

public class DtoProfile : Profile
{
    public DtoProfile()
    {
        CreateMap<SegmentEffortEntity, SegmentEffortViewModel>();
        CreateMap<SegmentEntity, SegmentViewModel>();
        CreateMap<KomsSummarySegmentEffortEntity, KomsSummarySegmentEffortViewModel>();
        CreateMap<EffortModel, EffortViewModel>();
        CreateMap<KomsSummaryEntity, KomsSummaryViewModel>();
    }
}

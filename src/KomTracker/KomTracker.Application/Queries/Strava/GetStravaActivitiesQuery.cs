using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models;
using KomTracker.Application.Models.Strava;
using KomTracker.Domain.Entities.Bike;
using MediatR;

namespace KomTracker.Application.Queries.Strava;

/// <summary>One page of the athlete's synced activities, newest first, with each row's linked bike resolved.</summary>
public class GetStravaActivitiesQuery : IRequest<PagedResultModel<ActivityListItemModel>>
{
    public int AthleteId { get; set; }
    public string UserId { get; set; } = default!;
    public int Page { get; set; }        // 0-based
    public int PageSize { get; set; } = 20;
}

public class GetStravaActivitiesQueryHandler : IRequestHandler<GetStravaActivitiesQuery, PagedResultModel<ActivityListItemModel>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetStravaActivitiesQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<PagedResultModel<ActivityListItemModel>> Handle(GetStravaActivitiesQuery request, CancellationToken cancellationToken)
    {
        var activityRepo = _komUoW.GetRepository<IActivityRepository>();
        var bikeLinkRepo = _komUoW.GetRepository<IBikeLinkRepository>();
        var bikeRepo = _komUoW.GetRepository<IBikeRepository>();
        var stravaBikeRepo = _komUoW.GetRepository<IStravaBikeRepository>();

        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;
        var page = request.Page < 0 ? 0 : request.Page;

        var total = await activityRepo.CountAthleteActivitiesAsync(request.AthleteId);
        var activities = (await activityRepo.GetActivitiesPageAsync(request.AthleteId, page * pageSize, pageSize)).ToList();

        // Resolve gear_id -> the caller's bt.bike (name), for the "Rower" column.
        var gearIds = activities
            .Where(a => a.GearId is not null)
            .Select(a => a.GearId!)
            .Distinct()
            .ToList();

        var bikeIdByGearId = (await bikeLinkRepo.GetByExternalIdsAsync(ExternalService.Strava, gearIds))
            .ToDictionary(l => l.ExternalId, l => l.BikeId);

        var bikeNameById = (await bikeRepo.GetBikesAsync(request.UserId, includeInactive: true))
            .ToDictionary(b => b.Id, b => b.Name);

        // The gear's Strava name (shown even when not linked to a bt.bike).
        var stravaBikeNameByGearId = (await stravaBikeRepo.GetByAthleteAsync(request.AthleteId))
            .Where(b => b.Name is not null)
            .ToDictionary(b => b.Id, b => b.Name!);

        var items = activities.Select(a =>
        {
            int? linkedBikeId = a.GearId is not null
                && bikeIdByGearId.TryGetValue(a.GearId, out var bikeId)
                && bikeNameById.ContainsKey(bikeId)
                    ? bikeId
                    : null;

            return new ActivityListItemModel
            {
                Id = a.Id,
                Name = a.Name,
                SportType = a.SportType,
                DistanceMeters = a.Distance,
                MovingTimeSeconds = a.MovingTime,
                AverageSpeedMps = a.AverageSpeed,
                ElevationMeters = a.TotalElevationGain,
                StartDateUtc = a.StartDate,
                UtcOffset = a.UtcOffset,
                GearId = a.GearId,
                LinkedBikeId = linkedBikeId,
                LinkedBikeName = linkedBikeId is int id ? bikeNameById[id] : null,
                StravaBikeName = a.GearId is not null && stravaBikeNameByGearId.TryGetValue(a.GearId, out var sbName) ? sbName : null
            };
        }).ToList();

        return new PagedResultModel<ActivityListItemModel>
        {
            Items = items,
            TotalCount = total
        };
    }
}

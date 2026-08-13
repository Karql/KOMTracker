using FluentResults;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Bike;
using MediatR;

namespace KomTracker.Application.Commands.Strava;

/// <summary>
/// Link an existing bt.bike (owned by the caller) to one of the caller's Strava bikes (strava.bike),
/// creating a bt.bike_link. Validates ownership on both sides and rejects a gear already linked.
/// </summary>
public class LinkStravaBikeCommand : IRequest<Result>
{
    public int BikeId { get; set; }
    public string StravaGearId { get; set; } = default!;

    // Server-owned (from claims).
    public string UserId { get; set; } = default!;
    public int AthleteId { get; set; }
}

public class LinkStravaBikeCommandHandler : IRequestHandler<LinkStravaBikeCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public LinkStravaBikeCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(LinkStravaBikeCommand request, CancellationToken cancellationToken)
    {
        var bikeRepo = _komUoW.GetRepository<IBikeRepository>();
        var stravaBikeRepo = _komUoW.GetRepository<IStravaBikeRepository>();
        var bikeLinkRepo = _komUoW.GetRepository<IBikeLinkRepository>();

        var bike = await bikeRepo.GetBikeAsync(request.BikeId);
        if (bike is null)
        {
            return Result.Fail(new NotFoundError($"Bike {request.BikeId} not found."));
        }

        if (bike.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Bike does not belong to the current user."));
        }

        var stravaBike = await stravaBikeRepo.GetAsync(request.AthleteId, request.StravaGearId);
        if (stravaBike is null)
        {
            return Result.Fail(new NotFoundError($"Strava bike {request.StravaGearId} not found for the current athlete."));
        }

        if (await bikeLinkRepo.ExistsAsync(ExternalService.Strava, request.StravaGearId))
        {
            return Result.Fail(new ConflictError($"Strava bike {request.StravaGearId} is already linked."));
        }

        bikeLinkRepo.Add(new BikeLinkEntity
        {
            BikeId = request.BikeId,
            ExternalService = ExternalService.Strava,
            ExternalId = request.StravaGearId
        });

        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}

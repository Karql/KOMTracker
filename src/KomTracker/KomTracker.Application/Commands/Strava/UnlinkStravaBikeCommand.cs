using FluentResults;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Bike;
using KomTracker.Application.Notifications.Bike;
using MediatR;

namespace KomTracker.Application.Commands.Strava;

/// <summary>Remove the bt.bike_link between a bt.bike and a Strava gear (unlink, from either side).</summary>
public class UnlinkStravaBikeCommand : IRequest<Result>
{
    public string StravaGearId { get; set; } = default!;

    // Server-owned (from claims).
    public string UserId { get; set; } = default!;
}

public class UnlinkStravaBikeCommandHandler : IRequestHandler<UnlinkStravaBikeCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IMediator _mediator;

    public UnlinkStravaBikeCommandHandler(IKOMUnitOfWork komUoW, IMediator mediator)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<Result> Handle(UnlinkStravaBikeCommand request, CancellationToken cancellationToken)
    {
        var bikeLinkRepo = _komUoW.GetRepository<IBikeLinkRepository>();
        var bikeRepo = _komUoW.GetRepository<IBikeRepository>();

        var link = await bikeLinkRepo.GetByExternalIdAsync(ExternalService.Strava, request.StravaGearId);
        if (link is null)
        {
            return Result.Fail(new NotFoundError($"No link for Strava bike {request.StravaGearId}."));
        }

        // The link is owned through its bike — only the bike's owner may unlink.
        var bike = await bikeRepo.GetBikeAsync(link.BikeId);
        if (bike is null || bike.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Link does not belong to the current user."));
        }

        bikeLinkRepo.Remove(link);
        await _komUoW.SaveChangesAsync();

        // That gear's rides no longer count toward the bike → refresh its components' and services' mileage.
        await _mediator.Publish(new BikeChangedNotification { BikeId = bike.Id }, cancellationToken);

        return Result.Ok();
    }
}

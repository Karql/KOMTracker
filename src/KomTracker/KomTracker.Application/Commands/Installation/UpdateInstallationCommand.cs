using FluentResults;
using FluentValidation;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Notifications.Component;
using KomTracker.Domain.Entities.Component;
using MediatR;

namespace KomTracker.Application.Commands.Installation;

/// <summary>
/// Edits an existing installation record (corrections). Every field except <see cref="ComponentInstallationType"/>
/// is editable; the parent may be re-pointed to a bike (<see cref="BikeId"/>) XOR a parent component
/// (<see cref="ParentComponentId"/>). Tracked rows edit the date window, Manual rows the static totals.
/// </summary>
public class UpdateInstallationCommand : IRequest<Result>
{
    public string UserId { get; set; } = default!;
    public int InstallationId { get; set; }

    public int? BikeId { get; set; }
    public int? ParentComponentId { get; set; }
    public InstallationPosition? Position { get; set; }

    // Tracked only
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    // Manual only
    public decimal? ManualDistanceKm { get; set; }
    public decimal? ManualMovingHours { get; set; }
    public decimal? ManualElevationM { get; set; }
}

public class UpdateInstallationCommandValidator : AbstractValidator<UpdateInstallationCommand>
{
    public UpdateInstallationCommandValidator()
    {
        RuleFor(x => x.InstallationId).GreaterThan(0);
        RuleFor(x => x).Must(x => x.BikeId.HasValue ^ x.ParentComponentId.HasValue)
            .WithMessage("Exactly one of BikeId or ParentComponentId must be set.");
        RuleFor(x => x.BikeId).GreaterThan(0).When(x => x.BikeId.HasValue);
        RuleFor(x => x.ParentComponentId).GreaterThan(0).When(x => x.ParentComponentId.HasValue);
        RuleFor(x => x.Position).IsInEnum().When(x => x.Position.HasValue);
        RuleFor(x => x.ManualDistanceKm).GreaterThanOrEqualTo(0).When(x => x.ManualDistanceKm.HasValue);
        RuleFor(x => x.ManualMovingHours).GreaterThanOrEqualTo(0).When(x => x.ManualMovingHours.HasValue);
        RuleFor(x => x.ManualElevationM).GreaterThanOrEqualTo(0).When(x => x.ManualElevationM.HasValue);
    }
}

public class UpdateInstallationCommandHandler : IRequestHandler<UpdateInstallationCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IMediator _mediator;

    public UpdateInstallationCommandHandler(IKOMUnitOfWork komUoW, IMediator mediator)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<Result> Handle(UpdateInstallationCommand request, CancellationToken cancellationToken)
    {
        var installationRepo = _komUoW.GetRepository<IInstallationRepository>();
        var bikeRepo = _komUoW.GetRepository<IBikeRepository>();
        var componentRepo = _komUoW.GetRepository<IComponentRepository>();

        var installation = await installationRepo.GetAsync(request.InstallationId);
        if (installation is null)
        {
            return Result.Fail(new NotFoundError($"Installation {request.InstallationId} not found."));
        }

        if (installation.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Installation does not belong to the current user."));
        }

        // Resolve + own the target (bike XOR parent component).
        if (request.BikeId is int bikeId)
        {
            var bike = await bikeRepo.GetBikeAsync(bikeId);
            if (bike is null)
            {
                return Result.Fail(new NotFoundError($"Bike {bikeId} not found."));
            }

            if (bike.UserId != request.UserId)
            {
                return Result.Fail(new ForbiddenError("Bike does not belong to the current user."));
            }
        }
        else
        {
            var parent = await componentRepo.GetComponentAsync(request.ParentComponentId!.Value);
            if (parent is null)
            {
                return Result.Fail(new NotFoundError($"Component {request.ParentComponentId} not found."));
            }

            if (parent.UserId != request.UserId)
            {
                return Result.Fail(new ForbiddenError("Parent component does not belong to the current user."));
            }

            if (!parent.IsMetaComponent)
            {
                return Result.Fail(new ConflictError(
                    "Target is not a meta component — mark it as one to install components inside it."));
            }
        }

        var tracked = installation.Type == ComponentInstallationType.Tracked;

        if (tracked)
        {
            var newDateTo = InstallationDateHelper.EnsureUtc(request.DateTo);

            // A row that stays/becomes active (DateTo == null) must satisfy the full D-7 invariant (ignoring itself).
            if (newDateTo is null)
            {
                var invariant = await InstallationInvariant.CheckAsync(
                    installationRepo, installation.ComponentId, request.BikeId, request.ParentComponentId, installation.Id);
                if (invariant.IsFailed)
                {
                    return invariant;
                }
            }

            installation.DateFrom = InstallationDateHelper.EnsureUtc(request.DateFrom);
            installation.DateTo = newDateTo;
        }
        else
        {
            installation.ManualDistanceKm = request.ManualDistanceKm;
            installation.ManualMovingHours = request.ManualMovingHours;
            installation.ManualElevationM = request.ManualElevationM;
        }

        installation.BikeId = request.BikeId;
        installation.ParentComponentId = request.ParentComponentId;
        installation.Position = request.Position;
        installationRepo.Update(installation);

        await _komUoW.SaveChangesAsync();

        await _mediator.Publish(new InstallationChangedNotification { ComponentId = installation.ComponentId }, cancellationToken);

        return Result.Ok();
    }
}

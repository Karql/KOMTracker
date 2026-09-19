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
/// Moves an active Tracked installation to a new parent — a bike (<see cref="NewBikeId"/>) XOR a parent component
/// (<see cref="NewParentComponentId"/>): closes the current window and opens a new one (atomic).
/// </summary>
public class MoveInstallationCommand : IRequest<Result>
{
    public string UserId { get; set; } = default!;
    public int InstallationId { get; set; }
    public int? NewBikeId { get; set; }
    public int? NewParentComponentId { get; set; }
    public InstallationPosition? NewPosition { get; set; }
    public DateTime MoveDate { get; set; }
}

public class MoveInstallationCommandValidator : AbstractValidator<MoveInstallationCommand>
{
    public MoveInstallationCommandValidator()
    {
        RuleFor(x => x).Must(x => x.NewBikeId.HasValue ^ x.NewParentComponentId.HasValue)
            .WithMessage("Exactly one of NewBikeId or NewParentComponentId must be set.");
        RuleFor(x => x.NewBikeId).GreaterThan(0).When(x => x.NewBikeId.HasValue);
        RuleFor(x => x.NewParentComponentId).GreaterThan(0).When(x => x.NewParentComponentId.HasValue);
        RuleFor(x => x.NewPosition).IsInEnum().When(x => x.NewPosition.HasValue);
    }
}

public class MoveInstallationCommandHandler : IRequestHandler<MoveInstallationCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IMediator _mediator;

    public MoveInstallationCommandHandler(IKOMUnitOfWork komUoW, IMediator mediator)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<Result> Handle(MoveInstallationCommand request, CancellationToken cancellationToken)
    {
        var installationRepo = _komUoW.GetRepository<IInstallationRepository>();
        var bikeRepo = _komUoW.GetRepository<IBikeRepository>();
        var componentRepo = _komUoW.GetRepository<IComponentRepository>();

        var current = await installationRepo.GetAsync(request.InstallationId);
        if (current is null)
        {
            return Result.Fail(new NotFoundError($"Installation {request.InstallationId} not found."));
        }

        if (current.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Installation does not belong to the current user."));
        }

        if (!current.IsCurrent)
        {
            return Result.Fail(new ConflictError("Only a currently-installed component can be moved."));
        }

        // Resolve + own the new target (bike XOR parent component).
        if (request.NewBikeId is int newBikeId)
        {
            var newBike = await bikeRepo.GetBikeAsync(newBikeId);
            if (newBike is null)
            {
                return Result.Fail(new NotFoundError($"Bike {newBikeId} not found."));
            }

            if (newBike.UserId != request.UserId)
            {
                return Result.Fail(new ForbiddenError("Bike does not belong to the current user."));
            }
        }
        else
        {
            var parent = await componentRepo.GetComponentAsync(request.NewParentComponentId!.Value);
            if (parent is null)
            {
                return Result.Fail(new NotFoundError($"Component {request.NewParentComponentId} not found."));
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

        // D-7 — the new placement must be valid, ignoring the row we're about to close.
        var invariant = await InstallationInvariant.CheckAsync(
            installationRepo, current.ComponentId, request.NewBikeId, request.NewParentComponentId, current.Id);
        if (invariant.IsFailed)
        {
            return invariant;
        }

        var moveDate = InstallationDateHelper.EnsureUtc(request.MoveDate);

        // Close the current window.
        current.DateTo = moveDate;
        installationRepo.Update(current);

        // Open a new Tracked window on the new parent/position.
        installationRepo.Add(new InstallationEntity
        {
            UserId = request.UserId,
            ComponentId = current.ComponentId,
            BikeId = request.NewBikeId,
            ParentComponentId = request.NewParentComponentId,
            Type = ComponentInstallationType.Tracked,
            Position = request.NewPosition,
            DateFrom = moveDate,
            DateTo = null
        });

        await _komUoW.SaveChangesAsync();

        await _mediator.Publish(new InstallationChangedNotification { ComponentId = current.ComponentId }, cancellationToken);

        return Result.Ok();
    }
}

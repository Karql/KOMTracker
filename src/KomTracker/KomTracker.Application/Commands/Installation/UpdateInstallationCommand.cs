using FluentResults;
using FluentValidation;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Component;
using MediatR;

namespace KomTracker.Application.Commands.Installation;

/// <summary>
/// Edits an existing installation record (corrections). Every field except <see cref="ComponentInstallationType"/>
/// is editable; Tracked rows edit the bike/position/date window, Manual rows edit the bike/position/static totals.
/// </summary>
public class UpdateInstallationCommand : IRequest<Result>
{
    public string UserId { get; set; } = default!;
    public int InstallationId { get; set; }

    public int BikeId { get; set; }
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
        RuleFor(x => x.BikeId).GreaterThan(0);
        RuleFor(x => x.Position).IsInEnum().When(x => x.Position.HasValue);
        RuleFor(x => x.ManualDistanceKm).GreaterThanOrEqualTo(0).When(x => x.ManualDistanceKm.HasValue);
        RuleFor(x => x.ManualMovingHours).GreaterThanOrEqualTo(0).When(x => x.ManualMovingHours.HasValue);
        RuleFor(x => x.ManualElevationM).GreaterThanOrEqualTo(0).When(x => x.ManualElevationM.HasValue);
    }
}

public class UpdateInstallationCommandHandler : IRequestHandler<UpdateInstallationCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public UpdateInstallationCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(UpdateInstallationCommand request, CancellationToken cancellationToken)
    {
        var installationRepo = _komUoW.GetRepository<IInstallationRepository>();
        var bikeRepo = _komUoW.GetRepository<IBikeRepository>();

        var installation = await installationRepo.GetAsync(request.InstallationId);
        if (installation is null)
        {
            return Result.Fail(new NotFoundError($"Installation {request.InstallationId} not found."));
        }

        if (installation.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Installation does not belong to the current user."));
        }

        var bike = await bikeRepo.GetBikeAsync(request.BikeId);
        if (bike is null)
        {
            return Result.Fail(new NotFoundError($"Bike {request.BikeId} not found."));
        }

        if (bike.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Bike does not belong to the current user."));
        }

        var tracked = installation.Type == ComponentInstallationType.Tracked;

        if (tracked)
        {
            var newDateTo = InstallationDateHelper.EnsureUtc(request.DateTo);

            // Invariant (D-2b1-3): editing must not create a second active Tracked installation for the component.
            if (newDateTo is null)
            {
                var active = await installationRepo.GetActiveTrackedByComponentAsync(installation.ComponentId);
                if (active is not null && active.Id != installation.Id)
                {
                    return Result.Fail(new ConflictError(
                        "Component already has an active installation — close it first."));
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
        installation.Position = request.Position;
        installationRepo.Update(installation);

        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}

using FluentResults;
using FluentValidation;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Component;
using MediatR;

namespace KomTracker.Application.Commands.Installation;

/// <summary>Installs a component on a bike — Tracked (dated) or Manual (dateless historical, static totals).</summary>
public class InstallComponentCommand : IRequest<Result<InstallationEntity>>
{
    public string UserId { get; set; } = default!;

    public int ComponentId { get; set; }
    public int BikeId { get; set; }
    public ComponentInstallationType Type { get; set; }

    public DateTime? DateFrom { get; set; }
    public InstallationPosition? Position { get; set; }

    // Manual only
    public decimal? ManualDistanceKm { get; set; }
    public decimal? ManualMovingHours { get; set; }
    public decimal? ManualElevationM { get; set; }
}

public class InstallComponentCommandValidator : AbstractValidator<InstallComponentCommand>
{
    public InstallComponentCommandValidator()
    {
        RuleFor(x => x.ComponentId).GreaterThan(0);
        RuleFor(x => x.BikeId).GreaterThan(0);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Position).IsInEnum().When(x => x.Position.HasValue);

        When(x => x.Type == ComponentInstallationType.Tracked, () =>
        {
            RuleFor(x => x.DateFrom).NotNull();
        });

        RuleFor(x => x.ManualDistanceKm).GreaterThanOrEqualTo(0).When(x => x.ManualDistanceKm.HasValue);
        RuleFor(x => x.ManualMovingHours).GreaterThanOrEqualTo(0).When(x => x.ManualMovingHours.HasValue);
        RuleFor(x => x.ManualElevationM).GreaterThanOrEqualTo(0).When(x => x.ManualElevationM.HasValue);
    }
}

public class InstallComponentCommandHandler : IRequestHandler<InstallComponentCommand, Result<InstallationEntity>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public InstallComponentCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result<InstallationEntity>> Handle(InstallComponentCommand request, CancellationToken cancellationToken)
    {
        var componentRepo = _komUoW.GetRepository<IComponentRepository>();
        var bikeRepo = _komUoW.GetRepository<IBikeRepository>();
        var installationRepo = _komUoW.GetRepository<IInstallationRepository>();

        var component = await componentRepo.GetComponentAsync(request.ComponentId);
        if (component is null)
        {
            return Result.Fail(new NotFoundError($"Component {request.ComponentId} not found."));
        }

        if (component.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Component does not belong to the current user."));
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

        var tracked = request.Type == ComponentInstallationType.Tracked;

        if (tracked)
        {
            // Invariant (D-7 subset): at most one active Tracked installation per component.
            var active = await installationRepo.GetActiveTrackedByComponentAsync(request.ComponentId);
            if (active is not null)
            {
                return Result.Fail(new ConflictError("Component is already installed — remove or move it first."));
            }
        }

        var installation = new InstallationEntity
        {
            UserId = request.UserId,
            ComponentId = request.ComponentId,
            BikeId = request.BikeId,
            Type = request.Type,
            Position = request.Position,
            DateFrom = tracked ? InstallationDateHelper.EnsureUtc(request.DateFrom) : null,
            DateTo = null,
            ManualDistanceKm = tracked ? null : request.ManualDistanceKm,
            ManualMovingHours = tracked ? null : request.ManualMovingHours,
            ManualElevationM = tracked ? null : request.ManualElevationM
        };

        installationRepo.Add(installation);

        // Installing (Tracked) moves the component onto the bike — it's no longer sitting in a warehouse (D-2b1-5).
        if (tracked && component.WarehouseId is not null)
        {
            component.WarehouseId = null;
            componentRepo.UpdateComponent(component);
        }

        await _komUoW.SaveChangesAsync();

        // Read-model fields for the response VM.
        installation.ComponentName = component.Name;
        installation.ComponentCategory = component.Category;
        installation.BikeName = bike.Name;

        return Result.Ok(installation);
    }
}

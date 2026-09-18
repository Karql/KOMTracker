using FluentResults;
using FluentValidation;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Component;
using MediatR;

namespace KomTracker.Application.Commands.Installation;

/// <summary>
/// Installs a component onto a parent — a bike (<see cref="BikeId"/>) XOR a parent component
/// (<see cref="ParentComponentId"/>, 2b-ii). Tracked (dated) or Manual (dateless historical, static totals).
/// </summary>
public class InstallComponentCommand : IRequest<Result<InstallationEntity>>
{
    public string UserId { get; set; } = default!;

    public int ComponentId { get; set; }

    /// <summary>Target bike. XOR with <see cref="ParentComponentId"/>.</summary>
    public int? BikeId { get; set; }

    /// <summary>Target parent component (component-in-component). XOR with <see cref="BikeId"/>.</summary>
    public int? ParentComponentId { get; set; }

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
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Position).IsInEnum().When(x => x.Position.HasValue);

        // Exactly one target — a bike or a parent component.
        RuleFor(x => x).Must(x => x.BikeId.HasValue ^ x.ParentComponentId.HasValue)
            .WithMessage("Exactly one of BikeId or ParentComponentId must be set.");
        RuleFor(x => x.BikeId).GreaterThan(0).When(x => x.BikeId.HasValue);
        RuleFor(x => x.ParentComponentId).GreaterThan(0).When(x => x.ParentComponentId.HasValue);

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

        // Resolve + own the target (bike XOR parent component) and capture its display name.
        string? bikeName = null;
        string? parentComponentName = null;

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

            bikeName = bike.Name;
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

            parentComponentName = parent.Name;
        }

        var tracked = request.Type == ComponentInstallationType.Tracked;

        if (tracked)
        {
            // D-7 linkage invariant (homogeneity, distinct bikes, one-level comp-in-comp, no cycle).
            var invariant = await InstallationInvariant.CheckAsync(
                installationRepo, request.ComponentId, request.BikeId, request.ParentComponentId);
            if (invariant.IsFailed)
            {
                return invariant.ToResult<InstallationEntity>();
            }
        }

        var installation = new InstallationEntity
        {
            UserId = request.UserId,
            ComponentId = request.ComponentId,
            BikeId = request.BikeId,
            ParentComponentId = request.ParentComponentId,
            Type = request.Type,
            Position = request.Position,
            DateFrom = tracked ? InstallationDateHelper.EnsureUtc(request.DateFrom) : null,
            DateTo = null,
            ManualDistanceKm = tracked ? null : request.ManualDistanceKm,
            ManualMovingHours = tracked ? null : request.ManualMovingHours,
            ManualElevationM = tracked ? null : request.ManualElevationM
        };

        installationRepo.Add(installation);

        // Installing (Tracked) places the component — it's no longer sitting in a warehouse (D-2b1-5).
        if (tracked && component.WarehouseId is not null)
        {
            component.WarehouseId = null;
            componentRepo.UpdateComponent(component);
        }

        await _komUoW.SaveChangesAsync();

        // Read-model fields for the response VM.
        installation.ComponentName = component.Name;
        installation.ComponentCategory = component.Category;
        installation.BikeName = bikeName;
        installation.ParentComponentName = parentComponentName;

        return Result.Ok(installation);
    }
}

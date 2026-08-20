using FluentResults;
using FluentValidation;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Component;
using MediatR;

namespace KomTracker.Application.Commands.Component;

/// <summary>Creates a component when <see cref="Id"/> is null, otherwise updates the existing one.</summary>
public class SaveComponentCommand : IRequest<Result<ComponentEntity>>
{
    // Server-owned (set by the controller from route/claims), not part of the request body.
    public int? Id { get; set; }
    public string UserId { get; set; } = default!;

    // Editable fields — mirror SaveComponentViewModel (parity test guards drift).
    public string Name { get; set; } = default!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public ComponentCategory Category { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Notes { get; set; }
    public decimal? Price { get; set; }
    public string? PurchasePlace { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal InitialDistanceKm { get; set; }
    public decimal? InitialMovingHours { get; set; }
    public decimal? InitialElevationM { get; set; }
    public int? WarehouseId { get; set; }
}

public class SaveComponentCommandValidator : AbstractValidator<SaveComponentCommand>
{
    public SaveComponentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Brand).MaximumLength(200);
        RuleFor(x => x.Model).MaximumLength(200);
        RuleFor(x => x.PurchasePlace).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.WeightKg).GreaterThan(0).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).When(x => x.Price.HasValue);
        RuleFor(x => x.InitialDistanceKm).GreaterThanOrEqualTo(0);
        RuleFor(x => x.InitialMovingHours).GreaterThanOrEqualTo(0).When(x => x.InitialMovingHours.HasValue);
        RuleFor(x => x.InitialElevationM).GreaterThanOrEqualTo(0).When(x => x.InitialElevationM.HasValue);
    }
}

public class SaveComponentCommandHandler : IRequestHandler<SaveComponentCommand, Result<ComponentEntity>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public SaveComponentCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result<ComponentEntity>> Handle(SaveComponentCommand request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IComponentRepository>();

        // Guard the (optional) warehouse belongs to the caller — don't let a component point at someone else's.
        if (request.WarehouseId is int warehouseId)
        {
            var warehouse = await _komUoW.GetRepository<IWarehouseRepository>().GetWarehouseAsync(warehouseId);
            if (warehouse is null || warehouse.UserId != request.UserId)
            {
                return Result.Fail(new ValidationError(new Dictionary<string, string[]>
                {
                    ["warehouseId"] = new[] { $"Warehouse {warehouseId} not found." }
                }));
            }
        }

        ComponentEntity component;

        if (request.Id is null)
        {
            component = new ComponentEntity { UserId = request.UserId, Lifecycle = ComponentLifecycle.Active };
            Apply(request, component);
            repo.AddComponent(component);
        }
        else
        {
            var existing = await repo.GetComponentAsync(request.Id.Value);

            if (existing is null)
            {
                return Result.Fail(new NotFoundError($"Component {request.Id} not found."));
            }

            if (existing.UserId != request.UserId)
            {
                return Result.Fail(new ForbiddenError("Component does not belong to the current user."));
            }

            Apply(request, existing);
            repo.UpdateComponent(existing);
            component = existing;
        }

        await _komUoW.SaveChangesAsync();

        return Result.Ok(component);
    }

    private static void Apply(SaveComponentCommand request, ComponentEntity component)
    {
        component.Name = request.Name;
        component.Brand = request.Brand;
        component.Model = request.Model;
        component.Category = request.Category;
        component.WeightKg = request.WeightKg;
        component.Notes = request.Notes;
        component.Price = request.Price;
        component.PurchasePlace = request.PurchasePlace;
        component.PurchaseDate = ComponentDateHelper.EnsureUtc(request.PurchaseDate);
        component.InitialDistanceKm = request.InitialDistanceKm;
        component.InitialMovingHours = request.InitialMovingHours;
        component.InitialElevationM = request.InitialElevationM;
        component.WarehouseId = request.WarehouseId;
    }
}

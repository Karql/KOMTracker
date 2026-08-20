using FluentResults;
using FluentValidation;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Warehouse;
using MediatR;

namespace KomTracker.Application.Commands.Warehouse;

/// <summary>Creates a warehouse when <see cref="Id"/> is null, otherwise updates the existing one.</summary>
public class SaveWarehouseCommand : IRequest<Result<WarehouseEntity>>
{
    public int? Id { get; set; }
    public string UserId { get; set; } = default!;

    public string Name { get; set; } = default!;
}

public class SaveWarehouseCommandValidator : AbstractValidator<SaveWarehouseCommand>
{
    public SaveWarehouseCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class SaveWarehouseCommandHandler : IRequestHandler<SaveWarehouseCommand, Result<WarehouseEntity>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public SaveWarehouseCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result<WarehouseEntity>> Handle(SaveWarehouseCommand request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IWarehouseRepository>();

        WarehouseEntity warehouse;

        if (request.Id is null)
        {
            warehouse = new WarehouseEntity { UserId = request.UserId, Name = request.Name };
            repo.AddWarehouse(warehouse);
        }
        else
        {
            var existing = await repo.GetWarehouseAsync(request.Id.Value);

            if (existing is null)
            {
                return Result.Fail(new NotFoundError($"Warehouse {request.Id} not found."));
            }

            if (existing.UserId != request.UserId)
            {
                return Result.Fail(new ForbiddenError("Warehouse does not belong to the current user."));
            }

            existing.Name = request.Name;
            repo.UpdateWarehouse(existing);
            warehouse = existing;
        }

        await _komUoW.SaveChangesAsync();

        return Result.Ok(warehouse);
    }
}

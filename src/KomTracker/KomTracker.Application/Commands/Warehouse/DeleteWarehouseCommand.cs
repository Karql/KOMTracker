using FluentResults;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using MediatR;

namespace KomTracker.Application.Commands.Warehouse;

public class DeleteWarehouseCommand : IRequest<Result>
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
}

public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public DeleteWarehouseCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IWarehouseRepository>();
        var warehouse = await repo.GetWarehouseAsync(request.Id);

        if (warehouse is null)
        {
            return Result.Fail(new NotFoundError($"Warehouse {request.Id} not found."));
        }

        if (warehouse.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Warehouse does not belong to the current user."));
        }

        // Components in this warehouse are not deleted — clear their location first (explicit, so no dangling
        // warehouse_id remains regardless of DB cascade), then remove the warehouse.
        await _komUoW.GetRepository<IComponentRepository>().ClearWarehouseAsync(request.Id);

        repo.DeleteWarehouse(warehouse);
        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}

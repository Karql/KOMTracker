using FluentResults;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using MediatR;

namespace KomTracker.Application.Commands.Bike;

public class DeleteBikeCommand : IRequest<Result>
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
}

public class DeleteBikeCommandHandler : IRequestHandler<DeleteBikeCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public DeleteBikeCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(DeleteBikeCommand request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IBikeRepository>();
        var bike = await repo.GetBikeAsync(request.Id);

        if (bike is null)
        {
            return Result.Fail(new NotFoundError($"Bike {request.Id} not found."));
        }

        if (bike.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Bike does not belong to the current user."));
        }

        // Remove the bike's installations first (frees the components) — mirrors ClearWarehouseAsync (D-2b1-6).
        await _komUoW.GetRepository<IInstallationRepository>().DeleteByBikeAsync(bike.Id);

        repo.DeleteBike(bike);
        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}

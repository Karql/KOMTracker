using FluentResults;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using MediatR;

namespace KomTracker.Application.Commands.Component;

public class DeleteComponentCommand : IRequest<Result>
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
}

public class DeleteComponentCommandHandler : IRequestHandler<DeleteComponentCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public DeleteComponentCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(DeleteComponentCommand request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IComponentRepository>();
        var component = await repo.GetComponentAsync(request.Id);

        if (component is null)
        {
            return Result.Fail(new NotFoundError($"Component {request.Id} not found."));
        }

        if (component.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Component does not belong to the current user."));
        }

        // Phase 2a: no installations exist yet, so a hard delete is safe.
        // Phase 2b adds the install-history guard (D-18 — block/warn when the component has installation history).
        repo.DeleteComponent(component);
        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}

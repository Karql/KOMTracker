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

        // D-18: never hard-delete a component that has installation history — prefer archiving it.
        if (await _komUoW.GetRepository<IInstallationRepository>().AnyByComponentAsync(component.Id))
        {
            return Result.Fail(new ConflictError(
                "Component has installation history — archive it instead of deleting."));
        }

        repo.DeleteComponent(component);
        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}

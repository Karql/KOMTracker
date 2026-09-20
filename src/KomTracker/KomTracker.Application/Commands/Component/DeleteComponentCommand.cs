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

    /// <summary>
    /// Explicit user override (D-18): when true, also hard-delete the component's OWN installation records instead of
    /// failing with a Conflict. It never touches rows where this component is the parent — those belong to the
    /// children installed inside it (a row "tyre in wheel" is the tyre's own history), so a meta component that
    /// holds/held parts can't be forced away; delete those parts first, or archive it. Default false keeps deletion
    /// safe (archive is the normal path for a used component).
    /// </summary>
    public bool Force { get; set; }
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

        var installationRepo = _komUoW.GetRepository<IInstallationRepository>();

        // D-18: as-parent rows are the CHILDREN's history (deleting them would silently wipe a part's in-this-component
        // mileage). Never force through that — the parts must be removed/deleted first (each part's own force-delete
        // drops the "in this component" row), or the component archived. Applies even with Force.
        if (await installationRepo.AnyByParentComponentAsync(component.Id))
        {
            return Result.Fail(new ConflictError(
                "This component has other components installed inside it (now or previously) — remove or delete those parts first, or archive it instead."));
        }

        // Own installation history: safe to drop under Force (those rows feed only this component's mileage, whose
        // projection row goes via ON DELETE CASCADE). Without Force, steer to Archive.
        if (await installationRepo.AnyByComponentAsync(component.Id))
        {
            if (!request.Force)
            {
                return Result.Fail(new ConflictError(
                    "Component has installation history — archive it, or delete it anyway to drop its installation records."));
            }

            foreach (var installation in await installationRepo.GetByComponentAsync(component.Id))
            {
                installationRepo.Delete(installation);
            }
        }

        repo.DeleteComponent(component);
        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}

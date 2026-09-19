using FluentResults;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Notifications.Component;
using MediatR;

namespace KomTracker.Application.Commands.Installation;

/// <summary>Hard-deletes an installation record (corrections / removing a Manual entry).</summary>
public class DeleteInstallationCommand : IRequest<Result>
{
    public string UserId { get; set; } = default!;
    public int InstallationId { get; set; }
}

public class DeleteInstallationCommandHandler : IRequestHandler<DeleteInstallationCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IMediator _mediator;

    public DeleteInstallationCommandHandler(IKOMUnitOfWork komUoW, IMediator mediator)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<Result> Handle(DeleteInstallationCommand request, CancellationToken cancellationToken)
    {
        var installationRepo = _komUoW.GetRepository<IInstallationRepository>();

        var installation = await installationRepo.GetAsync(request.InstallationId);
        if (installation is null)
        {
            return Result.Fail(new NotFoundError($"Installation {request.InstallationId} not found."));
        }

        if (installation.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Installation does not belong to the current user."));
        }

        var componentId = installation.ComponentId;
        installationRepo.Delete(installation);
        await _komUoW.SaveChangesAsync();

        await _mediator.Publish(new InstallationChangedNotification { ComponentId = componentId }, cancellationToken);

        return Result.Ok();
    }
}

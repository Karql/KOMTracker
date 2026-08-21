using FluentResults;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
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

    public DeleteInstallationCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
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

        installationRepo.Delete(installation);
        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}

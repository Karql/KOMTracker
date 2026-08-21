using FluentResults;
using FluentValidation;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using MediatR;

namespace KomTracker.Application.Commands.Installation;

/// <summary>Removes (uninstalls) a component: closes the active Tracked window with a DateTo.</summary>
public class RemoveInstallationCommand : IRequest<Result>
{
    public string UserId { get; set; } = default!;
    public int InstallationId { get; set; }
    public DateTime DateTo { get; set; }
}

public class RemoveInstallationCommandValidator : AbstractValidator<RemoveInstallationCommand>
{
    public RemoveInstallationCommandValidator()
    {
        RuleFor(x => x.InstallationId).GreaterThan(0);
    }
}

public class RemoveInstallationCommandHandler : IRequestHandler<RemoveInstallationCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public RemoveInstallationCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(RemoveInstallationCommand request, CancellationToken cancellationToken)
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

        if (!installation.IsCurrent)
        {
            return Result.Fail(new ConflictError("This installation is not currently active."));
        }

        installation.DateTo = InstallationDateHelper.EnsureUtc(request.DateTo);
        installationRepo.Update(installation);

        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}

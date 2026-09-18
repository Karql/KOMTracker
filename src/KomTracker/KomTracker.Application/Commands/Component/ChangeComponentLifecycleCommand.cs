using FluentResults;
using FluentValidation;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Component;
using MediatR;

namespace KomTracker.Application.Commands.Component;

public class ChangeComponentLifecycleCommand : IRequest<Result>
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public ComponentLifecycle Lifecycle { get; set; }
    public DateTime? SaleDate { get; set; }
    public decimal? SalePrice { get; set; }

    /// <summary>Optional note to save alongside the transition (e.g. why it was archived/sold). Null = leave unchanged.</summary>
    public string? Notes { get; set; }
}

public class ChangeComponentLifecycleCommandValidator : AbstractValidator<ChangeComponentLifecycleCommand>
{
    public ChangeComponentLifecycleCommandValidator()
    {
        RuleFor(x => x.Lifecycle).IsInEnum();

        When(x => x.Lifecycle == ComponentLifecycle.Sold, () =>
        {
            RuleFor(x => x.SaleDate).NotNull();
            RuleFor(x => x.SalePrice).NotNull().GreaterThanOrEqualTo(0);
        });
    }
}

public class ChangeComponentLifecycleCommandHandler : IRequestHandler<ChangeComponentLifecycleCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public ChangeComponentLifecycleCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(ChangeComponentLifecycleCommand request, CancellationToken cancellationToken)
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

        component.Lifecycle = request.Lifecycle;

        if (request.Notes is not null)
        {
            component.Notes = request.Notes;
        }

        if (request.Lifecycle == ComponentLifecycle.Sold)
        {
            component.SaleDate = ComponentDateHelper.EnsureUtc(request.SaleDate);
            component.SalePrice = request.SalePrice;
        }
        else
        {
            // Leaving Sold clears the sale details.
            component.SaleDate = null;
            component.SalePrice = null;
        }

        repo.UpdateComponent(component);

        // D-18: going off-active closes the component's own active windows and cascades/detaches its children.
        if (request.Lifecycle == ComponentLifecycle.Sold)
        {
            await ApplyLifecycleToInstallationsAsync(
                repo, component.Id, component.SaleDate ?? ComponentDateHelper.EnsureUtc(request.SaleDate)!.Value,
                cascadeSoldToChildren: true);
        }
        else if (request.Lifecycle == ComponentLifecycle.Archived)
        {
            await ApplyLifecycleToInstallationsAsync(
                repo, component.Id, DateTime.UtcNow, cascadeSoldToChildren: false);
        }

        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }

    /// <summary>
    /// D-18: close the component's own active Tracked windows at <paramref name="closeDate"/>, then handle children
    /// currently installed into it — either cascade Sold (windows closed + child marked Sold) or detach (windows
    /// closed → child becomes unassigned, lifecycle untouched). One-level nesting means one pass suffices.
    /// </summary>
    private async Task ApplyLifecycleToInstallationsAsync(
        IComponentRepository componentRepo, int componentId, DateTime closeDate, bool cascadeSoldToChildren)
    {
        var installationRepo = _komUoW.GetRepository<IInstallationRepository>();

        // Close the component's own active windows (remove it from wherever it currently sits).
        foreach (var own in await installationRepo.GetActiveTrackedInstallationsByComponentAsync(componentId))
        {
            own.DateTo = closeDate;
            installationRepo.Update(own);
        }

        // Handle still-installed children (components installed INTO this one).
        var children = await installationRepo.GetActiveChildrenByParentComponentsAsync(new[] { componentId });
        foreach (var childInstall in children)
        {
            childInstall.DateTo = closeDate;
            installationRepo.Update(childInstall);

            if (cascadeSoldToChildren)
            {
                var child = await componentRepo.GetComponentAsync(childInstall.ComponentId);
                if (child is not null && child.Lifecycle != ComponentLifecycle.Sold)
                {
                    child.Lifecycle = ComponentLifecycle.Sold;
                    child.SaleDate = closeDate;
                    componentRepo.UpdateComponent(child);
                }
            }
        }
    }
}

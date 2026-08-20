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
        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}

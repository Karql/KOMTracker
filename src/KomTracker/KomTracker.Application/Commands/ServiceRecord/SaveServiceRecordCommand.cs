using FluentResults;
using FluentValidation;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Notifications.ServiceRecord;
using KomTracker.Domain.Entities.ServiceRecord;
using MediatR;

namespace KomTracker.Application.Commands.ServiceRecord;

/// <summary>
/// Creates a service when <see cref="Id"/> is null, otherwise updates the existing one. <see cref="Targets"/> replaces
/// the whole set of what the service covers (bikes and/or components, D-SRV-1). The targets' mileage at the service
/// date is recomputed afterwards via <see cref="ServiceRecordChangedNotification"/>.
/// </summary>
public class SaveServiceRecordCommand : IRequest<Result<ServiceRecordEntity>>
{
    // Server-owned (set by the controller from route/claims), not part of the request body.
    public int? Id { get; set; }
    public string UserId { get; set; } = default!;

    // Editable fields — mirror SaveServiceRecordViewModel (parity test guards drift).
    public string Name { get; set; } = default!;
    public DateTime Date { get; set; }
    public string? Place { get; set; }
    public decimal? Cost { get; set; }
    public string? Notes { get; set; }
    public List<ServiceTargetRef> Targets { get; set; } = new();
}

public class SaveServiceRecordCommandValidator : AbstractValidator<SaveServiceRecordCommand>
{
    public SaveServiceRecordCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Date).NotEmpty();
        RuleFor(x => x.Place).MaximumLength(200);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0).When(x => x.Cost.HasValue);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Targets).NotEmpty().WithMessage("Pick at least one bike or component.");
        RuleForEach(x => x.Targets)
            .Must(t => (t.BikeId is null) != (t.ComponentId is null))
            .WithMessage("Each target is either a bike or a component.");
        RuleFor(x => x.Targets)
            .Must(targets => targets is null || targets.Select(t => (t.BikeId, t.ComponentId)).Distinct().Count() == targets.Count)
            .WithMessage("The same bike or component is listed twice.");
    }
}

public class SaveServiceRecordCommandHandler : IRequestHandler<SaveServiceRecordCommand, Result<ServiceRecordEntity>>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IMediator _mediator;

    public SaveServiceRecordCommandHandler(IKOMUnitOfWork komUoW, IMediator mediator)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<Result<ServiceRecordEntity>> Handle(SaveServiceRecordCommand request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IServiceRecordRepository>();

        var targetsCheck = await CheckTargetsBelongToUserAsync(request);
        if (targetsCheck.IsFailed)
        {
            return targetsCheck;
        }

        ServiceRecordEntity service;

        if (request.Id is null)
        {
            service = new ServiceRecordEntity { UserId = request.UserId };
            Apply(request, service);
            repo.AddServiceRecord(service);
        }
        else
        {
            var existing = await repo.GetServiceRecordAsync(request.Id.Value);

            if (existing is null)
            {
                return Result.Fail(new NotFoundError($"Service {request.Id} not found."));
            }

            if (existing.UserId != request.UserId)
            {
                return Result.Fail(new ForbiddenError("Service does not belong to the current user."));
            }

            Apply(request, existing);
            service = existing;
        }

        await _komUoW.SaveChangesAsync();

        await _mediator.Publish(new ServiceRecordChangedNotification { ServiceId = service.Id }, cancellationToken);

        return Result.Ok(service);
    }

    // Every target must be one of the user's own bikes / components (any lifecycle — a sold bike's past service is fine).
    private async Task<Result> CheckTargetsBelongToUserAsync(SaveServiceRecordCommand request)
    {
        var bikeIds = request.Targets.Where(t => t.BikeId is not null).Select(t => t.BikeId!.Value).Distinct().ToList();
        var componentIds = request.Targets.Where(t => t.ComponentId is not null).Select(t => t.ComponentId!.Value).Distinct().ToList();

        if (bikeIds.Count > 0)
        {
            var bikes = await _komUoW.GetRepository<IBikeRepository>().GetByIdsAsync(bikeIds);
            if (bikes.Count(b => b.UserId == request.UserId) != bikeIds.Count)
            {
                return Result.Fail(new NotFoundError("A bike the service covers was not found."));
            }
        }

        if (componentIds.Count > 0)
        {
            var components = await _komUoW.GetRepository<IComponentRepository>().GetByIdsAsync(componentIds);
            if (components.Count(c => c.UserId == request.UserId) != componentIds.Count)
            {
                return Result.Fail(new NotFoundError("A component the service covers was not found."));
            }
        }

        return Result.Ok();
    }

    private static void Apply(SaveServiceRecordCommand request, ServiceRecordEntity service)
    {
        service.Name = request.Name;
        service.Date = DateTime.SpecifyKind(request.Date, DateTimeKind.Utc);
        service.Place = request.Place;
        service.Cost = request.Cost;
        service.Notes = request.Notes;

        // Replace the target set: keep the rows that stay (their stored mileage is recomputed anyway), drop the
        // removed ones (orphans are deleted), add the new ones.
        var wanted = request.Targets.Select(t => (t.BikeId, t.ComponentId)).ToHashSet();
        service.Targets.RemoveAll(t => !wanted.Contains((t.BikeId, t.ComponentId)));

        var existing = service.Targets.Select(t => (t.BikeId, t.ComponentId)).ToHashSet();
        foreach (var target in request.Targets.Where(t => !existing.Contains((t.BikeId, t.ComponentId))))
        {
            service.Targets.Add(new ServiceTargetEntity { BikeId = target.BikeId, ComponentId = target.ComponentId });
        }
    }
}

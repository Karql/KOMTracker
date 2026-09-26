using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Strava;
using MediatR;

namespace KomTracker.Application.Commands.Strava;

/// <summary>
/// Persist a raw Strava webhook event (D-12 inbox). Permissive — we store whatever arrives for later analysis;
/// no processing here (a future worker drains unprocessed rows).
/// </summary>
public class StoreStravaWebhookEventCommand : IRequest<Result>
{
    public string ObjectType { get; set; } = default!;
    public long ObjectId { get; set; }
    public string AspectType { get; set; } = default!;

    /// <summary>The raw "updates" hash as a JSON string; null when the event carries none.</summary>
    public string? Updates { get; set; }

    public long OwnerId { get; set; }
    public int SubscriptionId { get; set; }
    public long EventTime { get; set; }
}

public class StoreStravaWebhookEventCommandHandler : IRequestHandler<StoreStravaWebhookEventCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public StoreStravaWebhookEventCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(StoreStravaWebhookEventCommand request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IWebhookEventRepository>();

        repo.Add(new WebhookEventEntity
        {
            ObjectType = request.ObjectType,
            ObjectId = request.ObjectId,
            AspectType = request.AspectType,
            Updates = request.Updates,
            OwnerId = request.OwnerId,
            SubscriptionId = request.SubscriptionId,
            EventTime = request.EventTime,
            Processed = false,
            AuditCD = DateTime.UtcNow // received-at (audit is stamped manually in this repo family)
        });

        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}

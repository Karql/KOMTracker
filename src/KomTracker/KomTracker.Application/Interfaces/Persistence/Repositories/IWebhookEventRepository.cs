using KomTracker.Domain.Entities.Strava;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IWebhookEventRepository : IRepository
{
    /// <summary>Stage a raw Strava webhook event (committed by the unit of work).</summary>
    void Add(WebhookEventEntity webhookEvent);

    /// <summary>Unprocessed events, oldest first (drain order). AsNoTracking.</summary>
    Task<IReadOnlyList<WebhookEventEntity>> GetUnprocessedAsync();

    /// <summary>Mark one event processed (immediate, outside the change tracker).</summary>
    Task MarkProcessedAsync(int id);
}

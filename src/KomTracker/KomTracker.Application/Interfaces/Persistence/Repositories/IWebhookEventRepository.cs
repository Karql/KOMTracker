using KomTracker.Domain.Entities.Strava;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IWebhookEventRepository : IRepository
{
    /// <summary>Stage a raw Strava webhook event (committed by the unit of work).</summary>
    void Add(WebhookEventEntity webhookEvent);
}

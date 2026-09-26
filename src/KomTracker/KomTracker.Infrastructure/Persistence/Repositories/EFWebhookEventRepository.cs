using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Strava;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFWebhookEventRepository : EFBaseRepository, IWebhookEventRepository
{
    public void Add(WebhookEventEntity webhookEvent)
    {
        _context.WebhookEvent.Add(webhookEvent);
    }
}

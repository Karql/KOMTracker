using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Strava;
using Microsoft.EntityFrameworkCore;

namespace KomTracker.Infrastructure.Persistence.Repositories;

public class EFWebhookEventRepository : EFBaseRepository, IWebhookEventRepository
{
    public void Add(WebhookEventEntity webhookEvent)
    {
        _context.WebhookEvent.Add(webhookEvent);
    }

    public async Task<IReadOnlyList<WebhookEventEntity>> GetUnprocessedAsync()
    {
        return await _context.WebhookEvent.AsNoTracking()
            .Where(x => !x.Processed)
            .OrderBy(x => x.EventTime)
            .ThenBy(x => x.Id)
            .ToListAsync();
    }

    public async Task MarkProcessedAsync(int id)
    {
        await _context.WebhookEvent
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Processed, true)
                .SetProperty(x => x.AuditMD, DateTime.UtcNow));
    }
}

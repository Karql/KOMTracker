#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Contracts;
using KomTracker.Domain.Entities.Strava;
using KomTracker.Domain.Entities.SystemStats;
using Microsoft.EntityFrameworkCore;

namespace KomTracker.Infrastructure.Persistence.Repositories;

/// <summary>
/// The admin stats snapshot (D-SYS-1): plain counts / max / small top-N queries over the whole database, run one after
/// another (one DbContext). Everything is read-only and untracked.
/// </summary>
public class EFSystemStatsRepository : EFBaseRepository, ISystemStatsRepository
{
    private const int ListSize = 10;
    private const int FeedSize = 15;
    private const int GrowthMonths = 24;

    public async Task<SystemStatsReport> GetReportAsync(DateTime now)
    {
        var recent = now.AddDays(-SystemStatsReport.RecentDays);

        var users = await GetUsersAsync();
        var byUserId = users.ToDictionary(u => u.UserId);

        return new SystemStatsReport
        {
            GeneratedAt = now,
            Counts = await GetCountsAsync(users, now, recent),
            NewUsers = users.Where(u => u.Joined is not null).OrderByDescending(u => u.Joined).Take(ListSize).Select(u => u.ToSystemUser()).ToList(),
            ActiveUsers = users.Where(u => u.LastSeen is not null).OrderByDescending(u => u.LastSeen).Take(ListSize).Select(u => u.ToSystemUser()).ToList(),
            BikeTrackerUsers = await GetBikeTrackerUsersAsync(byUserId),
            RecentItems = await GetRecentItemsAsync(byUserId, recent),
            Health = await GetHealthAsync(users, now),
            UsersByMonth = SystemMonth.Build(users.Where(u => u.Joined is not null).Select(u => u.Joined!.Value), now, GrowthMonths)
        };
    }

    // ---- users ---------------------------------------------------------------------------------------------------

    private sealed record UserRow(string UserId, int AthleteId, string Name, string? Avatar, DateTime? Joined, DateTime? LastSeen, DateTime? LastLogin)
    {
        public SystemUser ToSystemUser() => new()
        {
            AthleteId = AthleteId, Name = Name, Avatar = Avatar, Joined = Joined, LastSeen = LastSeen, LastLogin = LastLogin
        };
    }

    // Every user with their athlete (name, avatar, first connect) — a few hundred rows at most, so the lists are cut in memory.
    private async Task<List<UserRow>> GetUsersAsync()
    {
        var rows = await (
                from u in _context.Users.AsNoTracking()
                join a in _context.Athlete.AsNoTracking() on u.AthleteId equals a.AthleteId into athletes
                from a in athletes.DefaultIfEmpty()
                select new
                {
                    u.Id, u.AthleteId, u.UserName, u.LastSeenAt, u.LastLoginAt,
                    FirstName = a != null ? a.FirstName : null,
                    LastName = a != null ? a.LastName : null,
                    Avatar = a != null ? a.ProfileMedium : null,
                    Joined = a != null ? (DateTime?)a.AuditCD : null
                })
            .ToListAsync();

        return rows
            .Select(r => new UserRow(r.Id, r.AthleteId, DisplayName(r.FirstName, r.LastName, r.UserName), r.Avatar, r.Joined, r.LastSeenAt, r.LastLoginAt))
            .ToList();
    }

    private static string DisplayName(string? firstName, string? lastName, string? userName)
    {
        var name = $"{firstName} {lastName}".Trim();
        return name.Length > 0 ? name : userName ?? "?";
    }

    // ---- counts --------------------------------------------------------------------------------------------------

    private async Task<SystemCounts> GetCountsAsync(List<UserRow> users, DateTime now, DateTime recent)
    {
        var counts = new SystemCounts
        {
            Users = new SystemCount { Total = users.Count, Recent = users.Count(u => u.Joined >= recent) },
            ActiveDay = users.Count(u => u.LastSeen >= now.AddDays(-1)),
            ActiveWeek = users.Count(u => u.LastSeen >= now.AddDays(-7)),
            ActiveMonth = users.Count(u => u.LastSeen >= recent),

            Segments = await CountAsync(_context.Segment, recent),
            Clubs = await CountAsync(_context.Club, recent),
            StravaBikes = await CountAsync(_context.StravaBike, recent),
            Bikes = await CountAsync(_context.Bike, recent),
            Components = await CountAsync(_context.Component, recent),
            Installations = await CountAsync(_context.Installation, recent),
            Warehouses = await CountAsync(_context.Warehouse, recent),
            Services = await CountAsync(_context.ServiceRecord, recent),
            Expenses = await CountAsync(_context.Expense, recent),

            Activities = new SystemCount
            {
                Total = await _context.Activity.CountAsync(),
                Recent = await _context.Activity.CountAsync(a => a.StartDate >= recent)
            },
            ActivityKm = (decimal)(await _context.Activity.SumAsync(a => a.Distance) / 1000d),
            RecentActivityKm = (decimal)(await _context.Activity.Where(a => a.StartDate >= recent).SumAsync(a => a.Distance) / 1000d),

            Takeovers = await _context.KomTakeover.CountAsync(t => t.TrackDate >= recent)
        };

        // KOMs held now: each athlete's latest summary (one per athlete — summed in memory).
        var latest = await _context.KomsSummary
            .GroupBy(s => s.AthleteId)
            .Select(g => g.OrderByDescending(s => s.TrackDate).Select(s => s.Koms).First())
            .ToListAsync();
        counts.KomsHeld = latest.Sum();

        var changes = await _context.KomsSummary
            .Where(s => s.TrackDate >= recent)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                New = g.Sum(s => s.NewKoms),
                Improved = g.Sum(s => s.ImprovedKoms),
                Lost = g.Sum(s => s.LostKoms),
                Returned = g.Sum(s => s.ReturnedKoms)
            })
            .FirstOrDefaultAsync();
        if (changes is not null)
        {
            counts.NewKoms = changes.New;
            counts.ImprovedKoms = changes.Improved;
            counts.LostKoms = changes.Lost;
            counts.ReturnedKoms = changes.Returned;
        }

        return counts;
    }

    private static async Task<SystemCount> CountAsync<T>(IQueryable<T> rows, DateTime recent) where T : BaseEntity
        => new()
        {
            Total = await rows.CountAsync(),
            Recent = await rows.CountAsync(x => x.AuditCD >= recent)
        };

    // ---- Bike Tracker usage --------------------------------------------------------------------------------------

    private sealed record UsageRow(string UserId, int Count, DateTime LastChange);

    private static async Task<List<UsageRow>> UsageAsync<T>(IQueryable<T> rows, System.Linq.Expressions.Expression<Func<T, string>> userId)
        where T : BaseEntity
        => await rows
            .GroupBy(userId)
            .Select(g => new UsageRow(g.Key, g.Count(), g.Max(x => x.AuditMD ?? x.AuditCD)))
            .ToListAsync();

    private async Task<List<SystemBikeTrackerUser>> GetBikeTrackerUsersAsync(Dictionary<string, UserRow> users)
    {
        var bikes = await UsageAsync(_context.Bike, x => x.UserId);
        var components = await UsageAsync(_context.Component, x => x.UserId);
        var installations = await UsageAsync(_context.Installation, x => x.UserId);
        var services = await UsageAsync(_context.ServiceRecord, x => x.UserId);
        var expenses = await UsageAsync(_context.Expense, x => x.UserId);
        var warehouses = await UsageAsync(_context.Warehouse, x => x.UserId);

        int CountOf(List<UsageRow> rows, string userId) => rows.FirstOrDefault(r => r.UserId == userId)?.Count ?? 0;

        return bikes.Concat(components).Concat(installations).Concat(services).Concat(expenses).Concat(warehouses)
            .GroupBy(r => r.UserId)
            .Select(g =>
            {
                var user = users.GetValueOrDefault(g.Key);
                return new SystemBikeTrackerUser
                {
                    AthleteId = user?.AthleteId ?? 0,
                    Name = user?.Name ?? "?",
                    Avatar = user?.Avatar,
                    Bikes = CountOf(bikes, g.Key),
                    Components = CountOf(components, g.Key),
                    Installations = CountOf(installations, g.Key),
                    Services = CountOf(services, g.Key),
                    Expenses = CountOf(expenses, g.Key),
                    Warehouses = CountOf(warehouses, g.Key),
                    LastChange = g.Max(r => r.LastChange)
                };
            })
            .OrderByDescending(u => u.LastChange)
            .ToList();
    }

    // ---- recent Bike Tracker items -------------------------------------------------------------------------------

    private async Task<List<SystemRecentItem>> GetRecentItemsAsync(Dictionary<string, UserRow> users, DateTime recent)
    {
        var items = new List<(string UserId, SystemRecentItem Item)>();

        items.AddRange((await _context.Bike.Where(x => x.AuditCD >= recent).OrderByDescending(x => x.AuditCD).Take(FeedSize)
                .Select(x => new { x.UserId, x.Name, x.AuditCD }).ToListAsync())
            .Select(x => (x.UserId, new SystemRecentItem { Type = SystemItemType.Bike, Name = x.Name, Added = x.AuditCD })));

        items.AddRange((await _context.Component.Where(x => x.AuditCD >= recent).OrderByDescending(x => x.AuditCD).Take(FeedSize)
                .Select(x => new { x.UserId, x.Name, x.Category, x.AuditCD }).ToListAsync())
            .Select(x => (x.UserId, new SystemRecentItem { Type = SystemItemType.Component, Name = x.Name, Detail = Domain.Entities.Component.ComponentCategoryMetadata.DisplayName(x.Category), Added = x.AuditCD })));

        items.AddRange((await (
                    from i in _context.Installation
                    where i.AuditCD >= recent
                    join c in _context.Component on i.ComponentId equals c.Id
                    join b in _context.Bike on i.BikeId equals b.Id into bikes
                    from b in bikes.DefaultIfEmpty()
                    join p in _context.Component on i.ParentComponentId equals p.Id into parents
                    from p in parents.DefaultIfEmpty()
                    orderby i.AuditCD descending
                    select new { i.UserId, Component = c.Name, Bike = b != null ? b.Name : null, Parent = p != null ? p.Name : null, i.AuditCD })
                .Take(FeedSize).ToListAsync())
            .Select(x => (x.UserId, new SystemRecentItem
            {
                Type = SystemItemType.Installation, Name = x.Component, Detail = (x.Bike ?? x.Parent) is { } on ? $"on {on}" : null, Added = x.AuditCD
            })));

        items.AddRange((await _context.ServiceRecord.Where(x => x.AuditCD >= recent).OrderByDescending(x => x.AuditCD).Take(FeedSize)
                .Select(x => new { x.UserId, x.Name, x.AuditCD }).ToListAsync())
            .Select(x => (x.UserId, new SystemRecentItem { Type = SystemItemType.Service, Name = x.Name, Added = x.AuditCD })));

        items.AddRange((await _context.Expense.Where(x => x.AuditCD >= recent).OrderByDescending(x => x.AuditCD).Take(FeedSize)
                .Select(x => new { x.UserId, x.Name, x.Category, x.AuditCD }).ToListAsync())
            .Select(x => (x.UserId, new SystemRecentItem { Type = SystemItemType.Expense, Name = x.Name, Detail = Domain.Entities.Expense.ExpenseCategoryMetadata.DisplayName(x.Category), Added = x.AuditCD })));

        items.AddRange((await _context.Warehouse.Where(x => x.AuditCD >= recent).OrderByDescending(x => x.AuditCD).Take(FeedSize)
                .Select(x => new { x.UserId, x.Name, x.AuditCD }).ToListAsync())
            .Select(x => (x.UserId, new SystemRecentItem { Type = SystemItemType.Warehouse, Name = x.Name, Added = x.AuditCD })));

        return items
            .OrderByDescending(x => x.Item.Added)
            .Take(FeedSize)
            .Select(x =>
            {
                x.Item.User = users.GetValueOrDefault(x.UserId)?.Name ?? "?";
                return x.Item;
            })
            .ToList();
    }

    // ---- health --------------------------------------------------------------------------------------------------

    private async Task<SystemHealth> GetHealthAsync(List<UserRow> users, DateTime now)
    {
        var week = now.AddDays(-7);
        var health = new SystemHealth
        {
            WebhooksDay = await _context.WebhookEvent.CountAsync(e => e.AuditCD >= now.AddDays(-1)),
            WebhooksWeek = await _context.WebhookEvent.CountAsync(e => e.AuditCD >= week),
            WebhooksPending = await _context.WebhookEvent.CountAsync(e => !e.Processed),
            OldestPendingWebhook = await _context.WebhookEvent.Where(e => !e.Processed).MinAsync(e => (DateTime?)e.AuditCD),

            LastJobSync = await _context.ActivitySyncHistory.Where(h => h.Type == ActivitySyncType.Job).MaxAsync(h => (DateTime?)h.RunAt),

            LastKomChange = await _context.KomsSummary.MaxAsync(s => (DateTime?)s.TrackDate),
            LastSegmentRefresh = await _context.Segment.MaxAsync(s => s.AuditMD),
            LastAthleteRefresh = await _context.Athlete.MaxAsync(a => a.AuditMD),
            LastStatsRefresh = await _context.AthleteStats.MaxAsync(s => s.AuditMD),
            LastClubRefresh = await _context.Club.MaxAsync(c => c.AuditMD)
        };

        health.Syncs = (await _context.ActivitySyncHistory
                .Where(h => h.RunAt >= week)
                .GroupBy(h => new { h.Type, h.Status })
                .Select(g => new { g.Key.Type, g.Key.Status, Count = g.Count() })
                .ToListAsync())
            .Select(x => new SystemSyncCount { Type = x.Type.ToString(), Status = x.Status, Count = x.Count })
            .OrderBy(x => x.Type).ThenByDescending(x => x.Count)
            .ToList();

        // Athletes whose latest scheduled sync found no valid token — they revoked access or the token died.
        var latestJobStatus = await _context.ActivitySyncHistory
            .Where(h => h.Type == ActivitySyncType.Job)
            .GroupBy(h => h.AthleteId)
            .Select(g => g.OrderByDescending(h => h.RunAt).Select(h => new { h.AthleteId, h.Status }).First())
            .ToListAsync();
        var lost = latestJobStatus.Where(x => x.Status == "NoValidToken").Select(x => x.AthleteId).ToHashSet();
        health.LostStravaAccess = users.Where(u => lost.Contains(u.AthleteId)).OrderBy(u => u.Name).Select(u => u.ToSystemUser()).ToList();

        return health;
    }
}

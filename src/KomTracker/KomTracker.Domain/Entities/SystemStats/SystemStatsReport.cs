#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.SystemStats;

/// <summary>
/// The admin "System → Stats" snapshot (D-SYS-1): what's in the app, who joined / is active, who uses Bike Tracker and
/// how healthy the background syncs are. A read model — it is also the wire format (no view-model copy, D-SYS-5).
/// "Recent" means the last <see cref="RecentDays"/> days.
/// </summary>
public class SystemStatsReport
{
    public const int RecentDays = 30;

    public DateTime GeneratedAt { get; set; }
    public SystemCounts Counts { get; set; } = new();

    /// <summary>Newest users first (joined = first Strava connect, D-SYS-3).</summary>
    public List<SystemUser> NewUsers { get; set; } = new();

    /// <summary>Most recently active users first (by last seen).</summary>
    public List<SystemUser> ActiveUsers { get; set; } = new();

    /// <summary>Every user with any Bike Tracker data, most recently changed first.</summary>
    public List<SystemBikeTrackerUser> BikeTrackerUsers { get; set; } = new();

    /// <summary>The newest Bike Tracker items added in the recent window, newest first.</summary>
    public List<SystemRecentItem> RecentItems { get; set; } = new();

    public SystemHealth Health { get; set; } = new();

    /// <summary>Users joined per month and the running total, oldest month first.</summary>
    public List<SystemMonth> UsersByMonth { get; set; } = new();
}

/// <summary>A total and how many of them were added in the recent window.</summary>
public class SystemCount
{
    public int Total { get; set; }
    public int Recent { get; set; }
}

public class SystemCounts
{
    public SystemCount Users { get; set; } = new();
    public int ActiveDay { get; set; }
    public int ActiveWeek { get; set; }
    public int ActiveMonth { get; set; }

    public SystemCount Segments { get; set; } = new();
    public SystemCount Clubs { get; set; } = new();

    /// <summary>KOMs held right now — the sum of every athlete's latest KOM summary.</summary>
    public int KomsHeld { get; set; }

    // KOM changes in the recent window (from the KOM summaries).
    public int NewKoms { get; set; }
    public int ImprovedKoms { get; set; }
    public int LostKoms { get; set; }
    public int ReturnedKoms { get; set; }

    /// <summary>KOMs taken from another app user in the recent window.</summary>
    public int Takeovers { get; set; }

    /// <summary>Synced activities; recent = started in the window.</summary>
    public SystemCount Activities { get; set; } = new();
    public decimal ActivityKm { get; set; }
    public decimal RecentActivityKm { get; set; }
    public SystemCount StravaBikes { get; set; } = new();

    // Bike Tracker
    public SystemCount Bikes { get; set; } = new();
    public SystemCount Components { get; set; } = new();
    public SystemCount Installations { get; set; } = new();
    public SystemCount Warehouses { get; set; } = new();
    public SystemCount Services { get; set; } = new();
    public SystemCount Expenses { get; set; } = new();
}

public class SystemUser
{
    public int AthleteId { get; set; }
    public string Name { get; set; } = default!;
    public string? Avatar { get; set; }

    /// <summary>First Strava connect (the athlete row's creation).</summary>
    public DateTime? Joined { get; set; }
    public DateTime? LastSeen { get; set; }
    public DateTime? LastLogin { get; set; }
}

public class SystemBikeTrackerUser
{
    public int AthleteId { get; set; }
    public string Name { get; set; } = default!;
    public string? Avatar { get; set; }
    public int Bikes { get; set; }
    public int Components { get; set; }
    public int Installations { get; set; }
    public int Services { get; set; }
    public int Expenses { get; set; }
    public int Warehouses { get; set; }

    /// <summary>The latest create / edit of any of the user's Bike Tracker rows.</summary>
    public DateTime? LastChange { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SystemItemType
{
    Bike,
    Component,
    Installation,
    Service,
    Expense,
    Warehouse
}

public class SystemRecentItem
{
    public SystemItemType Type { get; set; }
    public string Name { get; set; } = default!;

    /// <summary>A short context, e.g. where a component was installed.</summary>
    public string? Detail { get; set; }
    public string User { get; set; } = default!;
    public DateTime Added { get; set; }
}

public class SystemHealth
{
    public int WebhooksDay { get; set; }
    public int WebhooksWeek { get; set; }
    public int WebhooksPending { get; set; }
    public DateTime? OldestPendingWebhook { get; set; }

    /// <summary>Activity sync runs of the last 7 days by type + status.</summary>
    public List<SystemSyncCount> Syncs { get; set; } = new();
    public DateTime? LastJobSync { get; set; }

    /// <summary>Athletes whose latest scheduled sync found no valid Strava token (access revoked / expired).</summary>
    public List<SystemUser> LostStravaAccess { get; set; } = new();

    // Freshness of what the scheduled jobs refresh.
    public DateTime? LastKomChange { get; set; }
    public DateTime? LastSegmentRefresh { get; set; }
    public DateTime? LastAthleteRefresh { get; set; }
    public DateTime? LastStatsRefresh { get; set; }
    public DateTime? LastClubRefresh { get; set; }
}

public class SystemSyncCount
{
    public string Type { get; set; } = default!;
    public string Status { get; set; } = default!;
    public int Count { get; set; }
}

public class SystemMonth
{
    /// <summary>The first day of the month.</summary>
    public DateTime Month { get; set; }
    public int Joined { get; set; }
    public int Total { get; set; }

    /// <summary>
    /// The last <paramref name="months"/> months up to <paramref name="now"/>'s: users joined in each and the running
    /// total, counting everyone who joined before the first month too.
    /// </summary>
    public static List<SystemMonth> Build(IEnumerable<DateTime> joined, DateTime now, int months)
    {
        var first = new DateTime(now.Year, now.Month, 1).AddMonths(-(months - 1));
        var dates = joined.ToList();
        var total = dates.Count(d => d < first);

        return Enumerable.Range(0, months)
            .Select(i =>
            {
                var month = first.AddMonths(i);
                var count = dates.Count(d => d >= month && d < month.AddMonths(1));
                total += count;
                return new SystemMonth { Month = month, Joined = count, Total = total };
            })
            .ToList();
    }
}

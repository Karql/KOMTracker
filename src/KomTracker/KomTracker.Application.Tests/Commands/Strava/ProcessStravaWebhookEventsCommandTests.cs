#nullable enable
using FluentResults;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Strava;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Strava;

public class ProcessStravaWebhookEventsCommandTests
{
    private const int AthleteId = 7;
    private const long ActivityId = 123;

    private readonly IKOMUnitOfWork _komUoW = Substitute.For<IKOMUnitOfWork>();
    private readonly IWebhookEventRepository _webhookRepo = Substitute.For<IWebhookEventRepository>();
    private readonly IAthleteSyncRepository _syncRepo = Substitute.For<IAthleteSyncRepository>();
    private readonly IActivitySyncHistoryRepository _historyRepo = Substitute.For<IActivitySyncHistoryRepository>();
    private readonly IStravaActivitySyncService _activitySyncService = Substitute.For<IStravaActivitySyncService>();
    private readonly ProcessStravaWebhookEventsCommandHandler _handler;

    public ProcessStravaWebhookEventsCommandTests()
    {
        _komUoW.GetRepository<IWebhookEventRepository>().Returns(_webhookRepo);
        _komUoW.GetRepository<IAthleteSyncRepository>().Returns(_syncRepo);
        _komUoW.GetRepository<IActivitySyncHistoryRepository>().Returns(_historyRepo);
        _handler = new ProcessStravaWebhookEventsCommandHandler(_komUoW, _activitySyncService,
            Substitute.For<ILogger<ProcessStravaWebhookEventsCommandHandler>>());
    }

    private void Enabled() => _syncRepo.GetAsync(AthleteId).Returns(new AthleteSyncEntity { AthleteId = AthleteId, ActivitiesEnabled = true });

    private void Inbox(params WebhookEventEntity[] events) => _webhookRepo.GetUnprocessedAsync().Returns(events);

    private static WebhookEventEntity Event(string objectType, string aspect, int id = 1) => new()
    {
        Id = id, ObjectType = objectType, AspectType = aspect, ObjectId = ActivityId, OwnerId = AthleteId
    };

    [Fact]
    public async Task Create_syncs_writes_webhook_history_and_marks_processed()
    {
        Enabled();
        Inbox(Event("activity", "create", 11));
        _activitySyncService.SyncAthleteActivityAsync(AthleteId, ActivityId, Arg.Any<CancellationToken>()).Returns(Result.Ok());

        await _handler.Handle(new ProcessStravaWebhookEventsCommand(), CancellationToken.None);

        await _activitySyncService.Received().SyncAthleteActivityAsync(AthleteId, ActivityId, Arg.Any<CancellationToken>());
        _historyRepo.Received().Add(Arg.Is<ActivitySyncHistoryEntity>(h =>
            h.Type == ActivitySyncType.Webhook && h.ActivityId == ActivityId && h.AthleteId == AthleteId));
        await _webhookRepo.Received().MarkProcessedAsync(11);
    }

    [Fact]
    public async Task Delete_removes_activity_and_marks_processed()
    {
        Enabled();
        Inbox(Event("activity", "delete", 12));
        _activitySyncService.DeleteAthleteActivityAsync(AthleteId, ActivityId, Arg.Any<CancellationToken>()).Returns(Result.Ok());

        await _handler.Handle(new ProcessStravaWebhookEventsCommand(), CancellationToken.None);

        await _activitySyncService.Received().DeleteAthleteActivityAsync(AthleteId, ActivityId, Arg.Any<CancellationToken>());
        await _webhookRepo.Received().MarkProcessedAsync(12);
    }

    [Fact]
    public async Task Disabled_athlete_is_skipped_and_marked_processed()
    {
        _syncRepo.GetAsync(AthleteId).Returns((AthleteSyncEntity?)null);
        Inbox(Event("activity", "create", 13));

        await _handler.Handle(new ProcessStravaWebhookEventsCommand(), CancellationToken.None);

        await _activitySyncService.DidNotReceiveWithAnyArgs().SyncAthleteActivityAsync(default, default, default);
        await _webhookRepo.Received().MarkProcessedAsync(13);
    }

    [Fact]
    public async Task Non_activity_event_is_marked_processed_without_sync()
    {
        Inbox(Event("athlete", "update", 14));

        await _handler.Handle(new ProcessStravaWebhookEventsCommand(), CancellationToken.None);

        await _activitySyncService.DidNotReceiveWithAnyArgs().SyncAthleteActivityAsync(default, default, default);
        await _webhookRepo.Received().MarkProcessedAsync(14);
    }

    [Fact]
    public async Task Transient_failure_leaves_event_unprocessed()
    {
        Enabled();
        Inbox(Event("activity", "update", 15));
        _activitySyncService.SyncAthleteActivityAsync(AthleteId, ActivityId, Arg.Any<CancellationToken>())
            .Returns(Result.Fail("rate limited"));

        await _handler.Handle(new ProcessStravaWebhookEventsCommand(), CancellationToken.None);

        await _webhookRepo.DidNotReceive().MarkProcessedAsync(15);
        _historyRepo.DidNotReceiveWithAnyArgs().Add(default!);
    }

    [Fact]
    public async Task Not_found_is_terminal_and_marks_processed()
    {
        Enabled();
        Inbox(Event("activity", "update", 16));
        _activitySyncService.SyncAthleteActivityAsync(AthleteId, ActivityId, Arg.Any<CancellationToken>())
            .Returns(Result.Fail(new NotFoundError("gone")));

        await _handler.Handle(new ProcessStravaWebhookEventsCommand(), CancellationToken.None);

        await _webhookRepo.Received().MarkProcessedAsync(16);
        _historyRepo.Received().Add(Arg.Is<ActivitySyncHistoryEntity>(h => h.Status == "NotFound"));
    }
}

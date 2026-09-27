#nullable enable
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Strava;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Strava;

public class SyncActivityCommandHandlerTests
{
    private const int AthleteId = 7;
    private const long ActivityId = 19598505831;

    private readonly IKOMUnitOfWork _komUoW = Substitute.For<IKOMUnitOfWork>();
    private readonly IStravaActivitySyncService _activitySyncService = Substitute.For<IStravaActivitySyncService>();
    private readonly IActivitySyncHistoryRepository _historyRepo = Substitute.For<IActivitySyncHistoryRepository>();
    private readonly SyncActivityCommandHandler _handler;

    public SyncActivityCommandHandlerTests()
    {
        _komUoW.GetRepository<IActivitySyncHistoryRepository>().Returns(_historyRepo);
        _handler = new SyncActivityCommandHandler(_komUoW, _activitySyncService);
    }

    [Fact]
    public async Task Delegates_and_records_a_manual_history_row_on_success()
    {
        _activitySyncService.SyncAthleteActivityAsync(AthleteId, ActivityId, Arg.Any<CancellationToken>()).Returns(Result.Ok());

        var res = await _handler.Handle(new SyncActivityCommand { AthleteId = AthleteId, ActivityId = ActivityId }, CancellationToken.None);

        res.Should().BeSuccess();
        await _activitySyncService.Received().SyncAthleteActivityAsync(AthleteId, ActivityId, Arg.Any<CancellationToken>());
        _historyRepo.Received().Add(Arg.Is<ActivitySyncHistoryEntity>(h =>
            h.Type == ActivitySyncType.Manual && h.ActivityId == ActivityId && h.AthleteId == AthleteId && h.Status == "Ok"));
        await _komUoW.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task Not_found_still_records_a_manual_history_row()
    {
        _activitySyncService.SyncAthleteActivityAsync(AthleteId, ActivityId, Arg.Any<CancellationToken>())
            .Returns(Result.Fail(new NotFoundError("gone")));

        var res = await _handler.Handle(new SyncActivityCommand { AthleteId = AthleteId, ActivityId = ActivityId }, CancellationToken.None);

        res.Should().BeFailure();
        _historyRepo.Received().Add(Arg.Is<ActivitySyncHistoryEntity>(h => h.Type == ActivitySyncType.Manual && h.Status == "NotFound"));
    }

    [Fact]
    public async Task Transient_failure_is_surfaced_without_a_history_row()
    {
        _activitySyncService.SyncAthleteActivityAsync(AthleteId, ActivityId, Arg.Any<CancellationToken>()).Returns(Result.Fail("rate limited"));

        var res = await _handler.Handle(new SyncActivityCommand { AthleteId = AthleteId, ActivityId = ActivityId }, CancellationToken.None);

        res.Should().BeFailure();
        _historyRepo.DidNotReceiveWithAnyArgs().Add(default!);
    }
}

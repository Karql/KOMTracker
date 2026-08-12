#nullable enable
using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Strava;
using KomTracker.Domain.Entities.Token;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using IStravaActivityService = KomTracker.Application.Interfaces.Services.Strava.IActivityService;
using StravaActivitiesError = KomTracker.Application.Interfaces.Services.Strava.GetAthleteActivitiesError;

namespace KomTracker.Application.Tests.Commands.Strava;

public class SyncActivitiesCommandHandlerTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteService _athleteService;
    private readonly IStravaActivityService _activityService;
    private readonly IAthleteSyncRepository _athleteSyncRepo;
    private readonly IActivityRepository _activityRepo;
    private readonly IActivitySyncHistoryRepository _historyRepo;
    private readonly SyncActivitiesCommandHandler _handler;

    private const string ScopePartial = "read,activity:read,profile:read_all";

    public SyncActivitiesCommandHandlerTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _athleteService = Substitute.For<IAthleteService>();
        _activityService = Substitute.For<IStravaActivityService>();
        _athleteSyncRepo = Substitute.For<IAthleteSyncRepository>();
        _activityRepo = Substitute.For<IActivityRepository>();
        _historyRepo = Substitute.For<IActivitySyncHistoryRepository>();

        _komUoW.GetRepository<IAthleteSyncRepository>().Returns(_athleteSyncRepo);
        _komUoW.GetRepository<IActivityRepository>().Returns(_activityRepo);
        _komUoW.GetRepository<IActivitySyncHistoryRepository>().Returns(_historyRepo);

        _handler = new SyncActivitiesCommandHandler(_komUoW, _athleteService, _activityService,
            Substitute.For<ILogger<SyncActivitiesCommandHandler>>());
    }

    private void SetupAthlete(int athleteId, string scope = ScopePartial)
    {
        _athleteSyncRepo.GetActivitiesEnabledAthleteIdsAsync().Returns(new[] { athleteId }.AsEnumerable());
        _athleteService.GetValidTokenAsync(athleteId).Returns(Result.Ok(new TokenEntity { AccessToken = "t", Scope = scope }));
        _activityService.GetAthleteActivitiesAsync(athleteId, "t", Arg.Any<long?>())
            .Returns(Result.Ok(new List<ActivityEntity> { new() { Id = 1, AthleteId = athleteId } }.AsEnumerable()));
        _activityRepo.UpsertAthleteActivitiesAsync(athleteId, Arg.Any<IReadOnlyCollection<ActivityEntity>>(), Arg.Any<DateTime?>())
            .Returns(0);
        _activityRepo.CountAthleteActivitiesAsync(athleteId).Returns(42);
    }

    [Fact]
    public async Task Processes_enabled_athletes_and_upserts()
    {
        _athleteSyncRepo.GetActivitiesEnabledAthleteIdsAsync().Returns(new[] { 1, 2 }.AsEnumerable());
        foreach (var id in new[] { 1, 2 })
        {
            _athleteService.GetValidTokenAsync(id).Returns(Result.Ok(new TokenEntity { AccessToken = "t", Scope = ScopePartial }));
            _activityService.GetAthleteActivitiesAsync(id, "t", Arg.Any<long?>())
                .Returns(Result.Ok(new List<ActivityEntity>().AsEnumerable()));
        }

        var res = await _handler.Handle(new SyncActivitiesCommand { After = null }, CancellationToken.None);

        res.Should().BeSuccess();
        await _activityRepo.Received().UpsertAthleteActivitiesAsync(1, Arg.Any<IReadOnlyCollection<ActivityEntity>>(), null);
        await _activityRepo.Received().UpsertAthleteActivitiesAsync(2, Arg.Any<IReadOnlyCollection<ActivityEntity>>(), null);
    }

    [Fact]
    public async Task Stops_whole_run_on_429()
    {
        _athleteSyncRepo.GetActivitiesEnabledAthleteIdsAsync().Returns(new[] { 1, 2 }.AsEnumerable());
        _athleteService.GetValidTokenAsync(Arg.Any<int>()).Returns(Result.Ok(new TokenEntity { AccessToken = "t", Scope = ScopePartial }));
        _activityService.GetAthleteActivitiesAsync(1, "t", Arg.Any<long?>())
            .Returns(Result.Fail<IEnumerable<ActivityEntity>>(new StravaActivitiesError(StravaActivitiesError.TooManyRequests)));

        var res = await _handler.Handle(new SyncActivitiesCommand(), CancellationToken.None);

        res.Should().BeFailure();
        await _activityService.DidNotReceive().GetAthleteActivitiesAsync(2, Arg.Any<string>(), Arg.Any<long?>());
        await _activityRepo.DidNotReceiveWithAnyArgs().UpsertAthleteActivitiesAsync(default, default!, default);
        _historyRepo.Received().Add(Arg.Is<ActivitySyncHistoryEntity>(h => h.Status == "RateLimited"));
    }

    [Fact]
    public async Task Full_sync_records_ok_history_with_null_syncFrom()
    {
        SetupAthlete(1);

        var res = await _handler.Handle(new SyncActivitiesCommand { After = null }, CancellationToken.None);

        res.Should().BeSuccess();
        await _activityRepo.Received().UpsertAthleteActivitiesAsync(1, Arg.Any<IReadOnlyCollection<ActivityEntity>>(), null);
        _historyRepo.Received().Add(Arg.Is<ActivitySyncHistoryEntity>(h =>
            h.AthleteId == 1 && h.Status == "Ok" && h.SyncFrom == null && h.UpsertedCount == 1
            && h.ActivitiesCount == 42 && h.RunAt != default));
    }

    [Fact]
    public async Task Windowed_sync_passes_After_and_records_syncFrom()
    {
        var after = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        SetupAthlete(1);

        await _handler.Handle(new SyncActivitiesCommand { After = after }, CancellationToken.None);

        await _activityRepo.Received().UpsertAthleteActivitiesAsync(1, Arg.Any<IReadOnlyCollection<ActivityEntity>>(), after);
        _historyRepo.Received().Add(Arg.Is<ActivitySyncHistoryEntity>(h => h.Status == "Ok" && h.SyncFrom == after));
    }
}

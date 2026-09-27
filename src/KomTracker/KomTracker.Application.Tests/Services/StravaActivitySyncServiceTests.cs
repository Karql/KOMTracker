#nullable enable
using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Notifications.Strava;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Strava;
using KomTracker.Domain.Entities.Token;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using IStravaActivityService = KomTracker.Application.Interfaces.Services.Strava.IActivityService;
using StravaActivitiesError = KomTracker.Application.Interfaces.Services.Strava.GetAthleteActivitiesError;

namespace KomTracker.Application.Tests.Services;

public class StravaActivitySyncServiceTests
{
    private const int AthleteId = 7;
    private const long ActivityId = 19598505831;

    private readonly IKOMUnitOfWork _komUoW = Substitute.For<IKOMUnitOfWork>();
    private readonly IAthleteService _athleteService = Substitute.For<IAthleteService>();
    private readonly IStravaActivityService _activityService = Substitute.For<IStravaActivityService>();
    private readonly IActivityRepository _activityRepo = Substitute.For<IActivityRepository>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly StravaActivitySyncService _service;

    public StravaActivitySyncServiceTests()
    {
        _komUoW.GetRepository<IActivityRepository>().Returns(_activityRepo);
        _service = new StravaActivitySyncService(_komUoW, _athleteService, _activityService, _mediator,
            Substitute.For<ILogger<StravaActivitySyncService>>());
    }

    private void SetupToken() =>
        _athleteService.GetValidTokenAsync(AthleteId).Returns(Result.Ok(new TokenEntity { AccessToken = "t" }));

    [Fact]
    public async Task Sync_fetches_upserts_and_publishes()
    {
        SetupToken();
        var entity = new ActivityEntity { Id = ActivityId, AthleteId = AthleteId, GearId = "b1" };
        _activityService.GetAthleteActivityAsync(AthleteId, "t", ActivityId).Returns(Result.Ok(entity));

        var res = await _service.SyncAthleteActivityAsync(AthleteId, ActivityId);

        res.Should().BeSuccess();
        await _activityRepo.Received().UpsertActivityAsync(entity);
        await _mediator.Received().Publish(
            Arg.Is<ActivitySyncedNotification>(n => n.AthleteId == AthleteId && n.ActivityId == ActivityId && n.GearId == "b1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sync_without_token_does_not_fetch_or_upsert()
    {
        _athleteService.GetValidTokenAsync(AthleteId).Returns(Result.Fail<TokenEntity>("no token"));

        var res = await _service.SyncAthleteActivityAsync(AthleteId, ActivityId);

        res.Should().BeFailure();
        await _activityService.DidNotReceiveWithAnyArgs().GetAthleteActivityAsync(default, default!, default);
        await _activityRepo.DidNotReceiveWithAnyArgs().UpsertActivityAsync(default!);
    }

    [Fact]
    public async Task Sync_maps_not_found_to_NotFoundError()
    {
        SetupToken();
        _activityService.GetAthleteActivityAsync(AthleteId, "t", ActivityId)
            .Returns(Result.Fail<ActivityEntity>(new StravaActivitiesError(StravaActivitiesError.NotFound)));

        var res = await _service.SyncAthleteActivityAsync(AthleteId, ActivityId);

        res.Should().BeFailure();
        res.HasError<NotFoundError>().Should().BeTrue();
        await _activityRepo.DidNotReceiveWithAnyArgs().UpsertActivityAsync(default!);
    }

    [Fact]
    public async Task Sync_maps_other_error_to_generic_failure()
    {
        SetupToken();
        _activityService.GetAthleteActivityAsync(AthleteId, "t", ActivityId)
            .Returns(Result.Fail<ActivityEntity>(new StravaActivitiesError(StravaActivitiesError.TooManyRequests)));

        var res = await _service.SyncAthleteActivityAsync(AthleteId, ActivityId);

        res.Should().BeFailure();
        res.HasError<NotFoundError>().Should().BeFalse();
        await _activityRepo.DidNotReceiveWithAnyArgs().UpsertActivityAsync(default!);
    }

    [Fact]
    public async Task Delete_removes_the_activity_then_publishes_its_gear()
    {
        _activityRepo.GetAsync(AthleteId, ActivityId).Returns(new ActivityEntity { Id = ActivityId, AthleteId = AthleteId, GearId = "b9" });

        var res = await _service.DeleteAthleteActivityAsync(AthleteId, ActivityId);

        res.Should().BeSuccess();
        await _activityRepo.Received().DeleteAsync(AthleteId, ActivityId);
        // Publishes with the captured gear so the mileage projection recomputes and drops the ride.
        await _mediator.Received().Publish(
            Arg.Is<ActivitySyncedNotification>(n => n.AthleteId == AthleteId && n.ActivityId == ActivityId && n.GearId == "b9"),
            Arg.Any<CancellationToken>());
    }
}

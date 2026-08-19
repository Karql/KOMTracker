#nullable enable
using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Strava;
using KomTracker.Domain.Entities.Token;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using IStravaActivityService = KomTracker.Application.Interfaces.Services.Strava.IActivityService;
using StravaActivitiesError = KomTracker.Application.Interfaces.Services.Strava.GetAthleteActivitiesError;

namespace KomTracker.Application.Tests.Commands.Strava;

public class SyncActivityCommandHandlerTests
{
    private const int AthleteId = 7;
    private const long ActivityId = 19598505831;

    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteService _athleteService;
    private readonly IStravaActivityService _activityService;
    private readonly IActivityRepository _activityRepo;
    private readonly SyncActivityCommandHandler _handler;

    public SyncActivityCommandHandlerTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _athleteService = Substitute.For<IAthleteService>();
        _activityService = Substitute.For<IStravaActivityService>();
        _activityRepo = Substitute.For<IActivityRepository>();

        _komUoW.GetRepository<IActivityRepository>().Returns(_activityRepo);

        _handler = new SyncActivityCommandHandler(_komUoW, _athleteService, _activityService,
            Substitute.For<ILogger<SyncActivityCommandHandler>>());
    }

    private void SetupToken() =>
        _athleteService.GetValidTokenAsync(AthleteId).Returns(Result.Ok(new TokenEntity { AccessToken = "t" }));

    [Fact]
    public async Task Fetches_and_upserts_single_activity()
    {
        SetupToken();
        var entity = new ActivityEntity { Id = ActivityId, AthleteId = AthleteId };
        _activityService.GetAthleteActivityAsync(AthleteId, "t", ActivityId).Returns(Result.Ok(entity));

        var res = await _handler.Handle(new SyncActivityCommand { AthleteId = AthleteId, ActivityId = ActivityId }, CancellationToken.None);

        res.Should().BeSuccess();
        await _activityRepo.Received().UpsertActivityAsync(entity);
    }

    [Fact]
    public async Task Fails_without_valid_token_and_does_not_upsert()
    {
        _athleteService.GetValidTokenAsync(AthleteId).Returns(Result.Fail<TokenEntity>("no token"));

        var res = await _handler.Handle(new SyncActivityCommand { AthleteId = AthleteId, ActivityId = ActivityId }, CancellationToken.None);

        res.Should().BeFailure();
        await _activityService.DidNotReceiveWithAnyArgs().GetAthleteActivityAsync(default, default!, default);
        await _activityRepo.DidNotReceiveWithAnyArgs().UpsertActivityAsync(default!);
    }

    [Fact]
    public async Task Maps_service_not_found_to_NotFoundError()
    {
        SetupToken();
        _activityService.GetAthleteActivityAsync(AthleteId, "t", ActivityId)
            .Returns(Result.Fail<ActivityEntity>(new StravaActivitiesError(StravaActivitiesError.NotFound)));

        var res = await _handler.Handle(new SyncActivityCommand { AthleteId = AthleteId, ActivityId = ActivityId }, CancellationToken.None);

        res.Should().BeFailure();
        res.HasError<NotFoundError>().Should().BeTrue();
        await _activityRepo.DidNotReceiveWithAnyArgs().UpsertActivityAsync(default!);
    }

    [Fact]
    public async Task Maps_other_service_error_to_generic_failure()
    {
        SetupToken();
        _activityService.GetAthleteActivityAsync(AthleteId, "t", ActivityId)
            .Returns(Result.Fail<ActivityEntity>(new StravaActivitiesError(StravaActivitiesError.TooManyRequests)));

        var res = await _handler.Handle(new SyncActivityCommand { AthleteId = AthleteId, ActivityId = ActivityId }, CancellationToken.None);

        res.Should().BeFailure();
        res.HasError<NotFoundError>().Should().BeFalse();
        await _activityRepo.DidNotReceiveWithAnyArgs().UpsertActivityAsync(default!);
    }
}

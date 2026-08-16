#nullable enable
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Strava;
using KomTracker.Domain.Entities.Token;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using IStravaGearService = KomTracker.Application.Interfaces.Services.Strava.IGearService;
using StravaGearError = KomTracker.Application.Interfaces.Services.Strava.GetAthleteBikesError;

namespace KomTracker.Application.Tests.Commands.Strava;

public class SyncStravaBikesCommandHandlerTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteService _athleteService;
    private readonly IStravaGearService _gearService;
    private readonly IStravaBikeRepository _stravaBikeRepo;
    private readonly IAthleteSyncRepository _athleteSyncRepo;
    private readonly IActivityRepository _activityRepo;
    private readonly SyncStravaBikesCommandHandler _handler;

    public SyncStravaBikesCommandHandlerTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _athleteService = Substitute.For<IAthleteService>();
        _gearService = Substitute.For<IStravaGearService>();
        _stravaBikeRepo = Substitute.For<IStravaBikeRepository>();
        _athleteSyncRepo = Substitute.For<IAthleteSyncRepository>();
        _activityRepo = Substitute.For<IActivityRepository>();

        _komUoW.GetRepository<IStravaBikeRepository>().Returns(_stravaBikeRepo);
        _komUoW.GetRepository<IAthleteSyncRepository>().Returns(_athleteSyncRepo);
        _komUoW.GetRepository<IActivityRepository>().Returns(_activityRepo);

        _activityRepo.GetDistinctBikeGearIdsAsync(Arg.Any<int>()).Returns(Array.Empty<string>().AsEnumerable());

        _handler = new SyncStravaBikesCommandHandler(_komUoW, _athleteService, _gearService);
    }

    [Fact]
    public async Task Upserts_bikes_and_sets_bikes_enabled()
    {
        _athleteService.GetValidTokenAsync(1).Returns(Result.Ok(new TokenEntity { AccessToken = "t" }));
        var bikes = new List<StravaBikeEntity> { new() { Id = "b1", AthleteId = 1, Retired = true } };
        _gearService.GetAthleteBikesAsync(1, "t", Arg.Any<IReadOnlyCollection<string>>()).Returns(Result.Ok(bikes.AsEnumerable()));

        var res = await _handler.Handle(new SyncStravaBikesCommand { AthleteId = 1 }, CancellationToken.None);

        res.Should().BeSuccess();
        await _stravaBikeRepo.Received().UpsertAthleteBikesAsync(1,
            Arg.Is<IReadOnlyCollection<StravaBikeEntity>>(x => x.Count == 1 && x.First().Id == "b1"));
        await _athleteSyncRepo.Received().SetBikesEnabledAsync(1, true);
        // Never toggles activity sync.
        await _athleteSyncRepo.DidNotReceive().UpsertAsync(Arg.Any<AthleteSyncEntity>());
    }

    [Fact]
    public async Task Passes_activity_gear_ids_to_gear_service()
    {
        _athleteService.GetValidTokenAsync(1).Returns(Result.Ok(new TokenEntity { AccessToken = "t" }));
        _activityRepo.GetDistinctBikeGearIdsAsync(1).Returns(new[] { "b9" }.AsEnumerable());
        _gearService.GetAthleteBikesAsync(1, "t", Arg.Any<IReadOnlyCollection<string>>())
            .Returns(Result.Ok(Enumerable.Empty<StravaBikeEntity>()));

        var res = await _handler.Handle(new SyncStravaBikesCommand { AthleteId = 1 }, CancellationToken.None);

        res.Should().BeSuccess();
        await _gearService.Received().GetAthleteBikesAsync(1, "t",
            Arg.Is<IReadOnlyCollection<string>>(x => x.Contains("b9")));
    }

    [Fact]
    public async Task Fails_without_valid_token()
    {
        _athleteService.GetValidTokenAsync(1).Returns(Result.Fail<TokenEntity>("no token"));

        var res = await _handler.Handle(new SyncStravaBikesCommand { AthleteId = 1 }, CancellationToken.None);

        res.Should().BeFailure();
        await _stravaBikeRepo.DidNotReceiveWithAnyArgs().UpsertAthleteBikesAsync(default, default!);
    }

    [Fact]
    public async Task Surfaces_gear_service_failure()
    {
        _athleteService.GetValidTokenAsync(1).Returns(Result.Ok(new TokenEntity { AccessToken = "t" }));
        _gearService.GetAthleteBikesAsync(1, "t", Arg.Any<IReadOnlyCollection<string>>())
            .Returns(Result.Fail<IEnumerable<StravaBikeEntity>>(new StravaGearError(StravaGearError.TooManyRequests)));

        var res = await _handler.Handle(new SyncStravaBikesCommand { AthleteId = 1 }, CancellationToken.None);

        res.Should().BeFailure();
        await _stravaBikeRepo.DidNotReceiveWithAnyArgs().UpsertAthleteBikesAsync(default, default!);
        await _athleteSyncRepo.DidNotReceive().SetBikesEnabledAsync(Arg.Any<int>(), Arg.Any<bool>());
    }
}

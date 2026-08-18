#nullable enable
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using StravaGearError = KomTracker.Application.Interfaces.Services.Strava.GetAthleteBikesError;

namespace KomTracker.Application.Tests.Commands.Strava;

public class SyncBikesCommandHandlerTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IMediator _mediator;
    private readonly IAthleteSyncRepository _athleteSyncRepo;
    private readonly SyncBikesCommandHandler _handler;

    public SyncBikesCommandHandlerTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _mediator = Substitute.For<IMediator>();
        _athleteSyncRepo = Substitute.For<IAthleteSyncRepository>();
        _komUoW.GetRepository<IAthleteSyncRepository>().Returns(_athleteSyncRepo);

        _mediator.Send(Arg.Any<SyncStravaBikesCommand>(), Arg.Any<CancellationToken>()).Returns(Result.Ok());

        _handler = new SyncBikesCommandHandler(_komUoW, _mediator,
            Substitute.For<ILogger<SyncBikesCommandHandler>>());
    }

    [Fact]
    public async Task Processes_all_bikes_enabled_athletes()
    {
        _athleteSyncRepo.GetBikesEnabledAthleteIdsAsync().Returns(new[] { 1, 2 }.AsEnumerable());

        var res = await _handler.Handle(new SyncBikesCommand(), CancellationToken.None);

        res.Should().BeSuccess();
        await _mediator.Received().Send(Arg.Is<SyncStravaBikesCommand>(c => c.AthleteId == 1), Arg.Any<CancellationToken>());
        await _mediator.Received().Send(Arg.Is<SyncStravaBikesCommand>(c => c.AthleteId == 2), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Stops_whole_run_on_rate_limit()
    {
        _athleteSyncRepo.GetBikesEnabledAthleteIdsAsync().Returns(new[] { 1, 2 }.AsEnumerable());
        _mediator.Send(Arg.Is<SyncStravaBikesCommand>(c => c.AthleteId == 1), Arg.Any<CancellationToken>())
            .Returns(Result.Fail(new StravaGearError(StravaGearError.TooManyRequests)));

        var res = await _handler.Handle(new SyncBikesCommand(), CancellationToken.None);

        res.Should().BeFailure();
        await _mediator.DidNotReceive().Send(Arg.Is<SyncStravaBikesCommand>(c => c.AthleteId == 2), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Single_athlete_bypasses_enabled_list()
    {
        var res = await _handler.Handle(new SyncBikesCommand { AthleteId = 5 }, CancellationToken.None);

        res.Should().BeSuccess();
        await _athleteSyncRepo.DidNotReceive().GetBikesEnabledAthleteIdsAsync();
        await _mediator.Received().Send(Arg.Is<SyncStravaBikesCommand>(c => c.AthleteId == 5), Arg.Any<CancellationToken>());
    }
}

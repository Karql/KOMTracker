#nullable enable
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Strava;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Strava;

public class SetAthleteSyncCommandHandlerTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IMediator _mediator;
    private readonly IAthleteSyncRepository _athleteSyncRepo;
    private readonly SetAthleteSyncCommandHandler _handler;

    public SetAthleteSyncCommandHandlerTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _mediator = Substitute.For<IMediator>();
        _athleteSyncRepo = Substitute.For<IAthleteSyncRepository>();

        _komUoW.GetRepository<IAthleteSyncRepository>().Returns(_athleteSyncRepo);
        _mediator.Send(Arg.Any<SyncActivitiesCommand>(), Arg.Any<CancellationToken>()).Returns(Result.Ok());

        _handler = new SetAthleteSyncCommandHandler(_komUoW, _mediator,
            Substitute.For<ILogger<SetAthleteSyncCommandHandler>>());
    }

    [Fact]
    public async Task Fresh_enable_upserts_and_backfills_that_athlete()
    {
        _athleteSyncRepo.GetAsync(1).Returns((AthleteSyncEntity?)null);

        var res = await _handler.Handle(new SetAthleteSyncCommand { AthleteId = 1, Enabled = true }, CancellationToken.None);

        res.Should().BeSuccess();
        await _athleteSyncRepo.Received().UpsertAsync(Arg.Is<AthleteSyncEntity>(x => x.AthleteId == 1 && x.ActivitiesEnabled));
        await _mediator.Received().Send(Arg.Is<SyncActivitiesCommand>(c => c.AthleteId == 1 && c.After == null), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Already_enabled_does_not_rebackfill()
    {
        _athleteSyncRepo.GetAsync(1).Returns(new AthleteSyncEntity { AthleteId = 1, ActivitiesEnabled = true });

        await _handler.Handle(new SetAthleteSyncCommand { AthleteId = 1, Enabled = true }, CancellationToken.None);

        await _mediator.DidNotReceive().Send(Arg.Any<SyncActivitiesCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Disable_does_not_backfill()
    {
        _athleteSyncRepo.GetAsync(1).Returns(new AthleteSyncEntity { AthleteId = 1, ActivitiesEnabled = true });

        await _handler.Handle(new SetAthleteSyncCommand { AthleteId = 1, Enabled = false }, CancellationToken.None);

        await _athleteSyncRepo.Received().UpsertAsync(Arg.Is<AthleteSyncEntity>(x => x.AthleteId == 1 && !x.ActivitiesEnabled));
        await _mediator.DidNotReceive().Send(Arg.Any<SyncActivitiesCommand>(), Arg.Any<CancellationToken>());
    }
}

#nullable enable
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Bike;
using KomTracker.Domain.Entities.Strava;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Strava;

public class LinkStravaBikeCommandHandlerTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IBikeRepository _bikeRepo;
    private readonly IStravaBikeRepository _stravaBikeRepo;
    private readonly IBikeLinkRepository _bikeLinkRepo;
    private readonly LinkStravaBikeCommandHandler _handler;

    public LinkStravaBikeCommandHandlerTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _bikeRepo = Substitute.For<IBikeRepository>();
        _stravaBikeRepo = Substitute.For<IStravaBikeRepository>();
        _bikeLinkRepo = Substitute.For<IBikeLinkRepository>();

        _komUoW.GetRepository<IBikeRepository>().Returns(_bikeRepo);
        _komUoW.GetRepository<IStravaBikeRepository>().Returns(_stravaBikeRepo);
        _komUoW.GetRepository<IBikeLinkRepository>().Returns(_bikeLinkRepo);

        _handler = new LinkStravaBikeCommandHandler(_komUoW);
    }

    private LinkStravaBikeCommand Command() => new()
    {
        BikeId = 10,
        StravaGearId = "b1",
        UserId = "u1",
        AthleteId = 1
    };

    [Fact]
    public async Task Links_bike_to_strava_gear()
    {
        _bikeRepo.GetBikeAsync(10).Returns(new BikeEntity { Id = 10, UserId = "u1" });
        _stravaBikeRepo.GetAsync(1, "b1").Returns(new StravaBikeEntity { Id = "b1", AthleteId = 1 });
        _bikeLinkRepo.ExistsAsync(ExternalService.Strava, "b1").Returns(false);

        var res = await _handler.Handle(Command(), CancellationToken.None);

        res.Should().BeSuccess();
        _bikeLinkRepo.Received().Add(Arg.Is<BikeLinkEntity>(x =>
            x.BikeId == 10 && x.ExternalService == ExternalService.Strava && x.ExternalId == "b1"));
        await _komUoW.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task Rejects_foreign_bike()
    {
        _bikeRepo.GetBikeAsync(10).Returns(new BikeEntity { Id = 10, UserId = "someone-else" });

        var res = await _handler.Handle(Command(), CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ForbiddenError>();
        _bikeLinkRepo.DidNotReceive().Add(Arg.Any<BikeLinkEntity>());
    }

    [Fact]
    public async Task Rejects_gear_not_owned_by_athlete()
    {
        _bikeRepo.GetBikeAsync(10).Returns(new BikeEntity { Id = 10, UserId = "u1" });
        _stravaBikeRepo.GetAsync(1, "b1").Returns((StravaBikeEntity?)null);

        var res = await _handler.Handle(Command(), CancellationToken.None);

        res.Should().BeFailure().Which.HasError<NotFoundError>();
        _bikeLinkRepo.DidNotReceive().Add(Arg.Any<BikeLinkEntity>());
    }

    [Fact]
    public async Task Rejects_already_linked_gear()
    {
        _bikeRepo.GetBikeAsync(10).Returns(new BikeEntity { Id = 10, UserId = "u1" });
        _stravaBikeRepo.GetAsync(1, "b1").Returns(new StravaBikeEntity { Id = "b1", AthleteId = 1 });
        _bikeLinkRepo.ExistsAsync(ExternalService.Strava, "b1").Returns(true);

        var res = await _handler.Handle(Command(), CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
        _bikeLinkRepo.DidNotReceive().Add(Arg.Any<BikeLinkEntity>());
    }
}

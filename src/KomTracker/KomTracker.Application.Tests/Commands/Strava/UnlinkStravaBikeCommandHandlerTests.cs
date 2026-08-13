#nullable enable
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Bike;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Strava;

public class UnlinkStravaBikeCommandHandlerTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IBikeRepository _bikeRepo;
    private readonly IBikeLinkRepository _bikeLinkRepo;
    private readonly UnlinkStravaBikeCommandHandler _handler;

    public UnlinkStravaBikeCommandHandlerTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _bikeRepo = Substitute.For<IBikeRepository>();
        _bikeLinkRepo = Substitute.For<IBikeLinkRepository>();

        _komUoW.GetRepository<IBikeRepository>().Returns(_bikeRepo);
        _komUoW.GetRepository<IBikeLinkRepository>().Returns(_bikeLinkRepo);

        _handler = new UnlinkStravaBikeCommandHandler(_komUoW);
    }

    private static UnlinkStravaBikeCommand Command() => new() { StravaGearId = "b1", UserId = "u1" };

    [Fact]
    public async Task Removes_link_of_owned_bike()
    {
        var link = new BikeLinkEntity { Id = 3, BikeId = 10, ExternalService = ExternalService.Strava, ExternalId = "b1" };
        _bikeLinkRepo.GetByExternalIdAsync(ExternalService.Strava, "b1").Returns(link);
        _bikeRepo.GetBikeAsync(10).Returns(new BikeEntity { Id = 10, UserId = "u1" });

        var res = await _handler.Handle(Command(), CancellationToken.None);

        res.Should().BeSuccess();
        _bikeLinkRepo.Received().Remove(link);
        await _komUoW.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task Not_found_when_no_link()
    {
        _bikeLinkRepo.GetByExternalIdAsync(ExternalService.Strava, "b1").Returns((BikeLinkEntity?)null);

        var res = await _handler.Handle(Command(), CancellationToken.None);

        res.Should().BeFailure().Which.HasError<NotFoundError>();
        _bikeLinkRepo.DidNotReceive().Remove(Arg.Any<BikeLinkEntity>());
    }

    [Fact]
    public async Task Forbidden_when_link_bike_belongs_to_another_user()
    {
        var link = new BikeLinkEntity { Id = 3, BikeId = 10, ExternalService = ExternalService.Strava, ExternalId = "b1" };
        _bikeLinkRepo.GetByExternalIdAsync(ExternalService.Strava, "b1").Returns(link);
        _bikeRepo.GetBikeAsync(10).Returns(new BikeEntity { Id = 10, UserId = "someone-else" });

        var res = await _handler.Handle(Command(), CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ForbiddenError>();
        _bikeLinkRepo.DidNotReceive().Remove(Arg.Any<BikeLinkEntity>());
    }
}

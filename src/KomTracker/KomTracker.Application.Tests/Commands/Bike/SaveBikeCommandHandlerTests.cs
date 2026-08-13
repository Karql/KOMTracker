#nullable enable
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Bike;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Bike;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Bike;

public class SaveBikeCommandHandlerTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IBikeRepository _bikeRepo;
    private readonly IBikeLinkRepository _bikeLinkRepo;
    private readonly SaveBikeCommandHandler _handler;

    public SaveBikeCommandHandlerTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _bikeRepo = Substitute.For<IBikeRepository>();
        _bikeLinkRepo = Substitute.For<IBikeLinkRepository>();

        _komUoW.GetRepository<IBikeRepository>().Returns(_bikeRepo);
        _komUoW.GetRepository<IBikeLinkRepository>().Returns(_bikeLinkRepo);

        _handler = new SaveBikeCommandHandler(_komUoW);
    }

    private static SaveBikeCommand CreateCommand(string? gearId) => new()
    {
        Id = null,
        UserId = "u1",
        Name = "My bike",
        Type = BikeType.Gravel,
        StravaGearId = gearId
    };

    [Fact]
    public async Task Create_with_strava_gear_id_also_creates_link()
    {
        _bikeLinkRepo.ExistsAsync(ExternalService.Strava, "b1").Returns(false);

        var res = await _handler.Handle(CreateCommand("b1"), CancellationToken.None);

        res.Should().BeSuccess();
        _bikeRepo.Received().AddBike(Arg.Any<BikeEntity>());
        _bikeLinkRepo.Received().Add(Arg.Is<BikeLinkEntity>(x =>
            x.ExternalService == ExternalService.Strava && x.ExternalId == "b1"));
    }

    [Fact]
    public async Task Create_with_already_linked_gear_is_rejected()
    {
        _bikeLinkRepo.ExistsAsync(ExternalService.Strava, "b1").Returns(true);

        var res = await _handler.Handle(CreateCommand("b1"), CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
        _bikeRepo.DidNotReceive().AddBike(Arg.Any<BikeEntity>());
        _bikeLinkRepo.DidNotReceive().Add(Arg.Any<BikeLinkEntity>());
    }

    [Fact]
    public async Task Create_without_gear_id_creates_no_link()
    {
        var res = await _handler.Handle(CreateCommand(null), CancellationToken.None);

        res.Should().BeSuccess();
        _bikeRepo.Received().AddBike(Arg.Any<BikeEntity>());
        _bikeLinkRepo.DidNotReceive().Add(Arg.Any<BikeLinkEntity>());
    }

    [Fact]
    public async Task Update_ignores_strava_gear_id()
    {
        _bikeRepo.GetBikeAsync(5).Returns(new BikeEntity { Id = 5, UserId = "u1", Name = "old" });

        var cmd = CreateCommand("b1");
        cmd.Id = 5;

        var res = await _handler.Handle(cmd, CancellationToken.None);

        res.Should().BeSuccess();
        _bikeLinkRepo.DidNotReceive().Add(Arg.Any<BikeLinkEntity>());
    }
}

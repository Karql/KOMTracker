#nullable enable
using FluentAssertions;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Queries.Strava;
using KomTracker.Domain.Entities.Bike;
using KomTracker.Domain.Entities.Strava;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Queries.Strava;

public class GetStravaActivitiesQueryHandlerTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IActivityRepository _activityRepo;
    private readonly IBikeLinkRepository _bikeLinkRepo;
    private readonly IBikeRepository _bikeRepo;
    private readonly IStravaBikeRepository _stravaBikeRepo;
    private readonly GetStravaActivitiesQueryHandler _handler;

    public GetStravaActivitiesQueryHandlerTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _activityRepo = Substitute.For<IActivityRepository>();
        _bikeLinkRepo = Substitute.For<IBikeLinkRepository>();
        _bikeRepo = Substitute.For<IBikeRepository>();
        _stravaBikeRepo = Substitute.For<IStravaBikeRepository>();

        _komUoW.GetRepository<IActivityRepository>().Returns(_activityRepo);
        _komUoW.GetRepository<IBikeLinkRepository>().Returns(_bikeLinkRepo);
        _komUoW.GetRepository<IBikeRepository>().Returns(_bikeRepo);
        _komUoW.GetRepository<IStravaBikeRepository>().Returns(_stravaBikeRepo);

        _stravaBikeRepo.GetByAthleteAsync(Arg.Any<int>()).Returns(Enumerable.Empty<StravaBikeEntity>());

        _handler = new GetStravaActivitiesQueryHandler(_komUoW);
    }

    [Fact]
    public async Task Returns_page_with_resolved_bike_links()
    {
        _activityRepo.CountAthleteActivitiesAsync(1).Returns(2);
        _activityRepo.GetActivitiesPageAsync(1, 0, 20).Returns(new List<ActivityEntity>
        {
            new() { Id = 10, AthleteId = 1, GearId = "b1", Distance = 10000, MovingTime = 3600, AverageSpeed = 5 },
            new() { Id = 11, AthleteId = 1, GearId = null }
        }.AsEnumerable());

        _bikeLinkRepo.GetByExternalIdsAsync(ExternalService.Strava, Arg.Any<IReadOnlyCollection<string>>())
            .Returns(new List<BikeLinkEntity> { new() { BikeId = 7, ExternalService = ExternalService.Strava, ExternalId = "b1" } }.AsEnumerable());
        _bikeRepo.GetBikesAsync("u1", true)
            .Returns(new List<BikeEntity> { new() { Id = 7, Name = "Road" } }.AsEnumerable());
        _stravaBikeRepo.GetByAthleteAsync(1)
            .Returns(new List<StravaBikeEntity> { new() { Id = "b1", AthleteId = 1, Name = "White Lady" } }.AsEnumerable());

        var res = await _handler.Handle(new GetStravaActivitiesQuery { AthleteId = 1, UserId = "u1", Page = 0, PageSize = 20 }, CancellationToken.None);

        res.TotalCount.Should().Be(2);
        res.Items.Should().HaveCount(2);

        var linked = res.Items.Single(x => x.Id == 10);
        linked.LinkedBikeId.Should().Be(7);
        linked.LinkedBikeName.Should().Be("Road");
        linked.StravaBikeName.Should().Be("White Lady");
        linked.DistanceMeters.Should().Be(10000);

        var unlinked = res.Items.Single(x => x.Id == 11);
        unlinked.LinkedBikeId.Should().BeNull();
    }

    [Fact]
    public async Task Gear_without_link_stays_unattributed()
    {
        _activityRepo.CountAthleteActivitiesAsync(1).Returns(1);
        _activityRepo.GetActivitiesPageAsync(1, 0, 20).Returns(new List<ActivityEntity>
        {
            new() { Id = 20, AthleteId = 1, GearId = "b-unknown" }
        }.AsEnumerable());
        _bikeLinkRepo.GetByExternalIdsAsync(ExternalService.Strava, Arg.Any<IReadOnlyCollection<string>>())
            .Returns(Enumerable.Empty<BikeLinkEntity>());
        _bikeRepo.GetBikesAsync("u1", true).Returns(Enumerable.Empty<BikeEntity>());

        var res = await _handler.Handle(new GetStravaActivitiesQuery { AthleteId = 1, UserId = "u1" }, CancellationToken.None);

        res.Items.Single().LinkedBikeId.Should().BeNull();
    }
}

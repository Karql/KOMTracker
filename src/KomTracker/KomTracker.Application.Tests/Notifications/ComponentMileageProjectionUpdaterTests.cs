#nullable enable
using KomTracker.Application.Commands.Component;
using KomTracker.Application.Commands.ServiceRecord;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Interfaces.Services.Identity;
using KomTracker.Application.Models.Identity;
using KomTracker.Application.Notifications.Component;
using KomTracker.Application.Notifications.Strava;
using KomTracker.Domain.Entities.Bike;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Notifications;

public class ComponentMileageProjectionUpdaterTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IInstallationRepository _installationRepo;
    private readonly IBikeRepository _bikeRepo;
    private readonly IBikeLinkRepository _bikeLinkRepo;
    private readonly IUserService _userService;
    private readonly IMediator _mediator;
    private readonly ComponentMileageProjectionUpdater _updater;

    public ComponentMileageProjectionUpdaterTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _installationRepo = Substitute.For<IInstallationRepository>();
        _bikeRepo = Substitute.For<IBikeRepository>();
        _bikeLinkRepo = Substitute.For<IBikeLinkRepository>();
        _userService = Substitute.For<IUserService>();
        _mediator = Substitute.For<IMediator>();

        _komUoW.GetRepository<IInstallationRepository>().Returns(_installationRepo);
        _komUoW.GetRepository<IBikeRepository>().Returns(_bikeRepo);
        _komUoW.GetRepository<IBikeLinkRepository>().Returns(_bikeLinkRepo);

        _updater = new ComponentMileageProjectionUpdater(_komUoW, _userService, _mediator,
            Substitute.For<ILogger<ComponentMileageProjectionUpdater>>());
    }

    private Task Recompute(int componentId) => _mediator.Received().Send(
        Arg.Is<RecalculateComponentsMileageCommand>(c => c.ComponentIds.Contains(componentId)), Arg.Any<CancellationToken>());

    [Fact]
    public async Task Component_change_recomputes_the_component()
    {
        await _updater.Handle(new ComponentChangedNotification { ComponentId = 5 }, CancellationToken.None);

        await Recompute(5);
    }

    [Fact]
    public async Task Installation_change_recomputes_the_component()
    {
        await _updater.Handle(new InstallationChangedNotification { ComponentId = 5 }, CancellationToken.None);

        await Recompute(5);
    }

    [Fact]
    public async Task Athlete_sync_resolves_user_bikes_and_recomputes_their_components()
    {
        _userService.GetUserAsync(7).Returns(new UserModel { Id = "u1" });
        _bikeRepo.GetBikesAsync("u1", true).Returns(new[] { new BikeEntity { Id = 3 } });
        _installationRepo.GetComponentIdsByBikesAsync(Arg.Is<IReadOnlyCollection<int>>(c => c.Contains(3))).Returns(new[] { 5 });

        await _updater.Handle(new AthleteActivitiesSyncedNotification { AthleteId = 7 }, CancellationToken.None);

        await Recompute(5);
    }

    [Fact]
    public async Task Athlete_sync_with_no_owning_user_is_a_no_op()
    {
        _userService.GetUserAsync(7).Returns((UserModel?)null);

        await _updater.Handle(new AthleteActivitiesSyncedNotification { AthleteId = 7 }, CancellationToken.None);

        await _mediator.DidNotReceive().Send(Arg.Any<RecalculateComponentsMileageCommand>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activity_sync_resolves_gear_bike_and_recomputes_its_components()
    {
        _bikeLinkRepo.GetByExternalIdAsync(ExternalService.Strava, "b1").Returns(new BikeLinkEntity { BikeId = 3 });
        _installationRepo.GetComponentIdsByBikesAsync(Arg.Is<IReadOnlyCollection<int>>(c => c.Contains(3))).Returns(new[] { 5 });

        await _updater.Handle(new ActivitySyncedNotification { AthleteId = 7, ActivityId = 99, GearId = "b1" }, CancellationToken.None);

        await Recompute(5);
    }

    [Fact]
    public async Task Activity_moved_to_another_bike_recomputes_both_bikes_components_in_one_go()
    {
        _bikeLinkRepo.GetByExternalIdAsync(ExternalService.Strava, "b-new").Returns(new BikeLinkEntity { BikeId = 3 });
        _bikeLinkRepo.GetByExternalIdAsync(ExternalService.Strava, "b-old").Returns(new BikeLinkEntity { BikeId = 4 });
        _installationRepo.GetComponentIdsByBikesAsync(Arg.Is<IReadOnlyCollection<int>>(c => c.Contains(3) && c.Contains(4)))
            .Returns(new[] { 5, 6 });

        await _updater.Handle(new ActivitySyncedNotification { AthleteId = 7, ActivityId = 99, GearId = "b-new", PreviousGearId = "b-old" },
            CancellationToken.None);

        await _mediator.Received(1).Send(
            Arg.Is<RecalculateComponentsMileageCommand>(c => c.ComponentIds.Contains(5) && c.ComponentIds.Contains(6)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Activity_sync_with_unchanged_gear_resolves_the_bike_once()
    {
        _bikeLinkRepo.GetByExternalIdAsync(ExternalService.Strava, "b1").Returns(new BikeLinkEntity { BikeId = 3 });
        _installationRepo.GetComponentIdsByBikesAsync(Arg.Any<IReadOnlyCollection<int>>()).Returns(new[] { 5 });

        await _updater.Handle(new ActivitySyncedNotification { AthleteId = 7, ActivityId = 99, GearId = "b1", PreviousGearId = "b1" },
            CancellationToken.None);

        await _bikeLinkRepo.Received(1).GetByExternalIdAsync(ExternalService.Strava, "b1");
        await Recompute(5);
    }

    [Fact]
    public async Task Activity_moved_off_a_bike_to_no_gear_still_recomputes_the_old_bike()
    {
        _bikeLinkRepo.GetByExternalIdAsync(ExternalService.Strava, "b-old").Returns(new BikeLinkEntity { BikeId = 4 });
        _installationRepo.GetComponentIdsByBikesAsync(Arg.Is<IReadOnlyCollection<int>>(c => c.Contains(4))).Returns(new[] { 6 });

        await _updater.Handle(new ActivitySyncedNotification { AthleteId = 7, ActivityId = 99, GearId = null, PreviousGearId = "b-old" },
            CancellationToken.None);

        await Recompute(6);
    }

    private Task RecomputeServices(System.Func<RecalculateServiceMileageCommand, bool> match) => _mediator.Received().Send(
        Arg.Is<RecalculateServiceMileageCommand>(c => match(c)), Arg.Any<CancellationToken>());

    [Fact]
    public async Task Component_change_also_recomputes_the_services_covering_it()
    {
        await _updater.Handle(new ComponentChangedNotification { ComponentId = 5 }, CancellationToken.None);

        await RecomputeServices(c => c.ComponentIds.Contains(5));
    }

    [Fact]
    public async Task Activity_sync_recomputes_services_of_the_bike_and_its_components()
    {
        _bikeLinkRepo.GetByExternalIdAsync(ExternalService.Strava, "b1").Returns(new BikeLinkEntity { BikeId = 3 });
        _installationRepo.GetComponentIdsByBikesAsync(Arg.Is<IReadOnlyCollection<int>>(c => c.Contains(3))).Returns(new[] { 5 });

        await _updater.Handle(new ActivitySyncedNotification { AthleteId = 7, ActivityId = 99, GearId = "b1" }, CancellationToken.None);

        await RecomputeServices(c => c.BikeIds.Contains(3) && c.ComponentIds.Contains(5));
    }

    [Fact]
    public async Task Bike_change_recomputes_its_components_and_services()
    {
        _installationRepo.GetComponentIdsByBikesAsync(Arg.Is<IReadOnlyCollection<int>>(c => c.Contains(3))).Returns(new[] { 5 });

        await _updater.Handle(new KomTracker.Application.Notifications.Bike.BikeChangedNotification { BikeId = 3 }, CancellationToken.None);

        await Recompute(5);
        await RecomputeServices(c => c.BikeIds.Contains(3) && c.ComponentIds.Contains(5));
    }

    [Fact]
    public async Task Bike_change_without_components_still_recomputes_its_services()
    {
        _installationRepo.GetComponentIdsByBikesAsync(Arg.Any<IReadOnlyCollection<int>>()).Returns(System.Array.Empty<int>());

        await _updater.Handle(new KomTracker.Application.Notifications.Bike.BikeChangedNotification { BikeId = 3 }, CancellationToken.None);

        await _mediator.DidNotReceive().Send(Arg.Any<RecalculateComponentsMileageCommand>(), Arg.Any<CancellationToken>());
        await RecomputeServices(c => c.BikeIds.Contains(3));
    }

    [Fact]
    public async Task Service_change_recomputes_that_service()
    {
        await _updater.Handle(new KomTracker.Application.Notifications.ServiceRecord.ServiceRecordChangedNotification { ServiceId = 4 }, CancellationToken.None);

        await RecomputeServices(c => c.ServiceIds.Contains(4));
    }

    [Fact]
    public async Task Activity_sync_without_gear_is_a_no_op()
    {
        await _updater.Handle(new ActivitySyncedNotification { AthleteId = 7, ActivityId = 99, GearId = null }, CancellationToken.None);

        await _mediator.DidNotReceive().Send(Arg.Any<RecalculateComponentsMileageCommand>(), Arg.Any<CancellationToken>());
    }
}

#nullable enable
using KomTracker.Application.Commands.Component;
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
    public async Task Activity_sync_without_gear_is_a_no_op()
    {
        await _updater.Handle(new ActivitySyncedNotification { AthleteId = 7, ActivityId = 99, GearId = null }, CancellationToken.None);

        await _mediator.DidNotReceive().Send(Arg.Any<RecalculateComponentsMileageCommand>(), Arg.Any<CancellationToken>());
    }
}

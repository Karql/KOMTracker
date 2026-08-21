#nullable enable
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Installation;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Bike;
using KomTracker.Domain.Entities.Component;
using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Installation;

public class InstallationCommandHandlersTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IInstallationRepository _installationRepo;
    private readonly IComponentRepository _componentRepo;
    private readonly IBikeRepository _bikeRepo;

    public InstallationCommandHandlersTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _installationRepo = Substitute.For<IInstallationRepository>();
        _componentRepo = Substitute.For<IComponentRepository>();
        _bikeRepo = Substitute.For<IBikeRepository>();
        _komUoW.GetRepository<IInstallationRepository>().Returns(_installationRepo);
        _komUoW.GetRepository<IComponentRepository>().Returns(_componentRepo);
        _komUoW.GetRepository<IBikeRepository>().Returns(_bikeRepo);
    }

    private void OwnComponent(int id, string userId = "u1", int? warehouseId = null)
        => _componentRepo.GetComponentAsync(id).Returns(new ComponentEntity
        {
            Id = id, UserId = userId, Name = "Chain", Category = ComponentCategory.Chain, WarehouseId = warehouseId
        });

    private void OwnBike(int id, string userId = "u1")
        => _bikeRepo.GetBikeAsync(id).Returns(new BikeEntity { Id = id, UserId = userId, Name = "Road" });

    [Fact]
    public async Task Install_tracked_adds_row_and_clears_warehouse()
    {
        OwnComponent(5, warehouseId: 9);
        OwnBike(3);
        _installationRepo.GetActiveTrackedByComponentAsync(5).Returns((InstallationEntity?)null);

        var handler = new InstallComponentCommandHandler(_komUoW);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, BikeId = 3,
            Type = ComponentInstallationType.Tracked, DateFrom = new DateTime(2026, 1, 1), Position = InstallationPosition.Rear
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Add(Arg.Is<InstallationEntity>(i =>
            i.ComponentId == 5 && i.BikeId == 3 && i.Type == ComponentInstallationType.Tracked && i.DateTo == null));
        _componentRepo.Received().UpdateComponent(Arg.Is<ComponentEntity>(c => c.WarehouseId == null));
    }

    [Fact]
    public async Task Install_tracked_when_already_installed_conflicts()
    {
        OwnComponent(5);
        OwnBike(3);
        _installationRepo.GetActiveTrackedByComponentAsync(5)
            .Returns(new InstallationEntity { Id = 1, UserId = "u1", ComponentId = 5, BikeId = 2, Type = ComponentInstallationType.Tracked });

        var handler = new InstallComponentCommandHandler(_komUoW);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Tracked, DateFrom = new DateTime(2026, 1, 1)
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
        _installationRepo.DidNotReceive().Add(Arg.Any<InstallationEntity>());
    }

    [Fact]
    public async Task Install_manual_stores_static_totals_without_dates()
    {
        OwnComponent(5);
        OwnBike(3);

        var handler = new InstallComponentCommandHandler(_komUoW);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Manual,
            ManualDistanceKm = 1200, ManualMovingHours = 40, ManualElevationM = 8000
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Add(Arg.Is<InstallationEntity>(i =>
            i.Type == ComponentInstallationType.Manual && i.DateFrom == null && i.DateTo == null && i.ManualDistanceKm == 1200));
        // Manual install never touches the active-Tracked guard.
        await _installationRepo.DidNotReceive().GetActiveTrackedByComponentAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Install_other_users_component_is_forbidden()
    {
        OwnComponent(5, userId: "other");

        var handler = new InstallComponentCommandHandler(_komUoW);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Manual
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ForbiddenError>();
    }

    [Fact]
    public async Task Move_closes_current_and_opens_new_window()
    {
        var current = new InstallationEntity
        {
            Id = 7, UserId = "u1", ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Tracked, DateTo = null
        };
        _installationRepo.GetAsync(7).Returns(current);
        OwnBike(4);

        var handler = new MoveInstallationCommandHandler(_komUoW);
        var moveDate = new DateTime(2026, 6, 1);
        var res = await handler.Handle(new MoveInstallationCommand
        {
            UserId = "u1", InstallationId = 7, NewBikeId = 4, NewPosition = InstallationPosition.Front, MoveDate = moveDate
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Update(Arg.Is<InstallationEntity>(i => i.Id == 7 && i.DateTo != null));
        _installationRepo.Received().Add(Arg.Is<InstallationEntity>(i =>
            i.ComponentId == 5 && i.BikeId == 4 && i.DateFrom == DateTime.SpecifyKind(moveDate, DateTimeKind.Utc) && i.DateTo == null));
    }

    [Fact]
    public async Task Move_non_current_installation_conflicts()
    {
        _installationRepo.GetAsync(7).Returns(new InstallationEntity
        {
            Id = 7, UserId = "u1", ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Tracked, DateTo = new DateTime(2026, 1, 1)
        });

        var handler = new MoveInstallationCommandHandler(_komUoW);
        var res = await handler.Handle(new MoveInstallationCommand
        {
            UserId = "u1", InstallationId = 7, NewBikeId = 4, MoveDate = new DateTime(2026, 6, 1)
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
        _installationRepo.DidNotReceive().Add(Arg.Any<InstallationEntity>());
    }

    [Fact]
    public async Task Remove_sets_date_to_on_active_installation()
    {
        var current = new InstallationEntity
        {
            Id = 7, UserId = "u1", ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Tracked, DateTo = null
        };
        _installationRepo.GetAsync(7).Returns(current);

        var handler = new RemoveInstallationCommandHandler(_komUoW);
        var res = await handler.Handle(new RemoveInstallationCommand
        {
            UserId = "u1", InstallationId = 7, DateTo = new DateTime(2026, 7, 1)
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Update(Arg.Is<InstallationEntity>(i => i.Id == 7 && i.DateTo != null));
    }

    [Fact]
    public async Task Update_tracked_edits_bike_position_and_dates()
    {
        _installationRepo.GetAsync(7).Returns(new InstallationEntity
        {
            Id = 7, UserId = "u1", ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Tracked, DateTo = null
        });
        _installationRepo.GetActiveTrackedByComponentAsync(5).Returns(new InstallationEntity { Id = 7, UserId = "u1", ComponentId = 5 });
        OwnBike(4);

        var handler = new UpdateInstallationCommandHandler(_komUoW);
        var res = await handler.Handle(new UpdateInstallationCommand
        {
            UserId = "u1", InstallationId = 7, BikeId = 4, Position = InstallationPosition.Front,
            DateFrom = new DateTime(2026, 2, 2), DateTo = null
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Update(Arg.Is<InstallationEntity>(i =>
            i.Id == 7 && i.BikeId == 4 && i.Position == InstallationPosition.Front
            && i.DateFrom == DateTime.SpecifyKind(new DateTime(2026, 2, 2), DateTimeKind.Utc)));
    }

    [Fact]
    public async Task Update_tracked_reopening_when_another_active_exists_conflicts()
    {
        _installationRepo.GetAsync(7).Returns(new InstallationEntity
        {
            Id = 7, UserId = "u1", ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Tracked, DateTo = new DateTime(2026, 1, 1)
        });
        // A DIFFERENT active installation already holds the component.
        _installationRepo.GetActiveTrackedByComponentAsync(5).Returns(new InstallationEntity { Id = 9, UserId = "u1", ComponentId = 5 });
        OwnBike(4);

        var handler = new UpdateInstallationCommandHandler(_komUoW);
        var res = await handler.Handle(new UpdateInstallationCommand
        {
            UserId = "u1", InstallationId = 7, BikeId = 4, DateFrom = new DateTime(2026, 1, 1), DateTo = null
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
        _installationRepo.DidNotReceive().Update(Arg.Any<InstallationEntity>());
    }

    [Fact]
    public async Task Update_manual_edits_static_totals()
    {
        _installationRepo.GetAsync(7).Returns(new InstallationEntity
        {
            Id = 7, UserId = "u1", ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Manual
        });
        OwnBike(4);

        var handler = new UpdateInstallationCommandHandler(_komUoW);
        var res = await handler.Handle(new UpdateInstallationCommand
        {
            UserId = "u1", InstallationId = 7, BikeId = 4, ManualDistanceKm = 999
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Update(Arg.Is<InstallationEntity>(i =>
            i.BikeId == 4 && i.ManualDistanceKm == 999 && i.DateFrom == null));
    }

    [Fact]
    public async Task Delete_other_users_installation_is_forbidden()
    {
        _installationRepo.GetAsync(7).Returns(new InstallationEntity { Id = 7, UserId = "other", ComponentId = 5 });

        var handler = new DeleteInstallationCommandHandler(_komUoW);
        var res = await handler.Handle(new DeleteInstallationCommand { UserId = "u1", InstallationId = 7 }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ForbiddenError>();
        _installationRepo.DidNotReceive().Delete(Arg.Any<InstallationEntity>());
    }

    [Fact]
    public async Task Delete_own_installation_succeeds()
    {
        _installationRepo.GetAsync(7).Returns(new InstallationEntity { Id = 7, UserId = "u1", ComponentId = 5 });

        var handler = new DeleteInstallationCommandHandler(_komUoW);
        var res = await handler.Handle(new DeleteInstallationCommand { UserId = "u1", InstallationId = 7 }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Delete(Arg.Any<InstallationEntity>());
    }

    [Fact]
    public async Task Delete_component_with_installation_history_conflicts()
    {
        _componentRepo.GetComponentAsync(5).Returns(new ComponentEntity
        {
            Id = 5, UserId = "u1", Name = "Chain", Category = ComponentCategory.Chain
        });
        _installationRepo.AnyByComponentAsync(5).Returns(true);

        var handler = new KomTracker.Application.Commands.Component.DeleteComponentCommandHandler(_komUoW);
        var res = await handler.Handle(new KomTracker.Application.Commands.Component.DeleteComponentCommand
        {
            Id = 5, UserId = "u1"
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
        _componentRepo.DidNotReceive().DeleteComponent(Arg.Any<ComponentEntity>());
    }

    [Fact]
    public async Task Delete_bike_clears_its_installations_first()
    {
        _bikeRepo.GetBikeAsync(3).Returns(new BikeEntity { Id = 3, UserId = "u1", Name = "Road" });

        var handler = new KomTracker.Application.Commands.Bike.DeleteBikeCommandHandler(_komUoW);
        var res = await handler.Handle(new KomTracker.Application.Commands.Bike.DeleteBikeCommand
        {
            Id = 3, UserId = "u1"
        }, CancellationToken.None);

        res.Should().BeSuccess();
        await _installationRepo.Received().DeleteByBikeAsync(3);
        _bikeRepo.Received().DeleteBike(Arg.Any<BikeEntity>());
    }
}

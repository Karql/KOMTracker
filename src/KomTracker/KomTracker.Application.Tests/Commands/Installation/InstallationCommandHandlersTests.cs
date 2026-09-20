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
using System.Collections.Generic;
using System.Linq;
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
    private readonly MediatR.IMediator _mediator;

    public InstallationCommandHandlersTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _installationRepo = Substitute.For<IInstallationRepository>();
        _componentRepo = Substitute.For<IComponentRepository>();
        _bikeRepo = Substitute.For<IBikeRepository>();
        _mediator = Substitute.For<MediatR.IMediator>();
        _komUoW.GetRepository<IInstallationRepository>().Returns(_installationRepo);
        _komUoW.GetRepository<IComponentRepository>().Returns(_componentRepo);
        _komUoW.GetRepository<IBikeRepository>().Returns(_bikeRepo);
    }

    // isMeta defaults true so a component used as an install target passes the meta-parent check; set false to test the guard.
    private void OwnComponent(int id, string userId = "u1", int? warehouseId = null, bool isMeta = true)
        => _componentRepo.GetComponentAsync(id).Returns(new ComponentEntity
        {
            Id = id, UserId = userId, Name = "Chain", Category = ComponentCategory.Chain, WarehouseId = warehouseId, IsMetaComponent = isMeta
        });

    private void OwnBike(int id, string userId = "u1")
        => _bikeRepo.GetBikeAsync(id).Returns(new BikeEntity { Id = id, UserId = userId, Name = "Road" });

    // Stub the component's active Tracked placements (drives the D-7 invariant).
    private void ActiveTracked(int componentId, params InstallationEntity[] active)
        => _installationRepo.GetActiveTrackedInstallationsByComponentAsync(componentId).Returns(active);

    private static InstallationEntity OnBike(int componentId, int bikeId, int id = 0)
        => new() { Id = id, UserId = "u1", ComponentId = componentId, BikeId = bikeId, Type = ComponentInstallationType.Tracked };

    private static InstallationEntity InComponent(int componentId, int parentComponentId, int id = 0)
        => new() { Id = id, UserId = "u1", ComponentId = componentId, ParentComponentId = parentComponentId, Type = ComponentInstallationType.Tracked };

    // ---- Install onto a bike -------------------------------------------------

    [Fact]
    public async Task Install_tracked_adds_row_and_clears_warehouse()
    {
        OwnComponent(5, warehouseId: 9);
        OwnBike(3);

        var handler = new InstallComponentCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, BikeId = 3,
            Type = ComponentInstallationType.Tracked, DateFrom = new DateTime(2026, 1, 1), Position = InstallationPosition.Rear
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Add(Arg.Is<InstallationEntity>(i =>
            i.ComponentId == 5 && i.BikeId == 3 && i.ParentComponentId == null
            && i.Type == ComponentInstallationType.Tracked && i.DateTo == null));
        _componentRepo.Received().UpdateComponent(Arg.Is<ComponentEntity>(c => c.WarehouseId == null));
    }

    [Fact]
    public async Task Install_triggers_mileage_recompute_for_the_component()
    {
        OwnComponent(5);
        OwnBike(3);

        var handler = new InstallComponentCommandHandler(_komUoW, _mediator);
        await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Tracked, DateFrom = new DateTime(2026, 1, 1)
        }, CancellationToken.None);

        await _mediator.Received().Publish(
            Arg.Is<KomTracker.Application.Notifications.Component.InstallationChangedNotification>(n => n.ComponentId == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Install_onto_same_bike_twice_conflicts()
    {
        OwnComponent(5);
        OwnBike(3);
        ActiveTracked(5, OnBike(5, 3, id: 1));

        var handler = new InstallComponentCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Tracked, DateFrom = new DateTime(2026, 1, 1)
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
        _installationRepo.DidNotReceive().Add(Arg.Any<InstallationEntity>());
    }

    [Fact]
    public async Task Install_onto_another_bike_is_allowed_multi_bike()
    {
        OwnComponent(5);
        OwnBike(4);
        ActiveTracked(5, OnBike(5, 3, id: 1));   // already on bike 3

        var handler = new InstallComponentCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, BikeId = 4, Type = ComponentInstallationType.Tracked, DateFrom = new DateTime(2026, 1, 1)
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Add(Arg.Is<InstallationEntity>(i => i.BikeId == 4));
    }

    [Fact]
    public async Task Install_onto_bike_while_inside_a_component_conflicts()
    {
        OwnComponent(5);
        OwnBike(3);
        ActiveTracked(5, InComponent(5, 8, id: 1));   // currently inside component 8

        var handler = new InstallComponentCommandHandler(_komUoW, _mediator);
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

        var handler = new InstallComponentCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Manual,
            ManualDistanceKm = 1200, ManualMovingHours = 40, ManualElevationM = 8000
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Add(Arg.Is<InstallationEntity>(i =>
            i.Type == ComponentInstallationType.Manual && i.DateFrom == null && i.DateTo == null && i.ManualDistanceKm == 1200));
        // Manual install never runs the active-placement invariant.
        await _installationRepo.DidNotReceive().GetActiveTrackedInstallationsByComponentAsync(Arg.Any<int>());
    }

    [Fact]
    public async Task Install_other_users_component_is_forbidden()
    {
        OwnComponent(5, userId: "other");

        var handler = new InstallComponentCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Manual
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ForbiddenError>();
    }

    // ---- Install into a component (2b-ii) -----------------------------------

    [Fact]
    public async Task Install_into_component_adds_row_with_parent()
    {
        OwnComponent(5);            // tyre
        OwnComponent(8);            // wheel (parent)

        var handler = new InstallComponentCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, ParentComponentId = 8,
            Type = ComponentInstallationType.Tracked, DateFrom = new DateTime(2026, 1, 1)
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Add(Arg.Is<InstallationEntity>(i =>
            i.ComponentId == 5 && i.ParentComponentId == 8 && i.BikeId == null && i.DateTo == null));
    }

    [Fact]
    public async Task Install_into_non_meta_component_conflicts()
    {
        OwnComponent(5);
        OwnComponent(8, isMeta: false);   // target is not a meta component

        var handler = new InstallComponentCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, ParentComponentId = 8, Type = ComponentInstallationType.Tracked, DateFrom = new DateTime(2026, 1, 1)
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
        _installationRepo.DidNotReceive().Add(Arg.Any<InstallationEntity>());
    }

    [Fact]
    public async Task Install_into_component_while_on_a_bike_conflicts()
    {
        OwnComponent(5);
        OwnComponent(8);
        ActiveTracked(5, OnBike(5, 3, id: 1));   // exclusive: can't go into a component while on a bike

        var handler = new InstallComponentCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, ParentComponentId = 8, Type = ComponentInstallationType.Tracked, DateFrom = new DateTime(2026, 1, 1)
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
        _installationRepo.DidNotReceive().Add(Arg.Any<InstallationEntity>());
    }

    [Fact]
    public async Task Install_into_self_conflicts()
    {
        OwnComponent(5);

        var handler = new InstallComponentCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, ParentComponentId = 5, Type = ComponentInstallationType.Tracked, DateFrom = new DateTime(2026, 1, 1)
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
    }

    [Fact]
    public async Task Install_into_component_that_is_itself_nested_conflicts_one_level()
    {
        OwnComponent(5);
        OwnComponent(8);
        ActiveTracked(8, InComponent(8, 9, id: 2));   // parent 8 is itself inside component 9

        var handler = new InstallComponentCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, ParentComponentId = 8, Type = ComponentInstallationType.Tracked, DateFrom = new DateTime(2026, 1, 1)
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
    }

    [Fact]
    public async Task Install_into_component_when_source_has_children_conflicts_one_level()
    {
        OwnComponent(5);
        OwnComponent(8);
        // component 5 already contains a child → it can't itself be nested.
        _installationRepo.GetActiveChildrenByParentComponentsAsync(Arg.Any<IReadOnlyCollection<int>>())
            .Returns(new[] { InComponent(6, 5, id: 3) });

        var handler = new InstallComponentCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new InstallComponentCommand
        {
            UserId = "u1", ComponentId = 5, ParentComponentId = 8, Type = ComponentInstallationType.Tracked, DateFrom = new DateTime(2026, 1, 1)
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
    }

    // ---- Move ---------------------------------------------------------------

    [Fact]
    public async Task Move_closes_current_and_opens_new_window()
    {
        var current = OnBike(5, 3, id: 7);
        _installationRepo.GetAsync(7).Returns(current);
        OwnBike(4);

        var handler = new MoveInstallationCommandHandler(_komUoW, _mediator);
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
    public async Task Move_into_component_opens_component_window()
    {
        var current = OnBike(5, 3, id: 7);
        _installationRepo.GetAsync(7).Returns(current);
        OwnComponent(8);
        ActiveTracked(5, current);   // only the row being moved is active → excluded

        var handler = new MoveInstallationCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new MoveInstallationCommand
        {
            UserId = "u1", InstallationId = 7, NewParentComponentId = 8, MoveDate = new DateTime(2026, 6, 1)
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Add(Arg.Is<InstallationEntity>(i => i.ParentComponentId == 8 && i.BikeId == null));
    }

    [Fact]
    public async Task Move_non_current_installation_conflicts()
    {
        _installationRepo.GetAsync(7).Returns(new InstallationEntity
        {
            Id = 7, UserId = "u1", ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Tracked, DateTo = new DateTime(2026, 1, 1)
        });

        var handler = new MoveInstallationCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new MoveInstallationCommand
        {
            UserId = "u1", InstallationId = 7, NewBikeId = 4, MoveDate = new DateTime(2026, 6, 1)
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
        _installationRepo.DidNotReceive().Add(Arg.Any<InstallationEntity>());
    }

    // ---- Remove / Update ----------------------------------------------------

    [Fact]
    public async Task Remove_sets_date_to_on_active_installation()
    {
        _installationRepo.GetAsync(7).Returns(OnBike(5, 3, id: 7));

        var handler = new RemoveInstallationCommandHandler(_komUoW, _mediator);
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
        var row = OnBike(5, 3, id: 7);
        _installationRepo.GetAsync(7).Returns(row);
        ActiveTracked(5, row);   // only self is active → invariant excludes it
        OwnBike(4);

        var handler = new UpdateInstallationCommandHandler(_komUoW, _mediator);
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
    public async Task Update_tracked_reopening_when_another_bike_active_exists_conflicts()
    {
        _installationRepo.GetAsync(7).Returns(new InstallationEntity
        {
            Id = 7, UserId = "u1", ComponentId = 5, ParentComponentId = 8, Type = ComponentInstallationType.Tracked, DateTo = new DateTime(2026, 1, 1)
        });
        // Re-pointing to a bike, but the component already has an active bike placement (Id 9) → homogeneity conflict.
        ActiveTracked(5, OnBike(5, 3, id: 9));
        OwnBike(4);

        var handler = new UpdateInstallationCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new UpdateInstallationCommand
        {
            UserId = "u1", InstallationId = 7, ParentComponentId = 8, DateFrom = new DateTime(2026, 1, 1), DateTo = null
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

        var handler = new UpdateInstallationCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new UpdateInstallationCommand
        {
            UserId = "u1", InstallationId = 7, BikeId = 4, ManualDistanceKm = 999
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Update(Arg.Is<InstallationEntity>(i =>
            i.BikeId == 4 && i.ManualDistanceKm == 999 && i.DateFrom == null));
    }

    // ---- Delete -------------------------------------------------------------

    [Fact]
    public async Task Delete_other_users_installation_is_forbidden()
    {
        _installationRepo.GetAsync(7).Returns(new InstallationEntity { Id = 7, UserId = "other", ComponentId = 5 });

        var handler = new DeleteInstallationCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new DeleteInstallationCommand { UserId = "u1", InstallationId = 7 }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ForbiddenError>();
        _installationRepo.DidNotReceive().Delete(Arg.Any<InstallationEntity>());
    }

    [Fact]
    public async Task Delete_own_installation_succeeds()
    {
        _installationRepo.GetAsync(7).Returns(new InstallationEntity { Id = 7, UserId = "u1", ComponentId = 5 });

        var handler = new DeleteInstallationCommandHandler(_komUoW, _mediator);
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
    public async Task Delete_component_that_is_a_parent_of_history_conflicts()
    {
        _componentRepo.GetComponentAsync(8).Returns(new ComponentEntity
        {
            Id = 8, UserId = "u1", Name = "Wheel", Category = ComponentCategory.Other
        });
        _installationRepo.AnyByComponentAsync(8).Returns(false);
        _installationRepo.AnyByParentComponentAsync(8).Returns(true);   // a tyre was once in this wheel

        var handler = new KomTracker.Application.Commands.Component.DeleteComponentCommandHandler(_komUoW);
        var res = await handler.Handle(new KomTracker.Application.Commands.Component.DeleteComponentCommand
        {
            Id = 8, UserId = "u1"
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
        _componentRepo.DidNotReceive().DeleteComponent(Arg.Any<ComponentEntity>());
    }

    [Fact]
    public async Task Force_delete_removes_own_history_and_deletes_the_component()
    {
        _componentRepo.GetComponentAsync(5).Returns(new ComponentEntity
        {
            Id = 5, UserId = "u1", Name = "Chain", Category = ComponentCategory.Chain
        });
        _installationRepo.AnyByComponentAsync(5).Returns(true);
        _installationRepo.GetByComponentAsync(5).Returns(new[]
        {
            new InstallationEntity { Id = 11, ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Tracked }
        });

        var handler = new KomTracker.Application.Commands.Component.DeleteComponentCommandHandler(_komUoW);
        var res = await handler.Handle(new KomTracker.Application.Commands.Component.DeleteComponentCommand
        {
            Id = 5, UserId = "u1", Force = true
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Delete(Arg.Is<InstallationEntity>(i => i.Id == 11));
        _componentRepo.Received().DeleteComponent(Arg.Is<ComponentEntity>(c => c.Id == 5));
    }

    [Fact]
    public async Task Force_delete_of_component_with_parts_inside_still_conflicts()
    {
        // A meta component that holds/held parts can't be forced away — its as-parent rows are the children's history.
        _componentRepo.GetComponentAsync(8).Returns(new ComponentEntity
        {
            Id = 8, UserId = "u1", Name = "Wheel", Category = ComponentCategory.Wheel, IsMetaComponent = true
        });
        _installationRepo.AnyByParentComponentAsync(8).Returns(true);

        var handler = new KomTracker.Application.Commands.Component.DeleteComponentCommandHandler(_komUoW);
        var res = await handler.Handle(new KomTracker.Application.Commands.Component.DeleteComponentCommand
        {
            Id = 8, UserId = "u1", Force = true
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
        _installationRepo.DidNotReceive().Delete(Arg.Any<InstallationEntity>());
        _componentRepo.DidNotReceive().DeleteComponent(Arg.Any<ComponentEntity>());
    }

    [Fact]
    public async Task Turning_off_meta_while_it_has_children_conflicts()
    {
        _componentRepo.GetComponentAsync(8).Returns(new ComponentEntity
        {
            Id = 8, UserId = "u1", Name = "Wheel", Category = ComponentCategory.Wheel, IsMetaComponent = true
        });
        _installationRepo.AnyByParentComponentAsync(8).Returns(true);

        var handler = new KomTracker.Application.Commands.Component.SaveComponentCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new KomTracker.Application.Commands.Component.SaveComponentCommand
        {
            Id = 8, UserId = "u1", Name = "Wheel", Category = ComponentCategory.Wheel, IsMetaComponent = false
        }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ConflictError>();
        _componentRepo.DidNotReceive().UpdateComponent(Arg.Any<ComponentEntity>());
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

    // ---- D-18 lifecycle cascade / detach ------------------------------------

    [Fact]
    public async Task Sell_component_cascades_sold_to_children_and_closes_windows()
    {
        _componentRepo.GetComponentAsync(8).Returns(new ComponentEntity
        {
            Id = 8, UserId = "u1", Name = "Wheel", Category = ComponentCategory.Other, Lifecycle = ComponentLifecycle.Active
        });
        // Wheel 8 is on a bike; tyre 5 is inside the wheel.
        ActiveTracked(8, OnBike(8, 3, id: 20));
        _installationRepo.GetActiveChildrenByParentComponentsAsync(Arg.Any<IReadOnlyCollection<int>>())
            .Returns(new[] { InComponent(5, 8, id: 21) });
        _componentRepo.GetComponentAsync(5).Returns(new ComponentEntity
        {
            Id = 5, UserId = "u1", Name = "Tyre", Category = ComponentCategory.Tire, Lifecycle = ComponentLifecycle.Active
        });

        var handler = new KomTracker.Application.Commands.Component.ChangeComponentLifecycleCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new KomTracker.Application.Commands.Component.ChangeComponentLifecycleCommand
        {
            Id = 8, UserId = "u1", Lifecycle = ComponentLifecycle.Sold, SaleDate = new DateTime(2026, 5, 1), SalePrice = 100
        }, CancellationToken.None);

        res.Should().BeSuccess();
        // Own window closed + child window closed.
        _installationRepo.Received().Update(Arg.Is<InstallationEntity>(i => i.Id == 20 && i.DateTo != null));
        _installationRepo.Received().Update(Arg.Is<InstallationEntity>(i => i.Id == 21 && i.DateTo != null));
        // Child marked Sold.
        _componentRepo.Received().UpdateComponent(Arg.Is<ComponentEntity>(c => c.Id == 5 && c.Lifecycle == ComponentLifecycle.Sold));
    }

    [Fact]
    public async Task Archive_component_detaches_children_without_changing_their_lifecycle()
    {
        _componentRepo.GetComponentAsync(8).Returns(new ComponentEntity
        {
            Id = 8, UserId = "u1", Name = "Wheel", Category = ComponentCategory.Other, Lifecycle = ComponentLifecycle.Active
        });
        _installationRepo.GetActiveChildrenByParentComponentsAsync(Arg.Any<IReadOnlyCollection<int>>())
            .Returns(new[] { InComponent(5, 8, id: 21) });

        var handler = new KomTracker.Application.Commands.Component.ChangeComponentLifecycleCommandHandler(_komUoW, _mediator);
        var res = await handler.Handle(new KomTracker.Application.Commands.Component.ChangeComponentLifecycleCommand
        {
            Id = 8, UserId = "u1", Lifecycle = ComponentLifecycle.Archived
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _installationRepo.Received().Update(Arg.Is<InstallationEntity>(i => i.Id == 21 && i.DateTo != null));
        // Child lifecycle NOT changed on archive (only the child installation was closed).
        _componentRepo.DidNotReceive().UpdateComponent(Arg.Is<ComponentEntity>(c => c.Id == 5));
    }
}

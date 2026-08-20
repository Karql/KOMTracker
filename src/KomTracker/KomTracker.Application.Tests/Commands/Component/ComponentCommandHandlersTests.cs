#nullable enable
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Component;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Component;
using KomTracker.Domain.Entities.Warehouse;
using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Component;

public class ComponentCommandHandlersTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IComponentRepository _componentRepo;
    private readonly IWarehouseRepository _warehouseRepo;

    public ComponentCommandHandlersTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _componentRepo = Substitute.For<IComponentRepository>();
        _warehouseRepo = Substitute.For<IWarehouseRepository>();

        _komUoW.GetRepository<IComponentRepository>().Returns(_componentRepo);
        _komUoW.GetRepository<IWarehouseRepository>().Returns(_warehouseRepo);
    }

    private SaveComponentCommandHandler SaveHandler => new(_komUoW);

    [Fact]
    public async Task Create_adds_component()
    {
        var cmd = new SaveComponentCommand { Id = null, UserId = "u1", Name = "Chain", Category = ComponentCategory.Chain };

        var res = await SaveHandler.Handle(cmd, CancellationToken.None);

        res.Should().BeSuccess();
        _componentRepo.Received().AddComponent(Arg.Is<ComponentEntity>(c => c.UserId == "u1" && c.Name == "Chain"));
    }

    [Fact]
    public async Task Create_with_foreign_warehouse_is_rejected()
    {
        _warehouseRepo.GetWarehouseAsync(9).Returns(new WarehouseEntity { Id = 9, UserId = "other", Name = "Garage" });

        var cmd = new SaveComponentCommand { Id = null, UserId = "u1", Name = "Chain", Category = ComponentCategory.Chain, WarehouseId = 9 };

        var res = await SaveHandler.Handle(cmd, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ValidationError>();
        _componentRepo.DidNotReceive().AddComponent(Arg.Any<ComponentEntity>());
    }

    [Fact]
    public async Task Create_with_own_warehouse_is_accepted()
    {
        _warehouseRepo.GetWarehouseAsync(9).Returns(new WarehouseEntity { Id = 9, UserId = "u1", Name = "Garage" });

        var cmd = new SaveComponentCommand { Id = null, UserId = "u1", Name = "Chain", Category = ComponentCategory.Chain, WarehouseId = 9 };

        var res = await SaveHandler.Handle(cmd, CancellationToken.None);

        res.Should().BeSuccess();
        _componentRepo.Received().AddComponent(Arg.Is<ComponentEntity>(c => c.WarehouseId == 9));
    }

    [Fact]
    public async Task Update_missing_component_returns_not_found()
    {
        _componentRepo.GetComponentAsync(5).Returns((ComponentEntity?)null);

        var cmd = new SaveComponentCommand { Id = 5, UserId = "u1", Name = "Chain", Category = ComponentCategory.Chain };

        var res = await SaveHandler.Handle(cmd, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<NotFoundError>();
    }

    [Fact]
    public async Task Update_other_users_component_is_forbidden()
    {
        _componentRepo.GetComponentAsync(5).Returns(new ComponentEntity { Id = 5, UserId = "other", Name = "old" });

        var cmd = new SaveComponentCommand { Id = 5, UserId = "u1", Name = "Chain", Category = ComponentCategory.Chain };

        var res = await SaveHandler.Handle(cmd, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ForbiddenError>();
    }

    [Fact]
    public async Task ChangeLifecycle_sold_sets_sale_and_saves()
    {
        _componentRepo.GetComponentAsync(5).Returns(new ComponentEntity { Id = 5, UserId = "u1", Name = "Chain" });

        var handler = new ChangeComponentLifecycleCommandHandler(_komUoW);
        var res = await handler.Handle(new ChangeComponentLifecycleCommand
        {
            Id = 5,
            UserId = "u1",
            Lifecycle = ComponentLifecycle.Sold,
            SaleDate = new DateTime(2026, 1, 1),
            SalePrice = 100
        }, CancellationToken.None);

        res.Should().BeSuccess();
        _componentRepo.Received().UpdateComponent(Arg.Is<ComponentEntity>(c =>
            c.Lifecycle == ComponentLifecycle.Sold && c.SalePrice == 100 && c.SaleDate != null));
    }

    [Fact]
    public async Task Delete_other_users_component_is_forbidden()
    {
        _componentRepo.GetComponentAsync(5).Returns(new ComponentEntity { Id = 5, UserId = "other", Name = "Chain" });

        var handler = new DeleteComponentCommandHandler(_komUoW);
        var res = await handler.Handle(new DeleteComponentCommand { Id = 5, UserId = "u1" }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ForbiddenError>();
        _componentRepo.DidNotReceive().DeleteComponent(Arg.Any<ComponentEntity>());
    }

    [Fact]
    public async Task Delete_own_component_succeeds()
    {
        _componentRepo.GetComponentAsync(5).Returns(new ComponentEntity { Id = 5, UserId = "u1", Name = "Chain" });

        var handler = new DeleteComponentCommandHandler(_komUoW);
        var res = await handler.Handle(new DeleteComponentCommand { Id = 5, UserId = "u1" }, CancellationToken.None);

        res.Should().BeSuccess();
        _componentRepo.Received().DeleteComponent(Arg.Any<ComponentEntity>());
    }
}

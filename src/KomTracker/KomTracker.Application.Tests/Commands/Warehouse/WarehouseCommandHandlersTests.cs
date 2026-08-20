#nullable enable
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Warehouse;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Warehouse;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Warehouse;

public class WarehouseCommandHandlersTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IWarehouseRepository _warehouseRepo;
    private readonly IComponentRepository _componentRepo;

    public WarehouseCommandHandlersTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _warehouseRepo = Substitute.For<IWarehouseRepository>();
        _componentRepo = Substitute.For<IComponentRepository>();
        _komUoW.GetRepository<IWarehouseRepository>().Returns(_warehouseRepo);
        _komUoW.GetRepository<IComponentRepository>().Returns(_componentRepo);
    }

    [Fact]
    public async Task Create_adds_warehouse()
    {
        var handler = new SaveWarehouseCommandHandler(_komUoW);

        var res = await handler.Handle(new SaveWarehouseCommand { Id = null, UserId = "u1", Name = "Garage" }, CancellationToken.None);

        res.Should().BeSuccess();
        _warehouseRepo.Received().AddWarehouse(Arg.Is<WarehouseEntity>(w => w.UserId == "u1" && w.Name == "Garage"));
    }

    [Fact]
    public async Task Update_other_users_warehouse_is_forbidden()
    {
        _warehouseRepo.GetWarehouseAsync(3).Returns(new WarehouseEntity { Id = 3, UserId = "other", Name = "old" });

        var handler = new SaveWarehouseCommandHandler(_komUoW);
        var res = await handler.Handle(new SaveWarehouseCommand { Id = 3, UserId = "u1", Name = "Garage" }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ForbiddenError>();
    }

    [Fact]
    public async Task Delete_missing_warehouse_returns_not_found()
    {
        _warehouseRepo.GetWarehouseAsync(3).Returns((WarehouseEntity?)null);

        var handler = new DeleteWarehouseCommandHandler(_komUoW);
        var res = await handler.Handle(new DeleteWarehouseCommand { Id = 3, UserId = "u1" }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<NotFoundError>();
        _warehouseRepo.DidNotReceive().DeleteWarehouse(Arg.Any<WarehouseEntity>());
    }

    [Fact]
    public async Task Delete_own_warehouse_succeeds()
    {
        _warehouseRepo.GetWarehouseAsync(3).Returns(new WarehouseEntity { Id = 3, UserId = "u1", Name = "Garage" });

        var handler = new DeleteWarehouseCommandHandler(_komUoW);
        var res = await handler.Handle(new DeleteWarehouseCommand { Id = 3, UserId = "u1" }, CancellationToken.None);

        res.Should().BeSuccess();
        _warehouseRepo.Received().DeleteWarehouse(Arg.Any<WarehouseEntity>());
    }
}

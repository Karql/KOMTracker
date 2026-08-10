#nullable enable
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Bike;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Bike;
using NSubstitute;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Bike;

public class BikeCommandHandlersTests
{
    private const string UserId = "user-1";
    private const string OtherUserId = "user-2";

    private readonly IKOMUnitOfWork _komUoW;
    private readonly IBikeRepository _bikeRepository;

    public BikeCommandHandlersTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _bikeRepository = Substitute.For<IBikeRepository>();
        _komUoW.GetRepository<IBikeRepository>().Returns(_bikeRepository);
    }

    [Fact]
    public async Task SaveBike_creates_active_bike_when_id_is_null()
    {
        var handler = new SaveBikeCommandHandler(_komUoW);

        var result = await handler.Handle(new SaveBikeCommand
        {
            Id = null,
            UserId = UserId,
            Name = "Canyon",
            Type = BikeType.Road
        }, CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Name.Should().Be("Canyon");
        result.Value.Lifecycle.Should().Be(BikeLifecycle.Active);
        result.Value.UserId.Should().Be(UserId);

        _bikeRepository.Received().AddBike(Arg.Is<BikeEntity>(b =>
            b.UserId == UserId && b.Name == "Canyon" && b.Lifecycle == BikeLifecycle.Active));
        await _komUoW.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task SaveBike_update_fails_with_not_found()
    {
        _bikeRepository.GetBikeAsync(5).Returns((BikeEntity?)null);
        var handler = new SaveBikeCommandHandler(_komUoW);

        var result = await handler.Handle(new SaveBikeCommand { Id = 5, UserId = UserId, Name = "X", Type = BikeType.Road }, CancellationToken.None);

        result.Should().BeFailure();
        result.HasError<NotFoundError>().Should().BeTrue();
        await _komUoW.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task SaveBike_update_fails_with_forbidden_for_other_users_bike()
    {
        _bikeRepository.GetBikeAsync(5).Returns(new BikeEntity { Id = 5, UserId = OtherUserId, Name = "X" });
        var handler = new SaveBikeCommandHandler(_komUoW);

        var result = await handler.Handle(new SaveBikeCommand { Id = 5, UserId = UserId, Name = "Y", Type = BikeType.Road }, CancellationToken.None);

        result.Should().BeFailure();
        result.HasError<ForbiddenError>().Should().BeTrue();
        await _komUoW.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task SaveBike_update_applies_changes()
    {
        var bike = new BikeEntity { Id = 5, UserId = UserId, Name = "Old", Type = BikeType.Road };
        _bikeRepository.GetBikeAsync(5).Returns(bike);
        var handler = new SaveBikeCommandHandler(_komUoW);

        var result = await handler.Handle(new SaveBikeCommand { Id = 5, UserId = UserId, Name = "New", Type = BikeType.Gravel }, CancellationToken.None);

        result.Should().BeSuccess();
        bike.Name.Should().Be("New");
        bike.Type.Should().Be(BikeType.Gravel);
        await _komUoW.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task ChangeLifecycle_sells_bike_and_sets_sale_details()
    {
        var bike = new BikeEntity { Id = 5, UserId = UserId, Name = "X", Lifecycle = BikeLifecycle.Active };
        _bikeRepository.GetBikeAsync(5).Returns(bike);
        var handler = new ChangeBikeLifecycleCommandHandler(_komUoW);

        var result = await handler.Handle(new ChangeBikeLifecycleCommand
        {
            Id = 5,
            UserId = UserId,
            Lifecycle = BikeLifecycle.Sold,
            SaleDate = new DateTime(2026, 1, 1),
            SalePrice = 1200
        }, CancellationToken.None);

        result.Should().BeSuccess();
        bike.Lifecycle.Should().Be(BikeLifecycle.Sold);
        bike.SalePrice.Should().Be(1200);
        bike.SaleDate.Should().NotBeNull();
        await _komUoW.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task ChangeLifecycle_clears_sale_details_when_leaving_sold()
    {
        var bike = new BikeEntity
        {
            Id = 5,
            UserId = UserId,
            Name = "X",
            Lifecycle = BikeLifecycle.Sold,
            SaleDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            SalePrice = 1200
        };
        _bikeRepository.GetBikeAsync(5).Returns(bike);
        var handler = new ChangeBikeLifecycleCommandHandler(_komUoW);

        var result = await handler.Handle(new ChangeBikeLifecycleCommand
        {
            Id = 5,
            UserId = UserId,
            Lifecycle = BikeLifecycle.Active
        }, CancellationToken.None);

        result.Should().BeSuccess();
        bike.Lifecycle.Should().Be(BikeLifecycle.Active);
        bike.SaleDate.Should().BeNull();
        bike.SalePrice.Should().BeNull();
    }

    [Fact]
    public async Task DeleteBike_fails_with_forbidden_for_other_users_bike()
    {
        _bikeRepository.GetBikeAsync(5).Returns(new BikeEntity { Id = 5, UserId = OtherUserId, Name = "X" });
        var handler = new DeleteBikeCommandHandler(_komUoW);

        var result = await handler.Handle(new DeleteBikeCommand { Id = 5, UserId = UserId }, CancellationToken.None);

        result.Should().BeFailure();
        result.HasError<ForbiddenError>().Should().BeTrue();
        _bikeRepository.DidNotReceive().DeleteBike(Arg.Any<BikeEntity>());
        await _komUoW.DidNotReceive().SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteBike_removes_and_saves()
    {
        var bike = new BikeEntity { Id = 5, UserId = UserId, Name = "X" };
        _bikeRepository.GetBikeAsync(5).Returns(bike);
        var handler = new DeleteBikeCommandHandler(_komUoW);

        var result = await handler.Handle(new DeleteBikeCommand { Id = 5, UserId = UserId }, CancellationToken.None);

        result.Should().BeSuccess();
        _bikeRepository.Received().DeleteBike(bike);
        await _komUoW.Received().SaveChangesAsync();
    }
}

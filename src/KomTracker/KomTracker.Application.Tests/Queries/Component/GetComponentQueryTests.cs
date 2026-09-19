#nullable enable
using FluentAssertions;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Queries.Component;
using KomTracker.Domain.Entities.Bike;
using KomTracker.Domain.Entities.Component;
using NSubstitute;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Queries.Component;

public class GetComponentQueryTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IComponentRepository _componentRepo;
    private readonly IInstallationRepository _installationRepo;
    private readonly IWarehouseRepository _warehouseRepo;
    private readonly IBikeRepository _bikeRepo;
    private readonly IComponentMileageRepository _mileageRepo;
    private readonly KomTracker.Application.Services.ComponentMileageService _mileageService;

    public GetComponentQueryTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _componentRepo = Substitute.For<IComponentRepository>();
        _installationRepo = Substitute.For<IInstallationRepository>();
        _warehouseRepo = Substitute.For<IWarehouseRepository>();
        _bikeRepo = Substitute.For<IBikeRepository>();
        _mileageRepo = Substitute.For<IComponentMileageRepository>();
        _komUoW.GetRepository<IComponentRepository>().Returns(_componentRepo);
        _komUoW.GetRepository<IInstallationRepository>().Returns(_installationRepo);
        _komUoW.GetRepository<IWarehouseRepository>().Returns(_warehouseRepo);
        _komUoW.GetRepository<IBikeRepository>().Returns(_bikeRepo);
        _komUoW.GetRepository<IComponentMileageRepository>().Returns(_mileageRepo);
        _mileageService = new KomTracker.Application.Services.ComponentMileageService(_komUoW);
    }

    [Fact]
    public async Task Resolves_parent_component_placement()
    {
        _componentRepo.GetComponentAsync(5).Returns(new ComponentEntity { Id = 5, UserId = "u1", Name = "Tyre", Category = ComponentCategory.Tire });
        _componentRepo.GetComponentsAsync("u1", true).Returns(new[]
        {
            new ComponentEntity { Id = 5, UserId = "u1", Name = "Tyre", Category = ComponentCategory.Tire },
            new ComponentEntity { Id = 8, UserId = "u1", Name = "Wheel", Category = ComponentCategory.Wheel }
        });
        _bikeRepo.GetBikesAsync("u1", true).Returns(new List<BikeEntity>());
        _installationRepo.GetActiveTrackedInstallationsByComponentAsync(5).Returns(new[]
        {
            new InstallationEntity { Id = 1, UserId = "u1", ComponentId = 5, ParentComponentId = 8, Type = ComponentInstallationType.Tracked }
        });

        var handler = new GetComponentQueryHandler(_komUoW, _mileageService);
        var result = await handler.Handle(new GetComponentQuery { Id = 5, UserId = "u1" }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ParentComponentId.Should().Be(8);
        result.ParentComponentName.Should().Be("Wheel");
        result.InstalledBikeCount.Should().Be(0);
    }

    [Fact]
    public async Task Resolves_multiple_bike_placements_and_children()
    {
        _componentRepo.GetComponentAsync(8).Returns(new ComponentEntity { Id = 8, UserId = "u1", Name = "Wheel", Category = ComponentCategory.Wheel });
        _componentRepo.GetComponentsAsync("u1", true).Returns(new[]
        {
            new ComponentEntity { Id = 8, UserId = "u1", Name = "Wheel", Category = ComponentCategory.Wheel },
            new ComponentEntity { Id = 5, UserId = "u1", Name = "Tyre", Category = ComponentCategory.Tire }
        });
        _bikeRepo.GetBikesAsync("u1", true).Returns(new[]
        {
            new BikeEntity { Id = 3, UserId = "u1", Name = "Road" },
            new BikeEntity { Id = 4, UserId = "u1", Name = "Gravel" }
        });
        _installationRepo.GetActiveTrackedInstallationsByComponentAsync(8).Returns(new[]
        {
            new InstallationEntity { Id = 1, UserId = "u1", ComponentId = 8, BikeId = 3, Type = ComponentInstallationType.Tracked },
            new InstallationEntity { Id = 2, UserId = "u1", ComponentId = 8, BikeId = 4, Type = ComponentInstallationType.Tracked }
        });
        _installationRepo.GetByParentComponentAsync(8).Returns(new[]
        {
            new InstallationEntity { Id = 9, UserId = "u1", ComponentId = 5, ParentComponentId = 8, Type = ComponentInstallationType.Tracked }
        });

        var handler = new GetComponentQueryHandler(_komUoW, _mileageService);
        var result = await handler.Handle(new GetComponentQuery { Id = 8, UserId = "u1" }, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ParentComponentId.Should().BeNull();
        result.InstalledBikeCount.Should().Be(2);
        result.Children.Should().ContainSingle();
        result.Children[0].ComponentName.Should().Be("Tyre");
    }

    [Fact]
    public async Task Other_users_component_is_hidden()
    {
        _componentRepo.GetComponentAsync(5).Returns(new ComponentEntity { Id = 5, UserId = "other", Name = "Tyre", Category = ComponentCategory.Tire });

        var handler = new GetComponentQueryHandler(_komUoW, _mileageService);
        var result = await handler.Handle(new GetComponentQuery { Id = 5, UserId = "u1" }, CancellationToken.None);

        result.Should().BeNull();
    }
}

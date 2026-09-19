#nullable enable
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Component;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Component;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Component;

public class RecalculateComponentsMileageCommandTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IComponentRepository _componentRepo;
    private readonly IInstallationRepository _installationRepo;
    private readonly IComponentMileageRepository _mileageRepo;
    private readonly RecalculateComponentsMileageCommandHandler _handler;

    public RecalculateComponentsMileageCommandTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _componentRepo = Substitute.For<IComponentRepository>();
        _installationRepo = Substitute.For<IInstallationRepository>();
        _mileageRepo = Substitute.For<IComponentMileageRepository>();
        _komUoW.GetRepository<IComponentRepository>().Returns(_componentRepo);
        _komUoW.GetRepository<IInstallationRepository>().Returns(_installationRepo);
        _komUoW.GetRepository<IComponentMileageRepository>().Returns(_mileageRepo);
        _handler = new RecalculateComponentsMileageCommandHandler(_komUoW, new ComponentMileageService(_komUoW));
    }

    [Fact]
    public async Task Upserts_projection_with_seed_plus_manual()
    {
        _componentRepo.GetByIdsAsync(Arg.Is<IReadOnlyCollection<int>>(c => c.Contains(5))).Returns(new[]
        {
            new ComponentEntity { Id = 5, UserId = "u1", Name = "Chain", Category = ComponentCategory.Chain, InitialDistanceKm = 100 }
        });
        _installationRepo.GetByComponentsAsync(Arg.Is<IReadOnlyCollection<int>>(c => c.Contains(5))).Returns(new[]
        {
            new InstallationEntity { ComponentId = 5, Type = ComponentInstallationType.Manual, ManualDistanceKm = 50 }
        });

        var res = await _handler.Handle(new RecalculateComponentsMileageCommand { ComponentIds = new[] { 5 } }, CancellationToken.None);

        res.Should().BeSuccess();
        await _mileageRepo.Received().UpsertAsync(Arg.Is<ComponentMileageEntity>(m => m.ComponentId == 5 && m.TotalDistanceKm == 150m));
    }

    [Fact]
    public async Task Expands_children_current_and_historical()
    {
        _installationRepo.GetChildComponentIdsByParentsAsync(Arg.Any<IReadOnlyCollection<int>>())
            .Returns(new[] { 5 });
        _componentRepo.GetByIdsAsync(Arg.Any<IReadOnlyCollection<int>>()).Returns(new[]
        {
            new ComponentEntity { Id = 8, UserId = "u1", Name = "Wheel", Category = ComponentCategory.Wheel },
            new ComponentEntity { Id = 5, UserId = "u1", Name = "Tyre", Category = ComponentCategory.Tire }
        });

        var res = await _handler.Handle(new RecalculateComponentsMileageCommand { ComponentIds = new[] { 8 } }, CancellationToken.None);

        res.Should().BeSuccess();
        await _mileageRepo.Received().UpsertAsync(Arg.Is<ComponentMileageEntity>(m => m.ComponentId == 8));
        await _mileageRepo.Received().UpsertAsync(Arg.Is<ComponentMileageEntity>(m => m.ComponentId == 5));
    }

    [Fact]
    public async Task Missing_component_is_skipped()
    {
        _componentRepo.GetByIdsAsync(Arg.Any<IReadOnlyCollection<int>>()).Returns(Array.Empty<ComponentEntity>());

        var res = await _handler.Handle(new RecalculateComponentsMileageCommand { ComponentIds = new[] { 99 } }, CancellationToken.None);

        res.Should().BeSuccess();
        await _mileageRepo.DidNotReceive().UpsertAsync(Arg.Any<ComponentMileageEntity>());
    }
}

#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using FluentValidation.TestHelper;
using KomTracker.API.Shared.ViewModels.ServiceRecord;
using KomTracker.Application.Commands.ServiceRecord;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Notifications.ServiceRecord;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Bike;
using KomTracker.Domain.Entities.Component;
using KomTracker.Domain.Entities.ServiceRecord;
using MediatR;
using NSubstitute;
using Xunit;

namespace KomTracker.Application.Tests.Commands.ServiceRecord;

public class ServiceRecordCommandHandlersTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IServiceRecordRepository _serviceRepo;
    private readonly IBikeRepository _bikeRepo;
    private readonly IComponentRepository _componentRepo;
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    public ServiceRecordCommandHandlersTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _serviceRepo = Substitute.For<IServiceRecordRepository>();
        _bikeRepo = Substitute.For<IBikeRepository>();
        _componentRepo = Substitute.For<IComponentRepository>();
        _komUoW.GetRepository<IServiceRecordRepository>().Returns(_serviceRepo);
        _komUoW.GetRepository<IBikeRepository>().Returns(_bikeRepo);
        _komUoW.GetRepository<IComponentRepository>().Returns(_componentRepo);

        _bikeRepo.GetByIdsAsync(Arg.Any<IReadOnlyCollection<int>>())
            .Returns(new[] { new BikeEntity { Id = 3, UserId = "u1", Name = "Trutka" } });
        _componentRepo.GetByIdsAsync(Arg.Any<IReadOnlyCollection<int>>())
            .Returns(new[] { new ComponentEntity { Id = 7, UserId = "u1", Name = "Amortyzator", Category = ComponentCategory.Fork } });
    }

    private static SaveServiceRecordCommand Save(int? id = null, params ServiceTargetRef[] targets) => new()
    {
        Id = id,
        UserId = "u1",
        Name = "Przegląd + serwis amortyzatora",
        Date = new DateTime(2026, 5, 1, 9, 30, 0),
        Place = "Biostern",
        Cost = 350,
        Targets = targets.Length > 0
            ? targets.ToList()
            : new List<ServiceTargetRef> { new() { BikeId = 3 }, new() { ComponentId = 7 } }
    };

    private SaveServiceRecordCommandHandler SaveHandler() => new(_komUoW, _mediator);

    [Fact]
    public async Task Create_adds_the_service_with_its_targets_and_announces_it()
    {
        var res = await SaveHandler().Handle(Save(), CancellationToken.None);

        res.Should().BeSuccess();
        _serviceRepo.Received().AddServiceRecord(Arg.Is<ServiceRecordEntity>(s =>
            s.UserId == "u1" && s.Cost == 350 && s.Date.Kind == DateTimeKind.Utc && s.Date.Hour == 9
            && s.Targets.Count == 2 && s.Targets.Any(t => t.BikeId == 3) && s.Targets.Any(t => t.ComponentId == 7)));
        await _mediator.Received().Publish(Arg.Any<ServiceRecordChangedNotification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_target_the_user_does_not_own_is_rejected()
    {
        _componentRepo.GetByIdsAsync(Arg.Any<IReadOnlyCollection<int>>())
            .Returns(new[] { new ComponentEntity { Id = 7, UserId = "other", Name = "x" } });

        var res = await SaveHandler().Handle(Save(), CancellationToken.None);

        res.Should().BeFailure().Which.HasError<NotFoundError>().Should().BeTrue();
        _serviceRepo.DidNotReceive().AddServiceRecord(Arg.Any<ServiceRecordEntity>());
    }

    [Fact]
    public async Task Update_of_another_users_service_is_forbidden()
    {
        _serviceRepo.GetServiceRecordAsync(4).Returns(new ServiceRecordEntity { Id = 4, UserId = "other", Name = "x" });

        var res = await SaveHandler().Handle(Save(4), CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ForbiddenError>().Should().BeTrue();
    }

    [Fact]
    public async Task Update_of_a_missing_service_is_not_found()
    {
        var res = await SaveHandler().Handle(Save(4), CancellationToken.None);

        res.Should().BeFailure().Which.HasError<NotFoundError>().Should().BeTrue();
    }

    [Fact]
    public async Task Update_replaces_the_target_set_keeping_the_rows_that_stay()
    {
        var kept = new ServiceTargetEntity { Id = 10, BikeId = 3, DistanceKm = 1234 };
        var existing = new ServiceRecordEntity
        {
            Id = 4, UserId = "u1", Name = "x",
            Targets = new List<ServiceTargetEntity> { kept, new() { Id = 11, ComponentId = 99 } }
        };
        _serviceRepo.GetServiceRecordAsync(4).Returns(existing);

        var res = await SaveHandler().Handle(Save(4, new ServiceTargetRef { BikeId = 3 }, new ServiceTargetRef { ComponentId = 7 }), CancellationToken.None);

        res.Should().BeSuccess();
        existing.Targets.Should().HaveCount(2);
        existing.Targets.Should().Contain(kept);
        existing.Targets.Should().Contain(t => t.ComponentId == 7);
        existing.Targets.Should().NotContain(t => t.ComponentId == 99);
    }

    [Fact]
    public async Task Delete_of_another_users_service_is_forbidden()
    {
        _serviceRepo.GetServiceRecordAsync(4).Returns(new ServiceRecordEntity { Id = 4, UserId = "other", Name = "x" });

        var res = await new DeleteServiceRecordCommandHandler(_komUoW)
            .Handle(new DeleteServiceRecordCommand { Id = 4, UserId = "u1" }, CancellationToken.None);

        res.Should().BeFailure().Which.HasError<ForbiddenError>().Should().BeTrue();
        _serviceRepo.DidNotReceive().DeleteServiceRecord(Arg.Any<ServiceRecordEntity>());
    }

    [Fact]
    public async Task Delete_removes_the_service()
    {
        var service = new ServiceRecordEntity { Id = 4, UserId = "u1", Name = "x" };
        _serviceRepo.GetServiceRecordAsync(4).Returns(service);

        var res = await new DeleteServiceRecordCommandHandler(_komUoW)
            .Handle(new DeleteServiceRecordCommand { Id = 4, UserId = "u1" }, CancellationToken.None);

        res.Should().BeSuccess();
        _serviceRepo.Received().DeleteServiceRecord(service);
    }

    [Fact]
    public async Task Recalculate_stores_each_targets_mileage_at_the_service_date()
    {
        var date = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var service = new ServiceRecordEntity { Id = 4, UserId = "u1", Name = "x", Date = date };
        var bikeTarget = new ServiceTargetEntity { Id = 10, ServiceId = 4, BikeId = 3, Service = service };
        var componentTarget = new ServiceTargetEntity { Id = 11, ServiceId = 4, ComponentId = 7, Service = service };
        _serviceRepo.GetTargetsForRecalculationAsync(
                Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<IReadOnlyCollection<int>>())
            .Returns(new[] { bikeTarget, componentTarget });

        var installationRepo = Substitute.For<IInstallationRepository>();
        var activityRepo = Substitute.For<IActivityRepository>();
        var linkRepo = Substitute.For<IBikeLinkRepository>();
        _komUoW.GetRepository<IInstallationRepository>().Returns(installationRepo);
        _komUoW.GetRepository<IActivityRepository>().Returns(activityRepo);
        _komUoW.GetRepository<IBikeLinkRepository>().Returns(linkRepo);
        _bikeRepo.GetByIdsAsync(Arg.Any<IReadOnlyCollection<int>>())
            .Returns(new[] { new BikeEntity { Id = 3, UserId = "u1", Name = "Trutka", InitialDistanceKm = 500 } });
        _componentRepo.GetByIdsAsync(Arg.Any<IReadOnlyCollection<int>>())
            .Returns(new[] { new ComponentEntity { Id = 7, UserId = "u1", Name = "Amortyzator", InitialDistanceKm = 42 } });

        var handler = new RecalculateServiceMileageCommandHandler(_komUoW, new ComponentMileageService(_komUoW));
        var res = await handler.Handle(new RecalculateServiceMileageCommand { ServiceIds = new[] { 4 } }, CancellationToken.None);

        res.Should().BeSuccess();
        bikeTarget.DistanceKm.Should().Be(500m);
        componentTarget.DistanceKm.Should().Be(42m);
        componentTarget.ComputedAt.Should().NotBeNull();
        await _komUoW.Received().SaveChangesAsync();
    }
}

public class ServiceRecordValidatorTests
{
    private readonly SaveServiceRecordCommandValidator _validator = new();

    private static SaveServiceRecordCommand Valid() => new()
    {
        UserId = "u1",
        Name = "Serwis",
        Date = new DateTime(2026, 5, 1),
        Targets = new List<ServiceTargetRef> { new() { BikeId = 3 } }
    };

    [Fact]
    public void A_valid_service_passes() => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Missing_name_date_and_targets_fail()
    {
        var cmd = Valid();
        cmd.Name = "";
        cmd.Date = default;
        cmd.Targets = new List<ServiceTargetRef>();

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Date);
        result.ShouldHaveValidationErrorFor(x => x.Targets);
    }

    [Fact]
    public void A_target_must_be_a_bike_xor_a_component()
    {
        var cmd = Valid();
        cmd.Targets = new List<ServiceTargetRef> { new() { BikeId = 3, ComponentId = 7 }, new() };

        _validator.TestValidate(cmd).IsValid.Should().BeFalse();
    }

    [Fact]
    public void The_same_target_twice_fails()
    {
        var cmd = Valid();
        cmd.Targets = new List<ServiceTargetRef> { new() { ComponentId = 7 }, new() { ComponentId = 7 } };

        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Targets);
    }

    [Fact]
    public void Negative_cost_fails()
    {
        var cmd = Valid();
        cmd.Cost = -1;

        _validator.TestValidate(cmd).ShouldHaveValidationErrorFor(x => x.Cost);
    }
}

/// <summary>Every field the WEB posts must exist on the command with a matching type (the controller maps them one by one).</summary>
public class ServiceRecordContractParityTests
{
    [Fact]
    public void Command_covers_every_view_model_field_with_matching_type()
    {
        var commandProps = typeof(SaveServiceRecordCommand).GetProperties().ToDictionary(p => p.Name, p => p.PropertyType);

        foreach (var inputProp in typeof(SaveServiceRecordViewModel).GetProperties())
        {
            commandProps.Should().ContainKey(inputProp.Name);
            commandProps[inputProp.Name].Should().Be(inputProp.PropertyType, "'{0}' must have the same type", inputProp.Name);
        }
    }
}

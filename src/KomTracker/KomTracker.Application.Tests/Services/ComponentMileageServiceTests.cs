#nullable enable
using FluentAssertions;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Strava;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Bike;
using KomTracker.Domain.Entities.Component;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Services;

public class ComponentMileageServiceTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IActivityRepository _activityRepo;
    private readonly IBikeLinkRepository _bikeLinkRepo;
    private readonly IInstallationRepository _installationRepo;
    private readonly IComponentRepository _componentRepo;
    private readonly ComponentMileageService _service;

    public ComponentMileageServiceTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _activityRepo = Substitute.For<IActivityRepository>();
        _bikeLinkRepo = Substitute.For<IBikeLinkRepository>();
        _installationRepo = Substitute.For<IInstallationRepository>();
        _componentRepo = Substitute.For<IComponentRepository>();
        _komUoW.GetRepository<IActivityRepository>().Returns(_activityRepo);
        _komUoW.GetRepository<IBikeLinkRepository>().Returns(_bikeLinkRepo);
        _komUoW.GetRepository<IInstallationRepository>().Returns(_installationRepo);
        _komUoW.GetRepository<IComponentRepository>().Returns(_componentRepo);
        _service = new ComponentMileageService(_komUoW);

        // One stub returns the links for whichever bike ids the service asks about (it batches them into one call).
        _bikeLinkRepo.GetByBikeIdsAsync(Arg.Any<IReadOnlyCollection<int>>())
            .Returns(ci => _links.Where(l => ((IReadOnlyCollection<int>)ci[0]).Contains(l.BikeId)).ToList());
    }

    private readonly List<BikeLinkEntity> _links = new();

    private void GearForBike(int bikeId, string gearId)
        => _links.Add(new BikeLinkEntity { BikeId = bikeId, ExternalService = ExternalService.Strava, ExternalId = gearId });

    private void Activities(params ActivityAttributionModel[] activities)
        => _activityRepo.GetActivitiesByGearAsync(Arg.Any<IReadOnlyCollection<string>>()).Returns(activities);

    private static ActivityAttributionModel Ride(string gearId, DateTime start, double m, int s, double elev)
        => new() { GearId = gearId, StartDate = start, DistanceMeters = m, MovingTimeSeconds = s, ElevationMeters = elev };

    private static InstallationEntity OnBike(int bikeId, DateTime from, DateTime? to, int componentId = 5)
        => new() { ComponentId = componentId, BikeId = bikeId, Type = ComponentInstallationType.Tracked, DateFrom = from, DateTo = to };

    private void Component(int id, decimal seedKm = 0, decimal? seedHours = null, decimal? seedElev = null)
        => _componentRepo.GetByIdsAsync(Arg.Is<IReadOnlyCollection<int>>(c => c.Contains(id)))
            .Returns(new[] { new ComponentEntity { Id = id, UserId = "u1", Name = "Chain", Category = ComponentCategory.Chain, InitialDistanceKm = seedKm, InitialMovingHours = seedHours, InitialElevationM = seedElev } });

    private void InstallsFor(int componentId, params InstallationEntity[] installs)
        => _installationRepo.GetByComponentsAsync(Arg.Is<IReadOnlyCollection<int>>(c => c.Contains(componentId))).Returns(installs);

    [Fact]
    public async Task Direct_on_bike_window_sums_and_converts()
    {
        Component(5);
        InstallsFor(5, OnBike(3, new DateTime(2026, 1, 1), new DateTime(2026, 2, 1)));
        GearForBike(3, "b1");
        Activities(Ride("b1", new DateTime(2026, 1, 15), 10000, 3600, 250));

        var totals = (await _service.ComputeTotalsAsync(new[] { 5 }))[5];

        totals.DistanceKm.Should().Be(10m);
        totals.MovingHours.Should().Be(1m);
        totals.ElevationM.Should().Be(250m);
        totals.ActivityCount.Should().Be(1);
    }

    [Fact]
    public async Task Activity_outside_the_window_is_excluded()
    {
        Component(5);
        InstallsFor(5, OnBike(3, new DateTime(2026, 1, 1), new DateTime(2026, 2, 1)));
        GearForBike(3, "b1");
        Activities(Ride("b1", new DateTime(2026, 3, 1), 10000, 3600, 250));   // after DateTo

        var totals = (await _service.ComputeTotalsAsync(new[] { 5 }))[5];

        totals.DistanceKm.Should().Be(0m);
        totals.ActivityCount.Should().Be(0);
    }

    [Fact]
    public async Task Open_window_includes_rides_after_date_from()
    {
        Component(5);
        InstallsFor(5, OnBike(3, new DateTime(2026, 1, 1), null));   // still installed
        GearForBike(3, "b1");
        Activities(Ride("b1", new DateTime(2026, 6, 1), 5000, 1800, 100));

        var totals = (await _service.ComputeTotalsAsync(new[] { 5 }))[5];

        totals.DistanceKm.Should().Be(5m);
        totals.ActivityCount.Should().Be(1);
    }

    [Fact]
    public async Task Seed_and_manual_add_without_touching_activities()
    {
        Component(5, seedKm: 100, seedHours: 5, seedElev: 300);
        InstallsFor(5, new InstallationEntity { ComponentId = 5, BikeId = 3, Type = ComponentInstallationType.Manual, ManualDistanceKm = 50, ManualMovingHours = 2, ManualElevationM = 400 });

        var totals = (await _service.ComputeTotalsAsync(new[] { 5 }))[5];

        totals.DistanceKm.Should().Be(150m);
        totals.MovingHours.Should().Be(7m);
        totals.ElevationM.Should().Be(700m);
        totals.ActivityCount.Should().Be(0);
    }

    [Fact]
    public async Task Multi_bike_windows_are_summed()
    {
        Component(5);
        InstallsFor(5,
            OnBike(3, new DateTime(2026, 1, 1), null),
            OnBike(4, new DateTime(2026, 1, 1), null));
        GearForBike(3, "b1");
        GearForBike(4, "b2");
        Activities(
            Ride("b1", new DateTime(2026, 2, 1), 10000, 3600, 100),
            Ride("b2", new DateTime(2026, 2, 1), 5000, 1800, 50));

        var totals = (await _service.ComputeTotalsAsync(new[] { 5 }))[5];

        totals.DistanceKm.Should().Be(15m);
        totals.ActivityCount.Should().Be(2);
    }

    [Fact]
    public async Task In_component_uses_overlap_of_child_and_parent_windows()
    {
        // Tyre 5 in wheel 8 over [Feb, Apr); wheel 8 on bike 3 over [Jan, Mar). Overlap = [Feb, Mar).
        GearForBike(3, "b1");
        _installationRepo.GetByComponentsAsync(Arg.Is<IReadOnlyCollection<int>>(c => c.Contains(8))).Returns(new[]
        {
            new InstallationEntity { ComponentId = 8, BikeId = 3, Type = ComponentInstallationType.Tracked, DateFrom = new DateTime(2026, 1, 1), DateTo = new DateTime(2026, 3, 1) }
        });
        Activities(
            Ride("b1", new DateTime(2026, 2, 15), 2000, 720, 30),   // inside overlap → counts
            Ride("b1", new DateTime(2026, 3, 15), 9999, 9999, 9999)); // after overlap (Mar) → excluded

        var child = new InstallationEntity
        {
            ComponentId = 5, ParentComponentId = 8, Type = ComponentInstallationType.Tracked,
            DateFrom = new DateTime(2026, 2, 1), DateTo = new DateTime(2026, 4, 1)
        };

        await _service.ResolveWindowTotalsAsync(new[] { child });

        child.WindowDistanceKm.Should().Be(2m);
        child.WindowActivityCount.Should().Be(1);
    }
}

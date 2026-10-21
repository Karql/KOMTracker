#nullable enable
using FluentAssertions;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Strava;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Bike;
using KomTracker.Domain.Entities.Component;
using KomTracker.Domain.Entities.CostAnalysis;
using KomTracker.Domain.Entities.Expense;
using KomTracker.Domain.Entities.ServiceRecord;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Services;

public class CostAnalysisServiceTests
{
    private const string User = "u1";

    private readonly IKOMUnitOfWork _komUoW = Substitute.For<IKOMUnitOfWork>();
    private readonly List<BikeEntity> _bikes = new();
    private readonly List<ComponentEntity> _components = new();
    private readonly List<InstallationEntity> _installations = new();
    private readonly List<ServiceRecordEntity> _services = new();
    private readonly List<ExpenseEntity> _expenses = new();
    private readonly List<BikeLinkEntity> _links = new();
    private readonly List<ActivityAttributionModel> _activities = new();
    private readonly CostAnalysisService _service;

    public CostAnalysisServiceTests()
    {
        var bikeRepo = Substitute.For<IBikeRepository>();
        var componentRepo = Substitute.For<IComponentRepository>();
        var installationRepo = Substitute.For<IInstallationRepository>();
        var serviceRepo = Substitute.For<IServiceRecordRepository>();
        var expenseRepo = Substitute.For<IExpenseRepository>();
        var linkRepo = Substitute.For<IBikeLinkRepository>();
        var activityRepo = Substitute.For<IActivityRepository>();

        bikeRepo.GetBikesAsync(User, true).Returns(_ => _bikes);
        componentRepo.GetComponentsAsync(User, true).Returns(_ => _components);
        componentRepo.GetByIdsAsync(Arg.Any<IReadOnlyCollection<int>>())
            .Returns(ci => _components.Where(c => ((IReadOnlyCollection<int>)ci[0]).Contains(c.Id)).ToList());
        installationRepo.GetByComponentsAsync(Arg.Any<IReadOnlyCollection<int>>())
            .Returns(ci => _installations.Where(i => ((IReadOnlyCollection<int>)ci[0]).Contains(i.ComponentId)).ToList());
        serviceRepo.GetServiceRecordsAsync(User).Returns(_ => _services);
        expenseRepo.GetExpensesAsync(User).Returns(_ => _expenses);
        linkRepo.GetByBikeIdsAsync(Arg.Any<IReadOnlyCollection<int>>())
            .Returns(ci => _links.Where(l => ((IReadOnlyCollection<int>)ci[0]).Contains(l.BikeId)).ToList());
        activityRepo.GetActivitiesByGearAsync(Arg.Any<IReadOnlyCollection<string>>()).Returns(_ => _activities);

        _komUoW.GetRepository<IBikeRepository>().Returns(bikeRepo);
        _komUoW.GetRepository<IComponentRepository>().Returns(componentRepo);
        _komUoW.GetRepository<IInstallationRepository>().Returns(installationRepo);
        _komUoW.GetRepository<IServiceRecordRepository>().Returns(serviceRepo);
        _komUoW.GetRepository<IExpenseRepository>().Returns(expenseRepo);
        _komUoW.GetRepository<IBikeLinkRepository>().Returns(linkRepo);
        _komUoW.GetRepository<IActivityRepository>().Returns(activityRepo);

        _service = new CostAnalysisService(_komUoW, new ComponentMileageService(_komUoW));
    }

    private void Bike(int id, string gear, decimal? price = null, DateTime? bought = null, decimal? salePrice = null, DateTime? sold = null)
    {
        _bikes.Add(new BikeEntity { Id = id, UserId = User, Name = $"Bike{id}", Price = price, PurchaseDate = bought, SalePrice = salePrice, SaleDate = sold });
        _links.Add(new BikeLinkEntity { BikeId = id, ExternalService = ExternalService.Strava, ExternalId = gear });
    }

    private void Ride(string gear, DateTime when, double km)
        => _activities.Add(new ActivityAttributionModel { GearId = gear, StartDate = when, DistanceMeters = km * 1000 });

    private ComponentEntity Part(int id, decimal? price, DateTime? bought = null, decimal? salePrice = null, DateTime? sold = null)
    {
        var c = new ComponentEntity
        {
            Id = id, UserId = User, Name = $"Part{id}", Category = ComponentCategory.Wheel, Price = price,
            PurchaseDate = bought, SalePrice = salePrice, SaleDate = sold
        };
        _components.Add(c);
        return c;
    }

    private void OnBike(int componentId, int bikeId, DateTime from, DateTime? to)
        => _installations.Add(new InstallationEntity { ComponentId = componentId, BikeId = bikeId, Type = ComponentInstallationType.Tracked, DateFrom = from, DateTo = to });

    private static readonly DateTime Jan = new(2025, 1, 1);
    private static readonly DateTime Jun = new(2025, 6, 1);
    private static readonly DateTime Dec = new(2025, 12, 1);

    [Fact]
    public async Task A_part_used_on_two_bikes_is_split_by_its_km_on_each_and_so_is_its_resale()
    {
        Bike(3, "b3");
        Bike(4, "b4");
        Part(10, 1000m, Jan, salePrice: 400m, sold: Dec);
        OnBike(10, 3, Jan, Jun);
        OnBike(10, 4, Jun, Dec);
        Ride("b3", new DateTime(2025, 3, 1), 3000);
        Ride("b4", new DateTime(2025, 8, 1), 1000);

        var report = await _service.BuildAsync(User);

        var costs = report.Lines.Where(l => l.Source == CostSource.Components && l.Kind == CostKind.Cost).ToList();
        costs.Should().HaveCount(2);
        costs.Single(l => l.BikeId == 3).Amount.Should().Be(750m);
        costs.Single(l => l.BikeId == 4).Amount.Should().Be(250m);
        costs.Single(l => l.BikeId == 3).Note.Should().Contain("of");

        var resale = report.Lines.Where(l => l.Kind == CostKind.Resale).ToList();
        resale.Sum(l => l.Amount).Should().Be(-400m);
        resale.Single(l => l.BikeId == 3).Amount.Should().Be(-300m);
        resale.Should().OnlyContain(l => l.Date == Dec);
    }

    [Fact]
    public async Task Installed_but_never_ridden_splits_equally_and_never_installed_has_no_bike()
    {
        Bike(3, "b3");
        Bike(4, "b4");
        Part(10, 100m, Jan);
        OnBike(10, 3, Jan, Jun);
        OnBike(10, 4, Jun, null);
        Part(11, 50m, Jan);

        var report = await _service.BuildAsync(User);

        report.Lines.Where(l => l.ItemId == 10).Select(l => l.Amount).Should().Equal(50m, 50m);
        var unmounted = report.Lines.Single(l => l.ItemId == 11);
        unmounted.BikeId.Should().BeNull();
        unmounted.Amount.Should().Be(50m);
    }

    [Fact]
    public async Task An_undated_purchase_takes_the_first_install_date_and_unknown_prices_make_no_line()
    {
        Bike(3, "b3");
        Part(10, 100m);
        OnBike(10, 3, Jun, null);
        Part(11, null, Jan);

        var report = await _service.BuildAsync(User);

        report.Lines.Should().ContainSingle(l => l.Source == CostSource.Components);
        report.Lines.Single().Date.Should().Be(Jun);
    }

    [Fact]
    public async Task A_service_is_split_across_targets_and_a_component_goes_to_the_bike_it_was_on_that_day()
    {
        Bike(3, "b3");
        Bike(4, "b4");
        Part(10, null);
        OnBike(10, 4, Jan, null);
        _services.Add(new ServiceRecordEntity
        {
            Id = 7, UserId = User, Name = "Check-up", Date = Jun, Cost = 200m,
            Targets = new List<ServiceTargetEntity> { new() { BikeId = 3 }, new() { ComponentId = 10 } }
        });

        var report = await _service.BuildAsync(User);

        var lines = report.Lines.Where(l => l.Source == CostSource.Services).ToList();
        lines.Should().HaveCount(2);
        lines.Single(l => l.BikeId == 3).Amount.Should().Be(100m);
        lines.Single(l => l.BikeId == 4).Amount.Should().Be(100m);
        lines.Should().OnlyContain(l => l.FullAmount == 200m && l.Share == 0.5m);
    }

    [Fact]
    public async Task A_part_in_a_wheel_is_on_the_wheels_bike_at_the_service_date()
    {
        Bike(3, "b3");
        Part(20, null); // wheel
        Part(21, null); // tyre in the wheel
        OnBike(20, 3, Jan, null);
        _installations.Add(new InstallationEntity { ComponentId = 21, ParentComponentId = 20, Type = ComponentInstallationType.Tracked, DateFrom = Jan });
        _services.Add(new ServiceRecordEntity
        {
            Id = 7, UserId = User, Name = "Tyre fix", Date = Jun, Cost = 30m,
            Targets = new List<ServiceTargetEntity> { new() { ComponentId = 21 } }
        });

        var report = await _service.BuildAsync(User);

        report.Lines.Single(l => l.Source == CostSource.Services).BikeId.Should().Be(3);
    }

    [Fact]
    public async Task Bikes_and_expenses_yield_purchase_and_negative_sale_lines()
    {
        Bike(3, "b3", price: 5000m, bought: Jan, salePrice: 3000m, sold: Dec);
        _expenses.Add(new ExpenseEntity { Id = 5, UserId = User, Name = "Shoes", Category = ExpenseCategory.Shoes, Price = 400m, PurchaseDate = Jan, SalePrice = 150m, SaleDate = Dec });

        var report = await _service.BuildAsync(User);

        report.Lines.Where(l => l.Source == CostSource.Bikes).Select(l => l.Amount).Should().BeEquivalentTo(new[] { 5000m, -3000m });
        report.Lines.Where(l => l.Source == CostSource.Bikes).Should().OnlyContain(l => l.Group == "Bikes" && l.Category == "Bikes");   // no type categories
        var expense = report.Lines.Where(l => l.Source == CostSource.Expenses).ToList();
        expense.Should().OnlyContain(l => l.BikeId == null);
        expense.Sum(l => l.Amount).Should().Be(250m);
    }

    [Fact]
    public async Task Bike_km_are_bucketed_per_year_with_the_seed_kept_apart()
    {
        Bike(3, "b3");
        _bikes[0].InitialDistanceKm = 500m;
        Ride("b3", new DateTime(2024, 5, 1), 100);
        Ride("b3", new DateTime(2025, 5, 1), 40);
        Ride("b3", new DateTime(2025, 6, 1), 60);

        var report = await _service.BuildAsync(User);

        var bike = report.Bikes.Single();
        bike.SeedKm.Should().Be(500m);
        bike.KmByYear.Should().BeEquivalentTo(new Dictionary<int, decimal> { [2024] = 100m, [2025] = 100m });
    }
}

#nullable enable
using FluentAssertions;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Queries.Component;
using NSubstitute;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Queries.Component;

public class GetPurchaseSuggestionsQueryTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IComponentRepository _componentRepo;
    private readonly IBikeRepository _bikeRepo;
    private readonly IExpenseRepository _expenseRepo;
    private readonly IServiceRecordRepository _serviceRepo;
    private readonly GetPurchaseSuggestionsQueryHandler _handler;

    public GetPurchaseSuggestionsQueryTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _componentRepo = Substitute.For<IComponentRepository>();
        _bikeRepo = Substitute.For<IBikeRepository>();
        _expenseRepo = Substitute.For<IExpenseRepository>();
        _serviceRepo = Substitute.For<IServiceRecordRepository>();
        _komUoW.GetRepository<IServiceRecordRepository>().Returns(_serviceRepo);
        _serviceRepo.GetDistinctPlacesAsync(Arg.Any<string>()).Returns((IReadOnlyList<string>)new string[0]);
        _komUoW.GetRepository<IComponentRepository>().Returns(_componentRepo);
        _komUoW.GetRepository<IBikeRepository>().Returns(_bikeRepo);
        _komUoW.GetRepository<IExpenseRepository>().Returns(_expenseRepo);
        _expenseRepo.GetDistinctPlacesAsync(Arg.Any<string>()).Returns((
            (IReadOnlyList<string>)new string[0],
            (IReadOnlyList<string>)new string[0]));
        _handler = new GetPurchaseSuggestionsQueryHandler(_komUoW);
    }

    [Fact]
    public async Task Unions_distinct_trimmed_case_insensitive_sorted_dropping_blanks()
    {
        // What the DB DISTINCT hands back per source (already non-empty, distinct within a source).
        _componentRepo.GetDistinctPurchaseFieldsAsync("u1").Returns((
            (IReadOnlyList<string>)new[] { "Shimano" },
            (IReadOnlyList<string>)new[] { "XT" },
            (IReadOnlyList<string>)new[] { "Decathlon" },
            (IReadOnlyList<string>)new[] { "OLX" }));
        _bikeRepo.GetDistinctPurchaseFieldsAsync("u1").Returns((
            (IReadOnlyList<string>)new[] { "Canyon", " shimano " },   // trims + dedupes with "Shimano" case-insensitively
            (IReadOnlyList<string>)new[] { "Ultimate" },
            (IReadOnlyList<string>)new[] { "decathlon" },             // dedupes with "Decathlon" case-insensitively
            (IReadOnlyList<string>)new[] { "olx ", "Allegro" }));     // trims + dedupes with "OLX" case-insensitively

        var res = await _handler.Handle(new GetPurchaseSuggestionsQuery { UserId = "u1" }, CancellationToken.None);

        // Cross-source merge: trimmed, distinct (case-insensitive), ordered; first-seen casing wins on a dedupe.
        res.Brands.Should().Equal("Canyon", "Shimano");
        res.Models.Should().Equal("Ultimate", "XT");
        res.PurchasePlaces.Should().Equal("Decathlon");
        res.SalePlaces.Should().Equal("Allegro", "OLX");
    }

    [Fact]
    public async Task Expense_places_join_the_purchase_and_sale_places()
    {
        var empty = (IReadOnlyList<string>)new string[0];
        _componentRepo.GetDistinctPurchaseFieldsAsync("u1").Returns((empty, empty, (IReadOnlyList<string>)new[] { "Biostern" }, empty));
        _bikeRepo.GetDistinctPurchaseFieldsAsync("u1").Returns((empty, empty, empty, empty));
        _expenseRepo.GetDistinctPlacesAsync("u1").Returns((
            (IReadOnlyList<string>)new[] { "Decathlon", "biostern" },   // dedupes with the component's "Biostern"
            (IReadOnlyList<string>)new[] { "Rafał" }));

        var res = await _handler.Handle(new GetPurchaseSuggestionsQuery { UserId = "u1" }, CancellationToken.None);

        res.PurchasePlaces.Should().Equal("Biostern", "Decathlon");
        res.SalePlaces.Should().Equal("Rafał");
        res.Brands.Should().BeEmpty();   // expenses don't feed brands/models
    }

    [Fact]
    public async Task Service_places_join_the_purchase_places()
    {
        var empty = (IReadOnlyList<string>)new string[0];
        _componentRepo.GetDistinctPurchaseFieldsAsync("u1").Returns((empty, empty, (IReadOnlyList<string>)new[] { "Biostern" }, empty));
        _bikeRepo.GetDistinctPurchaseFieldsAsync("u1").Returns((empty, empty, empty, empty));
        _serviceRepo.GetDistinctPlacesAsync("u1").Returns((IReadOnlyList<string>)new[] { "Bike RS", "BIOSTERN" });

        var res = await _handler.Handle(new GetPurchaseSuggestionsQuery { UserId = "u1" }, CancellationToken.None);

        res.PurchasePlaces.Should().Equal("Bike RS", "Biostern");
        res.SalePlaces.Should().BeEmpty();
    }
}

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
    private readonly GetPurchaseSuggestionsQueryHandler _handler;

    public GetPurchaseSuggestionsQueryTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _componentRepo = Substitute.For<IComponentRepository>();
        _bikeRepo = Substitute.For<IBikeRepository>();
        _komUoW.GetRepository<IComponentRepository>().Returns(_componentRepo);
        _komUoW.GetRepository<IBikeRepository>().Returns(_bikeRepo);
        _handler = new GetPurchaseSuggestionsQueryHandler(_komUoW);
    }

    [Fact]
    public async Task Unions_distinct_trimmed_case_insensitive_sorted_dropping_blanks()
    {
        // What the DB DISTINCT hands back per source (already non-empty, distinct within a source).
        _componentRepo.GetDistinctPurchaseFieldsAsync("u1").Returns((
            (IReadOnlyList<string>)new[] { "Shimano" },
            (IReadOnlyList<string>)new[] { "XT" },
            (IReadOnlyList<string>)new[] { "Decathlon" }));
        _bikeRepo.GetDistinctPurchaseFieldsAsync("u1").Returns((
            (IReadOnlyList<string>)new[] { "Canyon", " shimano " },   // trims + dedupes with "Shimano" case-insensitively
            (IReadOnlyList<string>)new[] { "Ultimate" },
            (IReadOnlyList<string>)new[] { "decathlon" }));           // dedupes with "Decathlon" case-insensitively

        var res = await _handler.Handle(new GetPurchaseSuggestionsQuery { UserId = "u1" }, CancellationToken.None);

        // Cross-source merge: trimmed, distinct (case-insensitive), ordered; first-seen casing wins on a dedupe.
        res.Brands.Should().Equal("Canyon", "Shimano");
        res.Models.Should().Equal("Ultimate", "XT");
        res.PurchasePlaces.Should().Equal("Decathlon");
    }
}

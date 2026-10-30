#nullable enable
using FluentAssertions;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Queries.SystemStats;
using KomTracker.Domain.Entities.SystemStats;
using NSubstitute;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Queries.SystemStats;

public class GetSystemStatsQueryTests
{
    [Fact]
    public async Task The_snapshot_is_taken_as_of_now_in_utc()
    {
        var repository = Substitute.For<ISystemStatsRepository>();
        var komUoW = Substitute.For<IKOMUnitOfWork>();
        komUoW.GetRepository<ISystemStatsRepository>().Returns(repository);
        var report = new SystemStatsReport();
        repository.GetReportAsync(Arg.Any<DateTime>()).Returns(report);

        var result = await new GetSystemStatsQueryHandler(komUoW).Handle(new GetSystemStatsQuery(), CancellationToken.None);

        result.Should().BeSameAs(report);
        await repository.Received(1).GetReportAsync(Arg.Is<DateTime>(d => d.Kind == DateTimeKind.Utc && (DateTime.UtcNow - d).TotalSeconds < 5));
    }

    [Fact]
    public void Users_by_month_count_joins_per_month_and_carry_the_earlier_ones_in_the_total()
    {
        var now = new DateTime(2026, 10, 29);
        var joined = new[]
        {
            new DateTime(2025, 1, 5),                            // before the 3-month window
            new DateTime(2026, 8, 1), new DateTime(2026, 8, 31), // first month
            new DateTime(2026, 10, 2)                            // current month
        };

        var months = SystemMonth.Build(joined, now, 3);

        months.Select(m => (m.Month, m.Joined, m.Total)).Should().Equal(
            (new DateTime(2026, 8, 1), 2, 3),
            (new DateTime(2026, 9, 1), 0, 3),
            (new DateTime(2026, 10, 1), 1, 4));
    }
}

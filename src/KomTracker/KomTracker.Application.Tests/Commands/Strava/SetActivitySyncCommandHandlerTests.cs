#nullable enable
using FluentResults.Extensions.FluentAssertions;
using FluentAssertions;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Strava;

public class SetActivitySyncCommandHandlerTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteSyncRepository _athleteSyncRepo;
    private readonly IActivitySyncHistoryRepository _historyRepo;
    private readonly SetActivitySyncCommandHandler _handler;

    public SetActivitySyncCommandHandlerTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _athleteSyncRepo = Substitute.For<IAthleteSyncRepository>();
        _historyRepo = Substitute.For<IActivitySyncHistoryRepository>();

        _komUoW.GetRepository<IAthleteSyncRepository>().Returns(_athleteSyncRepo);
        _komUoW.GetRepository<IActivitySyncHistoryRepository>().Returns(_historyRepo);

        _handler = new SetActivitySyncCommandHandler(_komUoW);
    }

    [Fact]
    public async Task First_enable_no_history_needs_backfill()
    {
        _historyRepo.AnyForAthleteAsync(1).Returns(false);

        var res = await _handler.Handle(new SetActivitySyncCommand { AthleteId = 1, Enabled = true }, CancellationToken.None);

        res.Should().BeSuccess();
        res.Value.BackfillNeeded.Should().BeTrue();
        await _athleteSyncRepo.Received().SetActivitiesEnabledAsync(1, true);
    }

    [Fact]
    public async Task Enable_with_existing_history_does_not_backfill()
    {
        _historyRepo.AnyForAthleteAsync(1).Returns(true);

        var res = await _handler.Handle(new SetActivitySyncCommand { AthleteId = 1, Enabled = true }, CancellationToken.None);

        res.Value.BackfillNeeded.Should().BeFalse();
        await _athleteSyncRepo.Received().SetActivitiesEnabledAsync(1, true);
    }

    [Fact]
    public async Task Disable_never_backfills()
    {
        var res = await _handler.Handle(new SetActivitySyncCommand { AthleteId = 1, Enabled = false }, CancellationToken.None);

        res.Value.BackfillNeeded.Should().BeFalse();
        await _athleteSyncRepo.Received().SetActivitiesEnabledAsync(1, false);
    }
}

#nullable enable
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Services;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Strava;

public class SyncStravaBikesCommandHandlerTests
{
    private readonly IStravaBikeSyncService _bikeSyncService;
    private readonly SyncStravaBikesCommandHandler _handler;

    public SyncStravaBikesCommandHandlerTests()
    {
        _bikeSyncService = Substitute.For<IStravaBikeSyncService>();
        _handler = new SyncStravaBikesCommandHandler(_bikeSyncService);
    }

    [Fact]
    public async Task Delegates_to_the_bike_sync_service()
    {
        _bikeSyncService.SyncAthleteBikesAsync(1, Arg.Any<CancellationToken>()).Returns(Result.Ok());

        var res = await _handler.Handle(new SyncStravaBikesCommand { AthleteId = 1 }, CancellationToken.None);

        res.Should().BeSuccess();
        await _bikeSyncService.Received().SyncAthleteBikesAsync(1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Surfaces_the_service_failure()
    {
        _bikeSyncService.SyncAthleteBikesAsync(1, Arg.Any<CancellationToken>()).Returns(Result.Fail("boom"));

        var res = await _handler.Handle(new SyncStravaBikesCommand { AthleteId = 1 }, CancellationToken.None);

        res.Should().BeFailure();
    }
}

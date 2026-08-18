#nullable enable
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Strava;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Strava;

public class SetBikeSyncCommandHandlerTests
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteSyncRepository _athleteSyncRepo;
    private readonly SetBikeSyncCommandHandler _handler;

    public SetBikeSyncCommandHandlerTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _athleteSyncRepo = Substitute.For<IAthleteSyncRepository>();
        _komUoW.GetRepository<IAthleteSyncRepository>().Returns(_athleteSyncRepo);
        _handler = new SetBikeSyncCommandHandler(_komUoW);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Sets_bikes_enabled(bool enabled)
    {
        var res = await _handler.Handle(new SetBikeSyncCommand { AthleteId = 1, Enabled = enabled }, CancellationToken.None);

        res.Should().BeSuccess();
        await _athleteSyncRepo.Received().SetBikesEnabledAsync(1, enabled);
    }
}

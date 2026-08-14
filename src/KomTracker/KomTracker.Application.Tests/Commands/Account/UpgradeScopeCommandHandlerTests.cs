#nullable enable
using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Commands.Account;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Token;
using NSubstitute;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using IStravaTokenService = KomTracker.Application.Interfaces.Services.Strava.ITokenService;

namespace KomTracker.Application.Tests.Commands.Account;

public class UpgradeScopeCommandHandlerTests
{
    private const string Code = "code1";

    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteService _athleteService;
    private readonly IStravaTokenService _tokenService;
    private readonly UpgradeScopeCommandHandler _handler;

    private readonly TokenEntity _token = new() { AthleteId = 1, AccessToken = "t" };
    private readonly AthleteEntity _athlete = new() { AthleteId = 1 };

    public UpgradeScopeCommandHandlerTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _athleteService = Substitute.For<IAthleteService>();
        _tokenService = Substitute.For<IStravaTokenService>();

        _handler = new UpgradeScopeCommandHandler(_komUoW, _athleteService, _tokenService);
    }

    [Fact]
    public async Task Stores_token_and_reports_read_all_when_granted()
    {
        const string scope = "read,activity:read_all,profile:read_all";
        _tokenService.ExchangeAsync(Code, scope).Returns(Result.Ok((_athlete, _token)));

        var res = await _handler.Handle(new UpgradeScopeCommand(Code, scope), CancellationToken.None);

        res.Should().BeSuccess();
        res.Value.HasActivityReadAll.Should().BeTrue();
        await _athleteService.Received().AddOrUpdateTokenAsync(_token);
        await _komUoW.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task Stores_token_but_reports_not_read_all_when_declined()
    {
        const string scope = "read,activity:read,profile:read_all";
        _tokenService.ExchangeAsync(Code, scope).Returns(Result.Ok((_athlete, _token)));

        var res = await _handler.Handle(new UpgradeScopeCommand(Code, scope), CancellationToken.None);

        res.Should().BeSuccess();
        res.Value.HasActivityReadAll.Should().BeFalse();
        await _athleteService.Received().AddOrUpdateTokenAsync(_token);
    }

    [Fact]
    public async Task Fails_and_does_not_store_when_exchange_fails()
    {
        const string scope = "read,activity:read_all,profile:read_all";
        _tokenService.ExchangeAsync(Code, scope).Returns(Result.Fail<(AthleteEntity, TokenEntity)>("bad code"));

        var res = await _handler.Handle(new UpgradeScopeCommand(Code, scope), CancellationToken.None);

        res.Should().BeFailure();
        await _athleteService.DidNotReceiveWithAnyArgs().AddOrUpdateTokenAsync(null!);
        await _komUoW.DidNotReceiveWithAnyArgs().SaveChangesAsync();
    }
}

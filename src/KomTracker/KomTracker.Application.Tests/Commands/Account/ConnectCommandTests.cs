using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Errors.Account;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Token;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Services;
using IStravaTokenService = KomTracker.Application.Interfaces.Services.Strava.ITokenService;
using IIdentityUserService = KomTracker.Application.Interfaces.Services.Identity.IUserService;
using KomTracker.Application.Errors.Strava.Token;
using KomTracker.Application.Commands.Account;
using System.Threading;

namespace KomTracker.Application.Tests.Commands.Account;

public class ConnectCommandTests
{
    #region TestData
    private const string TEST_INVALID_CODE = "invalid";
    private const string TEST_EXISTING_ATHLETE_CODE = "exist";
    private const string TEST_NEW_ATHLETE_CODE = "new";
    private const string TEST_INVALID_SCOPE = "read";
    private const string TEST_VALID_SCOPE = "read,activity:read,profile:read_all";

    private AthleteEntity TestExistingAthlete = new AthleteEntity
    {
        AthleteId = 1,
        Username = "Athlete1"
    };
    private AthleteEntity TestNewAthlete = new AthleteEntity
    {
        AthleteId = 2,
        Username = "Athlete2"

    };
    private TokenEntity TestToken = new TokenEntity();
    #endregion

    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteService _athleteService;
    private readonly IStravaTokenService _stravaTokenService;
    private readonly IIdentityUserService _userService;

    private readonly CancellationToken _cancellationToken;
    private readonly ConnectCommandHandler _connectCommandHandler;

    public ConnectCommandTests()
    {
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _athleteService = Substitute.For<IAthleteService>();
        _stravaTokenService = Substitute.For<IStravaTokenService>();
        _userService = Substitute.For<IIdentityUserService>();
        _cancellationToken = new CancellationTokenSource().Token;

        _connectCommandHandler = new ConnectCommandHandler(_komUoW, _athleteService, _stravaTokenService, _userService);

        PrepareMocks();
    }

    #region Connect
    [Fact]
    public async Task Connect_checks_is_scope_contains_required()
    {
        // Act
        var res = await _connectCommandHandler.Handle(new ConnectCommand(TEST_INVALID_CODE, TEST_INVALID_SCOPE), _cancellationToken);

        // Assert
        res.Should().BeFailure();
        res.HasError<ConnectError>(x => x.Message == ConnectError.NoRequiredScope).Should().BeTrue();

        await AssertConnectNoUpdate();
    }

    [Fact]
    public async Task Connect_fails_when_invalid_code()
    {
        // Act
        var res = await _connectCommandHandler.Handle(new ConnectCommand(TEST_INVALID_CODE, TEST_VALID_SCOPE), _cancellationToken);

        // Assert
        res.Should().BeFailure();
        res.HasError<ConnectError>(x => x.Message == ConnectError.InvalidCode).Should().BeTrue();

        await AssertConnectNoUpdate();
    }

    [Fact]
    public async Task Connect_adds_new_athlete()
    {
        // Act
        var res = await _connectCommandHandler.Handle(new ConnectCommand(TEST_NEW_ATHLETE_CODE, TEST_VALID_SCOPE), _cancellationToken);

        // Assert
        await _athleteService.Received().AddOrUpdateAthleteAsync(TestNewAthlete);
        await _athleteService.Received().AddOrUpdateTokenAsync(TestToken);
        await _userService.Received().AddUserAsync(Arg.Is<AthleteEntity>(x =>
            x.AthleteId == TestNewAthlete.AthleteId
            && x.Username == TestNewAthlete.Username));

        await _komUoW.Received().SaveChangesAsync();

        res.Should().BeSuccess();
    }

    [Fact]
    public async Task Connect_updates_exsisting_athlete()
    {
        // Act
        var res = await _connectCommandHandler.Handle(new ConnectCommand(TEST_EXISTING_ATHLETE_CODE, TEST_VALID_SCOPE), _cancellationToken);

        // Assert
        await _athleteService.Received().AddOrUpdateAthleteAsync(TestExistingAthlete);
        await _athleteService.Received().AddOrUpdateTokenAsync(TestToken);
        await _userService.DidNotReceiveWithAnyArgs().AddUserAsync(null);
        await _komUoW.Received().SaveChangesAsync();

        res.Should().BeSuccess();
    }

    private void PrepareMocks()
    {
        _userService.IsUserExistsAsync(TestNewAthlete.AthleteId).Returns(false);
        _userService.IsUserExistsAsync(TestExistingAthlete.AthleteId).Returns(true);

        _stravaTokenService.ExchangeAsync(TEST_INVALID_CODE, TEST_VALID_SCOPE).Returns(Result.Fail(new ExchangeError(ExchangeError.InvalidCode)));
        _stravaTokenService.ExchangeAsync(TEST_INVALID_CODE, TEST_VALID_SCOPE).Returns(Result.Fail(new ExchangeError(ExchangeError.InvalidCode)));
        _stravaTokenService.ExchangeAsync(TEST_EXISTING_ATHLETE_CODE, TEST_VALID_SCOPE).Returns(Result.Ok((TestExistingAthlete, TestToken)));
        _stravaTokenService.ExchangeAsync(TEST_NEW_ATHLETE_CODE, TEST_VALID_SCOPE).Returns(Result.Ok((TestNewAthlete, TestToken)));
    }

    private async Task AssertConnectNoUpdate()
    {
        await _athleteService.DidNotReceiveWithAnyArgs().AddOrUpdateAthleteAsync(null);
        await _athleteService.DidNotReceiveWithAnyArgs().AddOrUpdateTokenAsync(null);
        await _userService.DidNotReceiveWithAnyArgs().AddUserAsync(null);
        await _komUoW.DidNotReceiveWithAnyArgs().SaveChangesAsync();
    }
    #endregion
}

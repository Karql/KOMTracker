using AutoFixture;
using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Errors.Strava.Token;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using KomTracker.Application.Tests.Persistence;
using KomTracker.Domain.Entities.Token;
using KomTracker.Domain.Errors.Athlete;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Tests.UserManager;
using Utils.UnitOfWork.Abstract;
using Xunit;
using IStravaTokenService = KomTracker.Application.Interfaces.Services.Strava.ITokenService;

namespace KomTracker.Application.Tests.Infrastructure.Services;

public class AthleteServiceTests
{
    #region TestData
    private const int TestAthleteId = 1;

    private TokenEntity TestValidToken => new TokenEntity
    {
        ExpiresAt = DateTime.UtcNow.AddMinutes(60)
    };

    private TokenEntity TestInvalidToken => new TokenEntity
    {
        ExpiresAt = DateTime.UtcNow.AddMinutes(-60)
    };

    private TokenEntity TestNewToken => new TokenEntity
    {
        ExpiresAt = DateTime.UtcNow.AddHours(6)
    };

    #endregion

    private readonly IKOMUnitOfWork _komUoW;
    private readonly IStravaTokenService _stravaTokenService;
    private readonly IAthleteRepository _athleteRepository;

    private readonly AthleteService _athleteService;

    public AthleteServiceTests()
    {
        _athleteRepository = Substitute.For<IAthleteRepository>();
        _komUoW = new TestKOMUnitOfWork(new Dictionary<Type, IRepository>
        {
            { typeof(IAthleteRepository), _athleteRepository }
        });

        _stravaTokenService = Substitute.For<IStravaTokenService>();

        _athleteService = new AthleteService(_komUoW, _stravaTokenService);
    }

    #region Get valid token
    [Fact]
    public async Task Get_valid_return_error_and_decativate_athlete_when_no_token_in_db()
    {
        // Arrange
        _athleteRepository.GetTokenAsync(TestAthleteId).Returns((TokenEntity)null);

        // Act
        var res = await _athleteService.GetValidTokenAsync(TestAthleteId);

        // Assert
        res.Should().BeFailure();
        res.HasError<GetValidTokenError>(x => x.Message == GetValidTokenError.NoTokenInDB).Should().BeTrue();

        await _athleteRepository.Received().DeactivateAthleteAsync(TestAthleteId);
        await _stravaTokenService.DidNotReceiveWithAnyArgs().RefreshAsync(null);
        await _athleteRepository.DidNotReceiveWithAnyArgs().AddOrUpdateTokenAsync(null);
    }

    [Fact]
    public async Task Get_valid_token_return_token_directly_from_db_when_valid()
    {
        // Arrange
        var testValidToken = TestValidToken;
        _athleteRepository.GetTokenAsync(TestAthleteId).Returns(testValidToken);

        // Act
        var res = await _athleteService.GetValidTokenAsync(TestAthleteId);

        // Assert
        res.Should().BeSuccess();
        res.Value.Should().BeEquivalentTo(testValidToken);

        await _stravaTokenService.DidNotReceiveWithAnyArgs().RefreshAsync(null);
        await _athleteRepository.DidNotReceiveWithAnyArgs().AddOrUpdateTokenAsync(null);
    }

    [Fact]
    public async Task Get_valid_token_try_refresh_when_invalid()
    {
        // Arrange
        var testInvalidToken = TestInvalidToken;
        var testNewToken = TestNewToken;
        _athleteRepository.GetTokenAsync(TestAthleteId).Returns(testInvalidToken);
        _stravaTokenService.RefreshAsync(testInvalidToken).Returns(Result.Ok(testNewToken));

        // Act
        var res = await _athleteService.GetValidTokenAsync(TestAthleteId);

        // Assert
        res.Should().BeSuccess();
        res.Value.Should().BeEquivalentTo(testNewToken);

        await _athleteRepository.Received().AddOrUpdateTokenAsync(testNewToken);
    }

    [Fact]
    public async Task Get_valid_token_return_error_and_decativate_athlete_when_refresh_fail()
    {
        // Arrange
        var testInvalidToken = TestInvalidToken;
        _athleteRepository.GetTokenAsync(TestAthleteId).Returns(testInvalidToken);
        _stravaTokenService.RefreshAsync(testInvalidToken).Returns(Result.Fail<TokenEntity>(new RefreshError(RefreshError.InvalidRefreshToken)));

        // Act
        var res = await _athleteService.GetValidTokenAsync(TestAthleteId);

        // Assert
        res.Should().BeFailure();
        res.HasError<GetValidTokenError>(x => x.Message == GetValidTokenError.RefreshFailed).Should().BeTrue();

        await _athleteRepository.Received().DeactivateAthleteAsync(TestAthleteId);
        await _athleteRepository.DidNotReceiveWithAnyArgs().AddOrUpdateTokenAsync(null);
    }
    #endregion    
}
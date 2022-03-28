using AutoMapper;
using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Errors.Strava.Token;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Token;
using KomTracker.Infrastructure.Strava.Services;
using NSubstitute;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ApiModel = Strava.API.Client.Model;

namespace KomTracker.Infrastructure.Tests.Strava.Services;

public class TokenServiceTests
{
    #region TestData
    private const string TEST_CODE = "xxx";
    private const string TEST_SCOPE = "read";

    private TokenEntity CurrentToken = new TokenEntity
    {
        RefreshToken = "currentRefresh123",
        AccessToken = "currentAccess123",
        AthleteId = 1,
        Scope = TEST_SCOPE
    };
    #endregion

    private readonly IMapper _mapper;
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ITokenApi _tokenApi;

    private readonly TokenService _tokenService;

    public TokenServiceTests()
    {            
        _mapper = Substitute.For<IMapper>();
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _tokenApi = Substitute.For<ITokenApi>();

        _tokenService = new TokenService(_mapper, _komUoW, _tokenApi);
    }

    #region Exchange code for token
    [Fact]
    public async Task Exchange_for_valid_code_returns_token_and_athlete_summary()
    {
        // Arrange
        var apiResult = new ApiModel.Token.TokenWithAthleteModel
        {
            Athlete = new ApiModel.Athlete.AthleteSummaryModel()
        };

        var expectedAthlete = new AthleteEntity
        {
            Username = "User"
        };

        var expectedToken = new TokenEntity
        {
            AccessToken = "123"
        };
            
        _tokenApi.ExchangeAsync(TEST_CODE).Returns(Result.Ok(apiResult));
        _mapper.Map<AthleteEntity>(apiResult.Athlete).Returns(expectedAthlete);
        _mapper.Map<TokenEntity>(apiResult).Returns(expectedToken);

        // Act
        var res = await _tokenService.ExchangeAsync(TEST_CODE, TEST_SCOPE);

        // Assert
        res.Should().BeSuccess();

        var (actualAthlete, actualToken) = res.Value;
        actualAthlete.Should().BeEquivalentTo(expectedAthlete);
        actualToken.Should().BeEquivalentTo(expectedToken);
    }

    [Fact]
    public async Task Exchange_for_invalid_code_returns_error()
    {
        // Arrange
        _tokenApi.ExchangeAsync(TEST_CODE).Returns(Result.Fail(new ApiModel.Token.Error.ExchangeError(ApiModel.Token.Error.ExchangeError.InvalidCode)));

        // Act
        var res = await _tokenService.ExchangeAsync(TEST_CODE, TEST_SCOPE);

        // Assert
        res.Should().BeFailure();
        res.HasError<ExchangeError>(x => x.Message == ExchangeError.InvalidCode);
    }

    [Fact]
    public async Task Exchange_when_failed_throws_error()
    {
        // Arrange
        _tokenApi.ExchangeAsync(TEST_CODE).Returns(Result.Fail(new ApiModel.Token.Error.ExchangeError(ApiModel.Token.Error.ExchangeError.UnknownError)));

        // Act
        var action = () => _tokenService.ExchangeAsync(TEST_CODE, TEST_SCOPE);

        // Assert
        await action.Should().ThrowAsync<Exception>();
    }
    #endregion

    #region Refresh token
    [Fact]
    public async Task Exchange_for_valid_refreshes_token_returns_new_token()
    {
        // Arrange
        var apiResult = new ApiModel.Token.TokenModel
        {
            RefreshToken = "newRefresh123",
            AccessToken = "newAccess123",
            ExpiresAt = DateTime.UtcNow,
        };

        var expectedToken = new TokenEntity
        {
            RefreshToken = apiResult.RefreshToken,
            AccessToken = apiResult.AccessToken,
            ExpiresAt = apiResult.ExpiresAt,
            // Testing rewrited properties
            AthleteId = CurrentToken.AthleteId,
            Scope = CurrentToken.Scope
        };

        _tokenApi.RefreshAsync(CurrentToken.RefreshToken).Returns(Result.Ok(apiResult));
        _mapper.Map<TokenEntity>(apiResult).Returns(new TokenEntity { 
            RefreshToken = apiResult.RefreshToken,
            AccessToken = apiResult.AccessToken,
            ExpiresAt = apiResult.ExpiresAt
        });

        // Act
        var res = await _tokenService.RefreshAsync(CurrentToken);

        // Assert
        res.Should().BeSuccess();
        var actualToken = res.Value;
        actualToken.Should().BeEquivalentTo(expectedToken);
    }

    [Fact]
    public async Task Refresh_for_invalid_refreshes_token_returns_error()
    {
        // Arrange
        _tokenApi.RefreshAsync(CurrentToken.RefreshToken).Returns(Result.Fail(new ApiModel.Token.Error.RefreshError(ApiModel.Token.Error.RefreshError.InvalidRefreshToken)));

        // Act
        var res = await _tokenService.RefreshAsync(CurrentToken);

        // Assert
        res.Should().BeFailure();
        res.HasError<RefreshError>(x => x.Message == RefreshError.InvalidRefreshToken);
    }

    [Fact]
    public async Task Refresh_when_failed_throws_error()
    {
        // Arrange
        _tokenApi.RefreshAsync(CurrentToken.RefreshToken).Returns(Result.Fail(new ApiModel.Token.Error.RefreshError(ApiModel.Token.Error.RefreshError.UnknownError)));

        // Act
        var action = () => _tokenService.RefreshAsync(CurrentToken);

        // Assert
        await action.Should().ThrowAsync<Exception>();
    }
    #endregion
}
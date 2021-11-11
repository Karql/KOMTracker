using AutoMapper;
using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KOMTracker.API.DAL;
using KOMTracker.API.Infrastructure.Services;
using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Token;
using KOMTracker.API.Models.Token.Error;
using NSubstitute;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ApiModel = Strava.API.Client.Model;

namespace KOMTracker.API.Tests.Infrastructure.Services;

public class TokenServiceTests
{
    #region TestData
    private const string TEST_CODE = "xxx";
    private const string TEST_SCOPE = "read";

    private TokenModel CurrentToken = new TokenModel
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
    public async Task Exchange_for_valid_code_return_token_and_athlete_summary()
    {
        // Arrange
        var apiResult = new ApiModel.Token.TokenWithAthleteModel
        {
            Athlete = new ApiModel.Athlete.AthleteSummaryModel()
        };

        var expectedAthlete = new AthleteModel
        {
            Username = "User"
        };

        var expectedToken = new TokenModel
        {
            AccessToken = "123"
        };
            
        _tokenApi.ExchangeAsync(TEST_CODE).Returns(Result.Ok(apiResult));
        _mapper.Map<AthleteModel>(apiResult.Athlete).Returns(expectedAthlete);
        _mapper.Map<TokenModel>(apiResult).Returns(expectedToken);

        // Act
        var res = await _tokenService.ExchangeAsync(TEST_CODE, TEST_SCOPE);

        // Assert
        res.Should().BeSuccess();

        var (actualAthlete, actualToken) = res.Value;
        actualAthlete.Should().BeEquivalentTo(expectedAthlete);
        actualToken.Should().BeEquivalentTo(expectedToken);
    }

    [Fact]
    public async Task Exchange_for_invalid_code_return_error()
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
    public async Task Exchange_when_failed_throw_error()
    {
        // Arrange
        _tokenApi.ExchangeAsync(TEST_CODE).Returns(Result.Fail(new ApiModel.Token.Error.ExchangeError(ApiModel.Token.Error.ExchangeError.UnknownError)));

        // Act
        Func<Task<Result<(AthleteModel, TokenModel)>>> action = () => _tokenService.ExchangeAsync(TEST_CODE, TEST_SCOPE);

        // Assert
        await action .Should().ThrowAsync<Exception>();
    }
    #endregion

    #region Refresh token
    [Fact]
    public async Task Exchange_for_valid_refresh_token_return_new_token()
    {
        // Arrange
        var apiResult = new ApiModel.Token.TokenModel
        {
            RefreshToken = "newRefresh123",
            AccessToken = "newAccess123",
            ExpiresAt = DateTime.UtcNow,
        };

        var expectedToken = new TokenModel
        {
            RefreshToken = apiResult.RefreshToken,
            AccessToken = apiResult.AccessToken,
            ExpiresAt = apiResult.ExpiresAt,
            // Testing rewrited properties
            AthleteId = CurrentToken.AthleteId,
            Scope = CurrentToken.Scope
        };

        _tokenApi.RefreshAsync(CurrentToken.RefreshToken).Returns(Result.Ok(apiResult));
        _mapper.Map<TokenModel>(apiResult).Returns(new TokenModel { 
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
    public async Task Refresh_for_invalid_refresh_token_return_error()
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
    public async Task Refresh_when_failed_throw_error()
    {
        // Arrange
        _tokenApi.RefreshAsync(CurrentToken.RefreshToken).Returns(Result.Fail(new ApiModel.Token.Error.RefreshError(ApiModel.Token.Error.RefreshError.UnknownError)));

        // Act
        Func<Task<Result<TokenModel>>> action = () => _tokenService.RefreshAsync(CurrentToken);

        // Assert
        await action.Should().ThrowAsync<Exception>();
    }
    #endregion
}
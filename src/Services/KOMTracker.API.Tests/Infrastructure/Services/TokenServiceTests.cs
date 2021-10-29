using AutoMapper;
using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
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

namespace KOMTracker.API.Tests.Infrastructure.Services
{
    public class TokenServiceTests
    {
        private const string TEST_CODE = "xxx";
        private const string TEST_SCOPE = "read";

        private readonly ITokenApi _tokenApi;
        private readonly IMapper _mapper;

        private readonly TokenService _tokenService;

        public TokenServiceTests()
        {
            _tokenApi = Substitute.For<ITokenApi>();
            _mapper = Substitute.For<IMapper>();

            _tokenService = new TokenService(_mapper, _tokenApi);
        }

        #region Exchange code for token
        [Fact]
        public async Task Exchange_for_valid_code_return_token_and_athlete_summary()
        {
            // Arrange
            var apiResult = new ApiModel.Token.TokenWithAthleteModel
            {
                Athlete = new ApiModel.Athlete.AthleteModel()
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
        public void Exchange_when_failed_throw_error()
        {
            // Arrange
            _tokenApi.ExchangeAsync(TEST_CODE).Returns(Result.Fail(new ApiModel.Token.Error.ExchangeError(ApiModel.Token.Error.ExchangeError.UnknownError)));

            // Act
            Func<Task<Result<(AthleteModel, TokenModel)>>> action = () => _tokenService.ExchangeAsync(TEST_CODE, TEST_SCOPE);

            // Assert
            action.Should().ThrowAsync<Exception>();
        }


        #endregion
    }
}

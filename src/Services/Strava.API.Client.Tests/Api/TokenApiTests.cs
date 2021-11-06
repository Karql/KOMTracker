using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Tests.Logging;
using Utils.Tests.HttpClient;
using RichardSzalay.MockHttp;
using Strava.API.Client.Model.Config;
using Xunit;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using System.Net.Mime;
using Strava.API.Client.Model.Athlete;
using Strava.API.Client.Model.Token;
using Strava.API.Client.Tests.Extensions.Model.Athlete;
using Strava.API.Client.Model.Base;
using Strava.API.Client.Tests.Extensions.Model.Base;
using Strava.API.Client.Model.Token.Error;
using FluentResults;

namespace Strava.API.Client.Tests.Api
{
    public class TokenApiTests
    {
        private readonly ITestLogger<TokenApi> _logger;
        private readonly MockHttpMessageHandler _mockHttp;

        private readonly TokenApi _tokenApi;

        #region TestData
        private const string TEST_CODE = "code123";
        private const int TEST_CLIENT_ID = 123;
        private const string TEST_CLIENT_SECRET = "xyz";
        private const string TEST_REFRESH_TOKEN = "refresh123";
        private static ConfigModel TestConfig => new ConfigModel
        {
            ClientID = TEST_CLIENT_ID,
            ClientSecret = TEST_CLIENT_SECRET
        };

        private static AthleteSummaryModel ExpectedAthlete => new AthleteSummaryModel
        {
            Id = 1,
            ResourceState = Client.Model.Base.ResourceStateEnum.Summary,
            Username = "username",
            FirstName = "firstname",
            Lastname = "lastname",
            Bio = "bio",
            City = "city",
            State = "state",
            Country = "country",
            Sex = "M",
            Premium = true,
            Summit = true,
            CreatedAt = DateTime.Today.ToUniversalTime(), // Today has no miliseconds etc.
            UpdatedAt = DateTime.Today.ToUniversalTime(), // Today has no miliseconds etc.
            BadgeTypeId = 1,
            Weight = 75,
            Profile = "http://profile.com",
            ProfileMedium = "http://profile-medium.com/"
        };

        private static TokenModel ExpectedToken => new TokenModel
        {
            ExpiresAt = DateTime.Today.AddSeconds(10).ToUniversalTime(), // Today has no miliseconds etc.
            ExpiresIn = 10,
            AccessToken = "xxx",
            RefreshToken = "yyy",
            TokenType = "Bearer"
        };

        private static TokenWithAthleteModel ExpectedTokenWithAthlete => new TokenWithAthleteModel(ExpectedToken, ExpectedAthlete);

        private static FaultModel InvalidCodeFault => new FaultModel
        {
            Message = "Bad Request",
            Errors = new[]
            {
                new ErrorModel
                {
                    Resource = "AuthorizationCode",
                    Field = "code",
                    Code = "invalid"
                }
            }
        };

        private static FaultModel InvalidRefreshTokenFault => new FaultModel
        {
            Message = "Bad Request",
            Errors = new[]
            {
                new ErrorModel
                {
                    Resource = "RefreshToken",
                    Field = "refresh_token",
                    Code = "invalid"
                }
            }
        };

        private static FaultModel Fault1 => new FaultModel
        {
            Message = "Bad Request",
            Errors = new[]
            {
                new ErrorModel
                {
                    Resource = "resource1",
                    Field = "field1",
                    Code = "code1"
                }
            }
        };
        #endregion

        public TokenApiTests(ITestLogger<TokenApi> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mockHttp = new MockHttpMessageHandler();

            _tokenApi = new TokenApi(_logger, TestConfig, _mockHttp.ToHttpClientFactory());
        }

        #region Exchange code for token
        [Fact]
        public async Task Exchange_for_valid_code_call_api_and_return_token_and_athlete_summary()
        {
            // Arrange
            var shouldUrl = $"https://www.strava.com/oauth/token?client_id={TEST_CLIENT_ID}&client_secret={TEST_CLIENT_SECRET}&code={TEST_CODE}&grant_type=authorization_code";

            var expectedTokenWithAthlete = ExpectedTokenWithAthlete;

            _mockHttp.Expect(HttpMethod.Post, shouldUrl)
                .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, ExpectedTokenWithAthlete.ToJson());

            // Act
            var res = await _tokenApi.ExchangeAsync(TEST_CODE);

            // Assert
            res.Should().BeSuccess();
            var actualTokenWithAthlete = res.Value;
            actualTokenWithAthlete.Should().BeEquivalentTo(expectedTokenWithAthlete);

            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task Exchange_for_invalid_code_return_error()
        {
            // Arrange
            var shouldUrl = $"https://www.strava.com/oauth/token?client_id={TEST_CLIENT_ID}&client_secret={TEST_CLIENT_SECRET}&code={TEST_CODE}&grant_type=authorization_code";

            _mockHttp.Expect(HttpMethod.Post, shouldUrl)
                .Respond(HttpStatusCode.BadRequest, MediaTypeNames.Application.Json, InvalidCodeFault.ToJson());

            // Act
            var res = await _tokenApi.ExchangeAsync(TEST_CODE);

            // Assert
            res.Should().BeFailure();
            res.HasError<ExchangeError>(x => x.Message == ExchangeError.InvalidCode).Should().BeTrue();

            _mockHttp.VerifyNoOutstandingExpectation();

            _logger.CheckLogWarning("Invalid code!");
        }

        [Fact]
        public async Task Exchange_when_failed_return_error()
        {
            // Arrange
            var shouldUrl = $"https://www.strava.com/oauth/token?client_id={TEST_CLIENT_ID}&client_secret={TEST_CLIENT_SECRET}&code={TEST_CODE}&grant_type=authorization_code";

            _mockHttp.Expect(HttpMethod.Post, shouldUrl)
                .Respond(HttpStatusCode.BadRequest, MediaTypeNames.Application.Json, Fault1.ToJson());

            // Act
            var res = await _tokenApi.ExchangeAsync(TEST_CODE);

            // Assert
            res.Should().BeFailure();
            res.HasError<ExchangeError>(x => x.Message == ExchangeError.UnknownError).Should().BeTrue();

            _mockHttp.VerifyNoOutstandingExpectation();

            _logger.CheckLogError("failed! SatusCode");
        }

        [Fact]
        public async Task Exchange_throw_exception_when_something_went_wrong()
        {
            // Arrange
            var shouldUrl = $"https://www.strava.com/oauth/token?client_id={TEST_CLIENT_ID}&client_secret={TEST_CLIENT_SECRET}&code={TEST_CODE}&grant_type=authorization_code";

            _mockHttp.Expect(HttpMethod.Post, shouldUrl)
                .Throw(new Exception("Something went wrong"));

            // Act
            Func<Task> action = () => _tokenApi.ExchangeAsync(TEST_CODE);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }
        #endregion

        #region Refresh token
        [Fact]
        public async Task Refresh_for_valid_refresh_token_call_api_and_return_new_token()
        {
            // Arrange
            var shouldUrl = $"https://www.strava.com/oauth/token?client_id={TEST_CLIENT_ID}&client_secret={TEST_CLIENT_SECRET}&refresh_token={TEST_REFRESH_TOKEN}&grant_type=refresh_token";

            _mockHttp.Expect(HttpMethod.Post, shouldUrl)
                .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, ExpectedToken.ToJson());

            // Act
            var res = await _tokenApi.RefreshAsync(TEST_REFRESH_TOKEN);

            // Assert
            res.Should().BeSuccess();
            var actualTokenWithAthlete = res.Value;
            actualTokenWithAthlete.Should().BeEquivalentTo(ExpectedToken);

            _mockHttp.VerifyNoOutstandingExpectation();
        }

        [Fact]
        public async Task Refresh_for_invalid_refresh_token_return_error()
        {
            // Arrange
            var shouldUrl = $"https://www.strava.com/oauth/token?client_id={TEST_CLIENT_ID}&client_secret={TEST_CLIENT_SECRET}&refresh_token={TEST_REFRESH_TOKEN}&grant_type=refresh_token";

            _mockHttp.Expect(HttpMethod.Post, shouldUrl)
                .Respond(HttpStatusCode.BadRequest, MediaTypeNames.Application.Json, InvalidRefreshTokenFault.ToJson());

            // Act
            var res = await _tokenApi.RefreshAsync(TEST_REFRESH_TOKEN);

            // Assert
            res.Should().BeFailure();
            res.HasError<RefreshError>(x => x.Message == RefreshError.InvalidRefreshToken).Should().BeTrue();

            _mockHttp.VerifyNoOutstandingExpectation();

            _logger.CheckLogWarning("Invalid refresh token!");
        }

        [Fact]
        public async Task Refresh_when_failed_return_error()
        {
            // Arrange
            var shouldUrl = $"https://www.strava.com/oauth/token?client_id={TEST_CLIENT_ID}&client_secret={TEST_CLIENT_SECRET}&refresh_token={TEST_REFRESH_TOKEN}&grant_type=refresh_token";

            _mockHttp.Expect(HttpMethod.Post, shouldUrl)
                .Respond(HttpStatusCode.BadRequest, MediaTypeNames.Application.Json, Fault1.ToJson());

            // Act
            var res = await _tokenApi.RefreshAsync(TEST_REFRESH_TOKEN);

            // Assert
            res.Should().BeFailure();
            res.HasError<RefreshError>(x => x.Message == RefreshError.UnknownError).Should().BeTrue();

            _mockHttp.VerifyNoOutstandingExpectation();

            _logger.CheckLogError("failed! SatusCode");
        }

        [Fact]
        public async Task Refresh_throw_exception_when_something_went_wrong()
        {
            // Arrange
            var shouldUrl = $"https://www.strava.com/oauth/token?client_id={TEST_CLIENT_ID}&client_secret={TEST_CLIENT_SECRET}&refresh_token={TEST_REFRESH_TOKEN}&grant_type=refresh_token";

            _mockHttp.Expect(HttpMethod.Post, shouldUrl)
                .Throw(new Exception("Something went wrong"));

            // Act
            Func<Task> action = () => _tokenApi.RefreshAsync(TEST_REFRESH_TOKEN);

            // Assert
            await action.Should().ThrowAsync<Exception>();
        }
        #endregion
    }
}

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

namespace Strava.API.Client.Tests.Api
{
    public class TokenApiTests
    {
        private readonly ITestLogger<TokenApi> _logger;
        private readonly MockHttpMessageHandler _mockHttp;

        private readonly TokenApi _tokenApi;

        private const string TEST_CODE = "code123";
        private const int TEST_CLIENT_ID = 123;
        private const string TEST_CLIENT_SECRET = "xyz";
        private static ConfigModel Config => new ConfigModel
        {
            ClientID = TEST_CLIENT_ID,
            ClientSecret = TEST_CLIENT_SECRET

        };

        public TokenApiTests(ITestLogger<TokenApi> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mockHttp = new MockHttpMessageHandler();

            _tokenApi = new TokenApi(_logger, Config, _mockHttp.ToHttpClientFactory());
        }

        #region ExchangeAsync
        [Fact]
        public async Task ExchangeAsync_Should_CallCorectUrlAndReturnSuccess()
        {
            // Arrange
            var shouldUrl = $"https://www.strava.com/oauth/token?client_id={TEST_CLIENT_ID}&client_secret={TEST_CLIENT_SECRET}&code={TEST_CODE}&grant_type=authorization_code";

            _mockHttp.Expect(HttpMethod.Post, shouldUrl)
                .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json,  "{}");

            // Act
            var res = await _tokenApi.ExchangeAsync(TEST_CODE);

            // Assert
            res.Should().BeSuccess();
            _mockHttp.VerifyNoOutstandingExpectation();
        }

        #endregion
    }
}

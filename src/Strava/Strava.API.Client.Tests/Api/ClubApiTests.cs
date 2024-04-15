using AutoFixture;
using FluentAssertions;
using RichardSzalay.MockHttp;
using Strava.API.Client.Api;
using Strava.API.Client.Model.Segment;
using Strava.API.Client.Tests.Extensions.Model.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Utils.Tests.HttpClient;
using Utils.Tests.Logging;
using Xunit;
using FluentResults.Extensions.FluentAssertions;
using Strava.API.Client.Model.Base;
using Strava.API.Client.Tests.Extensions.Model.Base;
using Strava.API.Client.Tests.Common;
using Strava.API.Client.Model.Club;
using Strava.API.Client.Tests.Extensions.Model.Club;

namespace Strava.API.Client.Tests.Api;

public class ClubApiTests
{
    private readonly ITestLogger<ClubApi> _logger;
    private readonly MockHttpMessageHandler _mockHttp;

    private readonly IClubApi _clubApi;

    #region TestData
    private const string TEST_TOKEN_VALID = "token123";
    private const string TEST_TOKEN_INVALID = "tokeninvalid123";
    #endregion

    public ClubApiTests(ITestLogger<ClubApi> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mockHttp = new MockHttpMessageHandler();

        _clubApi = new ClubApi(_logger, _mockHttp.ToHttpClientFactory());
    }

    #region Get clubs
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Get_clubs_iterates_through_all_pages_and_returns_list(int pageCount)
    {
        // Arrange
        var expectedClubs = new List<ClubSummaryModel>();

        var fixture = FixtureHelper.GetTestFixture();

        for (int pageNumber = 1; pageNumber <= pageCount; ++pageNumber)
        {
            var pageClubs = fixture.CreateMany<ClubSummaryModel>(5);

            _mockHttp.Expect(HttpMethod.Get, GetClubsUrl(pageNumber))
                .WithHeaders("Authorization", $"Bearer {TEST_TOKEN_VALID}")
                .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, pageClubs.ToJson());

            expectedClubs.AddRange(pageClubs);
        }

        // empty page at the end
        _mockHttp.Expect(HttpMethod.Get, GetClubsUrl(pageCount + 1))
            .WithHeaders("Authorization", $"Bearer {TEST_TOKEN_VALID}")
            .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, Enumerable.Empty<ClubSummaryModel>().ToJson());

        // Act
        var res = await _clubApi.GetClubsAsync(TEST_TOKEN_VALID);

        // Assert
        res.Should().BeSuccess();
        var actualClubs = res.Value;

        actualClubs.Should().BeEquivalentTo(expectedClubs);

        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Get_clubs_stops_iterating_and_returns_error(int errorOnPage)
    {
        // Arrange
        var fixture = FixtureHelper.GetTestFixture();

        for (int pageNumber = 1; pageNumber < errorOnPage; ++pageNumber)
        {
            var pageClubs = fixture.CreateMany<ClubSummaryModel>(5);

            _mockHttp.Expect(HttpMethod.Get, GetClubsUrl(pageNumber))
                .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, pageClubs.ToJson());
        }

        _mockHttp.Expect(HttpMethod.Get, GetClubsUrl(errorOnPage))
            .Respond(HttpStatusCode.BadRequest);

        // Act
        var res = await _clubApi.GetClubsAsync(TEST_TOKEN_VALID);

        // Assert
        res.Should().BeFailure();
        _mockHttp.VerifyNoOutstandingExpectation();
        _logger.CheckLogError("failed! SatusCode");
    }

    [Fact]
    public async Task Get_clubs_returns_unauthorized_on_401()
    {
        // Arrange
        _mockHttp.Expect(HttpMethod.Get, GetClubsUrl(1))
            .WithHeaders("Authorization", $"Bearer {TEST_TOKEN_INVALID}")
            .Respond(HttpStatusCode.Unauthorized);

        // Act
        var res = await _clubApi.GetClubsAsync(TEST_TOKEN_INVALID);

        // Arrange
        res.Should().BeFailure();
        _mockHttp.VerifyNoOutstandingExpectation();
        _logger.CheckLogWarning("Unauthorized!");
    }

    [Fact]
    public async Task Get_clubs_returns_too_many_requests_on_429()
    {
        // Arrange
        _mockHttp.Expect(HttpMethod.Get, GetClubsUrl(1))
            .WithHeaders("Authorization", $"Bearer {TEST_TOKEN_VALID}")
            .Respond(HttpStatusCode.TooManyRequests);

        // Act
        var res = await _clubApi.GetClubsAsync(TEST_TOKEN_VALID);

        // Arrange
        res.Should().BeFailure();
        _mockHttp.VerifyNoOutstandingExpectation();
        _logger.CheckLogError("Rate Limit Exceeded!");
    }

    [Fact]
    public async Task Get_clubs_throws_exception_when_something_went_wrong()
    {
        // Arrange
        _mockHttp.Expect(HttpMethod.Get, GetClubsUrl(1))
            .Throw(new Exception("Something went wrong"));

        // Act
        Func<Task> action = () => _clubApi.GetClubsAsync(TEST_TOKEN_VALID);

        // Assert
        await action.Should().ThrowAsync<Exception>();
    }

    private string GetClubsUrl(int page)
    {
        return $"https://www.strava.com/api/v3/athlete/clubs?per_page=200&page={page}"; // 200 max possible
    }
    #endregion
}

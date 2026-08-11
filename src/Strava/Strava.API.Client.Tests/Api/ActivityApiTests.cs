using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using RichardSzalay.MockHttp;
using Strava.API.Client.Api;
using Strava.API.Client.Model.Activity;
using Strava.API.Client.Model.Activity.Error;
using Strava.API.Client.Tests.Common;
using Strava.API.Client.Tests.Extensions.Model.Activity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Utils.Tests.HttpClient;
using Utils.Tests.Logging;
using Xunit;

namespace Strava.API.Client.Tests.Api;

public class ActivityApiTests
{
    private readonly ITestLogger<ActivityApi> _logger;
    private readonly MockHttpMessageHandler _mockHttp;

    private readonly IActivityApi _activityApi;

    #region TestData
    private const string TEST_TOKEN_VALID = "token123";
    private const string TEST_TOKEN_INVALID = "tokeninvalid123";
    #endregion

    public ActivityApiTests(ITestLogger<ActivityApi> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mockHttp = new MockHttpMessageHandler();

        _activityApi = new ActivityApi(_logger, _mockHttp.ToHttpClientFactory());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Get_activities_iterates_through_all_pages_and_returns_list(int pageCount)
    {
        // Arrange
        var expected = new List<ActivitySummaryModel>();
        var fixture = FixtureHelper.GetTestFixture();

        for (int page = 1; page <= pageCount; ++page)
        {
            var pageItems = fixture.CreateMany<ActivitySummaryModel>(5);

            _mockHttp.Expect(HttpMethod.Get, GetActivitiesUrl(page))
                .WithHeaders("Authorization", $"Bearer {TEST_TOKEN_VALID}")
                .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, pageItems.ToJson());

            expected.AddRange(pageItems);
        }

        // empty page terminates the loop
        _mockHttp.Expect(HttpMethod.Get, GetActivitiesUrl(pageCount + 1))
            .WithHeaders("Authorization", $"Bearer {TEST_TOKEN_VALID}")
            .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, Enumerable.Empty<ActivitySummaryModel>().ToJson());

        // Act
        var res = await _activityApi.GetActivitiesAsync(TEST_TOKEN_VALID);

        // Assert
        res.Should().BeSuccess();
        res.Value.Should().BeEquivalentTo(expected);

        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Get_activities_sends_after_and_before_window()
    {
        // Arrange
        const long after = 1000;
        const long before = 2000;

        var url = $"https://www.strava.com/api/v3/athlete/activities?per_page=200&page=1&after={after}&before={before}";

        _mockHttp.Expect(HttpMethod.Get, url)
            .WithHeaders("Authorization", $"Bearer {TEST_TOKEN_VALID}")
            .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, Enumerable.Empty<ActivitySummaryModel>().ToJson());

        // Act
        var res = await _activityApi.GetActivitiesAsync(TEST_TOKEN_VALID, after, before);

        // Assert
        res.Should().BeSuccess();
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Get_activities_stops_iterating_and_returns_error()
    {
        // Arrange
        _mockHttp.Expect(HttpMethod.Get, GetActivitiesUrl(1))
            .Respond(HttpStatusCode.BadRequest);

        // Act
        var res = await _activityApi.GetActivitiesAsync(TEST_TOKEN_VALID);

        // Assert
        res.Should().BeFailure();
        res.HasError<GetActivitiesError>(x => x.Message == GetActivitiesError.UnknownError).Should().BeTrue();

        _mockHttp.VerifyNoOutstandingExpectation();
        _logger.CheckLogError("failed! Page");
    }

    [Fact]
    public async Task Get_activities_returns_unauthorized_on_401()
    {
        // Arrange
        _mockHttp.Expect(HttpMethod.Get, GetActivitiesUrl(1))
            .WithHeaders("Authorization", $"Bearer {TEST_TOKEN_INVALID}")
            .Respond(HttpStatusCode.Unauthorized);

        // Act
        var res = await _activityApi.GetActivitiesAsync(TEST_TOKEN_INVALID);

        // Assert
        res.Should().BeFailure();
        res.HasError<GetActivitiesError>(x => x.Message == GetActivitiesError.Unauthorized).Should().BeTrue();

        _mockHttp.VerifyNoOutstandingExpectation();
        _logger.CheckLogWarning("Unauthorized!");
    }

    [Fact]
    public async Task Get_activities_returns_too_many_requests_on_429()
    {
        // Arrange
        _mockHttp.Expect(HttpMethod.Get, GetActivitiesUrl(1))
            .WithHeaders("Authorization", $"Bearer {TEST_TOKEN_VALID}")
            .Respond(HttpStatusCode.TooManyRequests);

        // Act
        var res = await _activityApi.GetActivitiesAsync(TEST_TOKEN_VALID);

        // Assert
        res.Should().BeFailure();
        res.HasError<GetActivitiesError>(x => x.Message == GetActivitiesError.TooManyRequests).Should().BeTrue();

        _mockHttp.VerifyNoOutstandingExpectation();
        _logger.CheckLogError("Rate Limit Exceeded!");
    }

    private static string GetActivitiesUrl(int page)
    {
        return $"https://www.strava.com/api/v3/athlete/activities?per_page=200&page={page}"; // 200 = max per page
    }
}

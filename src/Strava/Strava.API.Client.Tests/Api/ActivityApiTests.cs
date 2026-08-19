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

    #region GetActivity (single, detailed)
    private const long TEST_ACTIVITY_ID = 19598505831;

    private static string GetActivityUrl(long id) => $"https://www.strava.com/api/v3/activities/{id}";

    [Fact]
    public async Task Get_activity_deserializes_full_detailed_payload()
    {
        // Arrange
        _mockHttp.Expect(HttpMethod.Get, GetActivityUrl(TEST_ACTIVITY_ID))
            .WithHeaders("Authorization", $"Bearer {TEST_TOKEN_VALID}")
            .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, DetailedActivityJson);

        // Act
        var res = await _activityApi.GetActivityAsync(TEST_ACTIVITY_ID, TEST_TOKEN_VALID);

        // Assert — summary base
        res.Should().BeSuccess();
        var a = res.Value;
        a.Id.Should().Be(TEST_ACTIVITY_ID);
        a.Name.Should().Be("Afternoon Ride");
        a.GearId.Should().Be("b10707658");
        a.Athlete.Id.Should().Be(2394302);

        // Assert — detailed-only scalars + nested collections all populate
        a.Description.Should().Be("nice one");
        a.Calories.Should().Be(420.5f);
        a.PerceivedExertion.Should().Be(5);
        a.EmbedToken.Should().Be("tok-abc");
        a.AvailableZones.Should().Contain("heartrate");
        a.Gear.Should().NotBeNull();
        a.Gear!.Id.Should().Be("b10707658");
        a.Gear.Name.Should().Be("Sensa");
        a.SegmentEfforts.Should().ContainSingle();
        a.SegmentEfforts![0].Segment.Name.Should().Be("Test Segment");
        a.BestEfforts.Should().ContainSingle();
        a.SplitsMetric.Should().ContainSingle();
        a.SplitsMetric![0].Distance.Should().Be(1000f);
        a.SplitsStandard.Should().ContainSingle();
        a.Laps.Should().ContainSingle();
        a.Laps![0].LapIndex.Should().Be(1);
        a.Photos.Should().NotBeNull();
        a.Photos!.Primary!.UniqueId.Should().Be("photo-uid");
        a.Photos.Primary.Urls.Should().ContainKey("600");
        a.SimilarActivities.Should().NotBeNull();
        a.SimilarActivities!.Trend!.Speeds.Should().HaveCount(2);
        a.StatsVisibility.Should().ContainSingle();

        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Get_activity_returns_unauthorized_on_401()
    {
        _mockHttp.Expect(HttpMethod.Get, GetActivityUrl(TEST_ACTIVITY_ID))
            .Respond(HttpStatusCode.Unauthorized);

        var res = await _activityApi.GetActivityAsync(TEST_ACTIVITY_ID, TEST_TOKEN_INVALID);

        res.Should().BeFailure();
        res.HasError<GetActivityError>(x => x.Message == GetActivityError.Unauthorized).Should().BeTrue();
        _logger.CheckLogWarning("Unauthorized!");
    }

    [Fact]
    public async Task Get_activity_returns_too_many_requests_on_429()
    {
        _mockHttp.Expect(HttpMethod.Get, GetActivityUrl(TEST_ACTIVITY_ID))
            .Respond(HttpStatusCode.TooManyRequests);

        var res = await _activityApi.GetActivityAsync(TEST_ACTIVITY_ID, TEST_TOKEN_VALID);

        res.Should().BeFailure();
        res.HasError<GetActivityError>(x => x.Message == GetActivityError.TooManyRequests).Should().BeTrue();
        _logger.CheckLogError("Rate Limit Exceeded!");
    }

    [Fact]
    public async Task Get_activity_returns_not_found_on_404()
    {
        _mockHttp.Expect(HttpMethod.Get, GetActivityUrl(TEST_ACTIVITY_ID))
            .Respond(HttpStatusCode.NotFound);

        var res = await _activityApi.GetActivityAsync(TEST_ACTIVITY_ID, TEST_TOKEN_VALID);

        res.Should().BeFailure();
        res.HasError<GetActivityError>(x => x.Message == GetActivityError.NotFound).Should().BeTrue();
        _logger.CheckLogWarning("Not Found!");
    }

    [Fact]
    public async Task Get_activity_returns_unknown_error_on_500()
    {
        _mockHttp.Expect(HttpMethod.Get, GetActivityUrl(TEST_ACTIVITY_ID))
            .Respond(HttpStatusCode.InternalServerError);

        var res = await _activityApi.GetActivityAsync(TEST_ACTIVITY_ID, TEST_TOKEN_VALID);

        res.Should().BeFailure();
        res.HasError<GetActivityError>(x => x.Message == GetActivityError.UnknownError).Should().BeTrue();
        _logger.CheckLogError("failed!");
    }

    // Trimmed-but-representative real DetailedActivity payload (polyline/long arrays shortened).
    private const string DetailedActivityJson = @"{
        ""resource_state"": 3,
        ""athlete"": { ""id"": 2394302, ""resource_state"": 1 },
        ""name"": ""Afternoon Ride"",
        ""distance"": 3830.6,
        ""moving_time"": 629,
        ""elapsed_time"": 675,
        ""total_elevation_gain"": 11.0,
        ""type"": ""Ride"",
        ""sport_type"": ""Ride"",
        ""id"": 19598505831,
        ""start_date"": ""2026-08-06T13:44:38Z"",
        ""start_date_local"": ""2026-08-06T15:44:38Z"",
        ""timezone"": ""(GMT+01:00) Europe/Warsaw"",
        ""utc_offset"": 7200.0,
        ""gear_id"": ""b10707658"",
        ""average_speed"": 6.09,
        ""max_speed"": 10.34,
        ""description"": ""nice one"",
        ""calories"": 420.5,
        ""perceived_exertion"": 5,
        ""prefer_perceived_exertion"": true,
        ""hide_from_home"": false,
        ""leaderboard_opt_out"": false,
        ""segment_leaderboard_opt_out"": false,
        ""embed_token"": ""tok-abc"",
        ""available_zones"": [ ""heartrate"", ""power"" ],
        ""gear"": {
            ""id"": ""b10707658"", ""primary"": false, ""name"": ""Sensa"", ""nickname"": ""Sensa"",
            ""resource_state"": 2, ""retired"": false, ""distance"": 29143765, ""converted_distance"": 29143.8
        },
        ""segment_efforts"": [
            {
                ""id"": 111, ""resource_state"": 2, ""name"": ""Test Segment"",
                ""activity"": { ""id"": 19598505831, ""resource_state"": 1 },
                ""athlete"": { ""id"": 2394302, ""resource_state"": 1 },
                ""elapsed_time"": 60, ""moving_time"": 60,
                ""start_date"": ""2026-08-06T13:45:00Z"", ""start_date_local"": ""2026-08-06T15:45:00Z"",
                ""distance"": 300, ""start_index"": 10, ""end_index"": 40,
                ""average_cadence"": 85, ""device_watts"": false,
                ""segment"": { ""id"": 555, ""resource_state"": 2, ""name"": ""Test Segment"", ""activity_type"": ""Ride"", ""distance"": 300 }
            }
        ],
        ""best_efforts"": [
            {
                ""id"": 222, ""resource_state"": 2, ""name"": ""1k"",
                ""elapsed_time"": 120, ""moving_time"": 120,
                ""start_date"": ""2026-08-06T13:44:38Z"", ""start_date_local"": ""2026-08-06T15:44:38Z"",
                ""distance"": 1000, ""start_index"": 0, ""end_index"": 100
            }
        ],
        ""splits_metric"": [
            { ""distance"": 1000, ""elapsed_time"": 160, ""elevation_difference"": 3, ""moving_time"": 160, ""split"": 1, ""average_speed"": 6.25, ""pace_zone"": 0 }
        ],
        ""splits_standard"": [
            { ""distance"": 1609.34, ""elapsed_time"": 260, ""elevation_difference"": 5, ""moving_time"": 260, ""split"": 1, ""average_speed"": 6.19, ""pace_zone"": 0 }
        ],
        ""laps"": [
            {
                ""id"": 333, ""resource_state"": 2, ""name"": ""Lap 1"",
                ""activity"": { ""id"": 19598505831, ""resource_state"": 1 },
                ""athlete"": { ""id"": 2394302, ""resource_state"": 1 },
                ""elapsed_time"": 675, ""moving_time"": 629, ""start_date"": ""2026-08-06T13:44:38Z"", ""start_date_local"": ""2026-08-06T15:44:38Z"",
                ""distance"": 3830.6, ""start_index"": 0, ""end_index"": 200, ""total_elevation_gain"": 11,
                ""average_speed"": 6.09, ""max_speed"": 10.34, ""lap_index"": 1, ""split"": 1, ""pace_zone"": 0
            }
        ],
        ""photos"": { ""count"": 1, ""primary"": { ""id"": null, ""unique_id"": ""photo-uid"", ""urls"": { ""100"": ""u100"", ""600"": ""u600"" }, ""source"": 1 } },
        ""similar_activities"": {
            ""effort_count"": 3, ""average_speed"": 6.0, ""min_average_speed"": 5.0, ""mid_average_speed"": 6.0, ""max_average_speed"": 7.0,
            ""pr_rank"": null, ""frequency_milestone"": null, ""resource_state"": 2,
            ""trend"": { ""speeds"": [ 5.5, 6.5 ], ""current_activity_index"": 1, ""min_speed"": 5.0, ""mid_speed"": 6.0, ""max_speed"": 7.0, ""direction"": 0 }
        },
        ""stats_visibility"": [ { ""type"": ""heart_rate"", ""visibility"": ""everyone"" } ]
    }";
    #endregion

    private static string GetActivitiesUrl(int page)
    {
        return $"https://www.strava.com/api/v3/athlete/activities?per_page=200&page={page}"; // 200 = max per page
    }
}

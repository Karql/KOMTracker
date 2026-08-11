using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using RichardSzalay.MockHttp;
using Strava.API.Client.Api;
using Strava.API.Client.Model.Gear;
using Strava.API.Client.Model.Gear.Error;
using Strava.API.Client.Tests.Common;
using Strava.API.Client.Tests.Extensions.Model.Gear;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using Utils.Tests.HttpClient;
using Utils.Tests.Logging;
using Xunit;

namespace Strava.API.Client.Tests.Api;

public class GearApiTests
{
    private readonly ITestLogger<GearApi> _logger;
    private readonly MockHttpMessageHandler _mockHttp;

    private readonly IGearApi _gearApi;

    #region TestData
    private const string TEST_GEAR_ID = "b12345";
    private const string TEST_TOKEN_VALID = "token123";
    private const string TEST_TOKEN_INVALID = "tokeninvalid123";
    #endregion

    public GearApiTests(ITestLogger<GearApi> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mockHttp = new MockHttpMessageHandler();

        _gearApi = new GearApi(_logger, _mockHttp.ToHttpClientFactory());
    }

    [Fact]
    public async Task Get_gear_returns_detailed_gear()
    {
        // Arrange
        var expected = FixtureHelper.GetTestFixture().Create<GearDetailedModel>();

        _mockHttp.Expect(HttpMethod.Get, GetGearUrl(TEST_GEAR_ID))
            .WithHeaders("Authorization", $"Bearer {TEST_TOKEN_VALID}")
            .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, expected.ToJson());

        // Act
        var res = await _gearApi.GetGearAsync(TEST_GEAR_ID, TEST_TOKEN_VALID);

        // Assert
        res.Should().BeSuccess();
        res.Value.Should().BeEquivalentTo(expected);

        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Get_gear_preserves_large_distance_precision()
    {
        // Arrange — real Strava value: 21 207 353 m needs double (float would drop the last digit).
        const string json = @"{
            ""id"": ""b805524"", ""resource_state"": 3, ""primary"": false,
            ""name"": ""Bianka"", ""nickname"": ""Bianka"", ""retired"": false,
            ""distance"": 21207353, ""converted_distance"": 21207.4,
            ""brand_name"": ""Canyon"", ""model_name"": ""Endurace"", ""frame_type"": 3, ""description"": ""x""
        }";

        _mockHttp.Expect(HttpMethod.Get, GetGearUrl(TEST_GEAR_ID))
            .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, json);

        // Act
        var res = await _gearApi.GetGearAsync(TEST_GEAR_ID, TEST_TOKEN_VALID);

        // Assert
        res.Should().BeSuccess();
        res.Value.Distance.Should().Be(21207353d);
        res.Value.ConvertedDistance.Should().Be(21207.4d);
        res.Value.Retired.Should().BeFalse();
        res.Value.FrameType.Should().Be(3);

        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Get_gear_returns_unauthorized_on_401()
    {
        // Arrange
        _mockHttp.Expect(HttpMethod.Get, GetGearUrl(TEST_GEAR_ID))
            .WithHeaders("Authorization", $"Bearer {TEST_TOKEN_INVALID}")
            .Respond(HttpStatusCode.Unauthorized);

        // Act
        var res = await _gearApi.GetGearAsync(TEST_GEAR_ID, TEST_TOKEN_INVALID);

        // Assert
        res.Should().BeFailure();
        res.HasError<GetGearError>(x => x.Message == GetGearError.Unauthorized).Should().BeTrue();

        _mockHttp.VerifyNoOutstandingExpectation();
        _logger.CheckLogWarning("Unauthorized!");
    }

    [Fact]
    public async Task Get_gear_returns_unknown_error_on_failure()
    {
        // Arrange
        _mockHttp.Expect(HttpMethod.Get, GetGearUrl(TEST_GEAR_ID))
            .Respond(HttpStatusCode.BadRequest);

        // Act
        var res = await _gearApi.GetGearAsync(TEST_GEAR_ID, TEST_TOKEN_VALID);

        // Assert
        res.Should().BeFailure();
        res.HasError<GetGearError>(x => x.Message == GetGearError.UnknownError).Should().BeTrue();

        _mockHttp.VerifyNoOutstandingExpectation();
        _logger.CheckLogError("failed! Gear Id");
    }

    private static string GetGearUrl(string gearId)
    {
        return $"https://www.strava.com/api/v3/gear/{gearId}";
    }
}

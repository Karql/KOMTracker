using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
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

namespace Strava.API.Client.Tests.Api;

public class SegmentApiTests
{
    private readonly ITestLogger<SegmentApi> _logger;
    private readonly MockHttpMessageHandler _mockHttp;

    private readonly ISegmentApi _segmentApi;

    #region TestData
    private const int TEST_SEGMENT_ID = 1;
    private const string TEST_VALID_TOKEN = "token123";
    #endregion

    public SegmentApiTests(ITestLogger<SegmentApi> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mockHttp = new MockHttpMessageHandler();

        _segmentApi = new SegmentApi(_logger, _mockHttp.ToHttpClientFactory());
    }

    #region GetSegmentAsync
    [Fact]
    public async Task Get_segment_returns_detailed_segment()
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Register<DateTime>(() => DateTime.Today.ToUniversalTime()); // Today has no miliseconds etc.
        var expectedSegment = fixture.Create<SegmentDetailedModel>();

        _mockHttp.Expect(HttpMethod.Get, GetSegmentUrl(TEST_SEGMENT_ID))
            .WithHeaders("Authorization", $"Bearer {TEST_VALID_TOKEN}")
            .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, expectedSegment.ToJson());

        // Act
        var res = await _segmentApi.GetSegmentAsync(TEST_SEGMENT_ID, TEST_VALID_TOKEN);

        // Assert
        res.Should().BeSuccess();
        var actualSegment = res.Value;

        actualSegment.Should().BeEquivalentTo(expectedSegment);

        _mockHttp.VerifyNoOutstandingExpectation();
    }

    private string GetSegmentUrl(long segmentId)
    {
        return $"https://www.strava.com/api/v3/segments/{segmentId}";
    }
    #endregion
}

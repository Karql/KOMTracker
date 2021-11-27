﻿using AutoFixture;
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

namespace Strava.API.Client.Tests.Api;

public class AthleteApiTests
{
    private readonly ITestLogger<AthleteApi> _logger;
    private readonly MockHttpMessageHandler _mockHttp;

    private readonly IAthleteApi _athleteApi;

    #region TestData
    private const int TEST_ATHLETE_ID = 666;
    private const string TEST_VALID_TOKEN = "token123";
    private const string TEST_INVALID_TOKEN = "tokeninvalid123";
    #endregion

    public AthleteApiTests(ITestLogger<AthleteApi> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mockHttp = new MockHttpMessageHandler();

        _athleteApi = new AthleteApi(_logger, _mockHttp.ToHttpClientFactory());
    }

    #region Get koms
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Get_koms_iterate_through_all_pages_and_return_list(int pageCount)
    {
        // Arrange
        var expectedEfforts = new List<SegmentEffortDetailedModel>();

        var fixture = new Fixture();
        fixture.Register<DateTime>(() => DateTime.Today.ToUniversalTime()); // Today has no miliseconds etc.

        for (int pageNumber = 1; pageNumber <= pageCount; ++pageNumber)
        {
            var pageEfforts = fixture.CreateMany<SegmentEffortDetailedModel>(5);

            _mockHttp.Expect(HttpMethod.Get, GetKomsUrl(pageNumber))
                .WithHeaders("Authorization", $"Bearer {TEST_VALID_TOKEN}")
                .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, pageEfforts.ToJson());

            expectedEfforts.AddRange(pageEfforts);
        }

        // empty page at the end
        _mockHttp.Expect(HttpMethod.Get, GetKomsUrl(pageCount + 1))
            .WithHeaders("Authorization", $"Bearer {TEST_VALID_TOKEN}")
            .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, Enumerable.Empty<SegmentEffortDetailedModel>().ToJson());

        // Act
        var res = await _athleteApi.GetKomsAsync(TEST_ATHLETE_ID, TEST_VALID_TOKEN);

        // Assert
        res.Should().BeSuccess();
        var actualEfforts = res.Value;

        actualEfforts.Should().BeEquivalentTo(expectedEfforts);

        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Get_koms_stop_iterate_and_return_error(int errorOnPage)
    {
        // Arrange
        var fixture = new Fixture();
        fixture.Register<DateTime>(() => DateTime.Today.ToUniversalTime()); // Today has no miliseconds etc.

        for (int pageNumber = 1; pageNumber < errorOnPage; ++pageNumber)
        {
            var pageEfforts = fixture.CreateMany<SegmentEffortDetailedModel>(5);

            _mockHttp.Expect(HttpMethod.Get, GetKomsUrl(pageNumber))
                .Respond(HttpStatusCode.OK, MediaTypeNames.Application.Json, pageEfforts.ToJson());
        }

        _mockHttp.Expect(HttpMethod.Get, GetKomsUrl(errorOnPage))
            .Respond(HttpStatusCode.BadRequest);

        // Act
        var res = await _athleteApi.GetKomsAsync(TEST_ATHLETE_ID, TEST_VALID_TOKEN);

        // Assert
        res.Should().BeFailure();
        _mockHttp.VerifyNoOutstandingExpectation();
        _logger.CheckLogError("failed! SatusCode");
    }

    [Fact]
    public async Task Get_koms_return_unauthorized_on_401()
    {
        // Arrange
        _mockHttp.Expect(HttpMethod.Get, GetKomsUrl(1))
            .WithHeaders("Authorization", $"Bearer {TEST_INVALID_TOKEN}")
            .Respond(HttpStatusCode.Unauthorized);

        // Act
        var res = await _athleteApi.GetKomsAsync(TEST_ATHLETE_ID, TEST_INVALID_TOKEN);

        // Arrange
        res.Should().BeFailure();
        _mockHttp.VerifyNoOutstandingExpectation();
        _logger.CheckLogWarning("Unauthorized!");
    }

    [Fact]
    public async Task Get_koms_throw_exception_when_something_went_wrong()
    {
        // Arrange
        _mockHttp.Expect(HttpMethod.Get, GetKomsUrl(1))
            .Throw(new Exception("Something went wrong"));

        // Act
        Func<Task> action = () => _athleteApi.GetKomsAsync(TEST_ATHLETE_ID, TEST_VALID_TOKEN);

        // Assert
        await action.Should().ThrowAsync<Exception>();
    }

    private string GetKomsUrl(int page)
    {
        return $"https://www.strava.com/api/v3/athletes/{TEST_ATHLETE_ID}/koms?per_page=200&page={page}"; // 200 max possible
    }
    #endregion
}
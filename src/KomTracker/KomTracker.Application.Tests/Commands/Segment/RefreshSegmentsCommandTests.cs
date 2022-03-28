using FluentAssertions;
using FluentResults;
using KomTracker.Application.Commands.Tracking;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Models.Configuration;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Token;
using KomTracker.Domain.Errors.Athlete;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils.Tests.Logging;
using Xunit;
using IStravaSegmentService = KomTracker.Application.Interfaces.Services.Strava.ISegmentService;

namespace KomTracker.Application.Tests.Commands.Segment;
public class RefreshSegmentsCommandTests
{
    #region TestData
    private const int TEST_MASTER_STRAVA_ATHLETE_ID = 66;
    private const string TEST_TOKEN_VALID = "token123";
    #endregion

    private readonly IKOMUnitOfWork _komUoW;
    private readonly ILogger<RefreshSegmentsCommandHandler> _logger;
    private readonly ApplicationConfiguration _applicationConfiguration;
    private readonly ISegmentService _segmentService;
    private readonly IAthleteService _athleteService;
    private readonly IStravaSegmentService _stravaSegmentService;

    private readonly CancellationToken _cancellationToken;
    private readonly RefreshSegmentsCommandHandler _refreshSegmentsCommandHandler;

    public RefreshSegmentsCommandTests(ITestLogger<RefreshSegmentsCommandHandler> logger)
    {
        _logger = logger;
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _applicationConfiguration = Substitute.For<ApplicationConfiguration>();
        _segmentService = Substitute.For<ISegmentService>();
        _athleteService = Substitute.For<IAthleteService>();
        _stravaSegmentService = Substitute.For<IStravaSegmentService>();

        _cancellationToken = new CancellationTokenSource().Token;

        _refreshSegmentsCommandHandler = new RefreshSegmentsCommandHandler(_komUoW, _logger, _applicationConfiguration, _segmentService, _athleteService, _stravaSegmentService);

        PrepareMocks();
    }

    private void PrepareMocks()
    {
        _applicationConfiguration.MasterStravaAthleteId = TEST_MASTER_STRAVA_ATHLETE_ID;

        _athleteService.GetValidTokenAsync(TEST_MASTER_STRAVA_ATHLETE_ID).Returns(Result.Ok(new TokenEntity
        {
            AccessToken = TEST_TOKEN_VALID
        }));
    }

    [Fact]
    public async Task Refresh_segments_use_master_strava_athlete_id_from_configuration()
    {
        // Act 
        var res = await _refreshSegmentsCommandHandler.Handle(new RefreshSegmentsCommand(), _cancellationToken);

        // Assert
        await _athleteService.Received().GetValidTokenAsync(TEST_MASTER_STRAVA_ATHLETE_ID);
    }

    [Fact]
    public async Task Refresh_segments_throws_when_no_token()
    {
        // Arrange
        _athleteService.GetValidTokenAsync(TEST_MASTER_STRAVA_ATHLETE_ID).Returns(Result.Fail<TokenEntity>(new GetValidTokenError(GetValidTokenError.NoTokenInDB)));

        // Act 
        var action = async () => await _refreshSegmentsCommandHandler.Handle(new RefreshSegmentsCommand(), _cancellationToken);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }
}

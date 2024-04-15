using AutoFixture;
using AutoMapper;
using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Domain.Entities.Token;
using NSubstitute;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using ApiModel = Strava.API.Client.Model;
using MoreLinq;
using KomTracker.Domain.Entities.Segment;
using KomTracker.Application.Interfaces.Services.Strava;
using KomTracker.Infrastructure.Strava.Services;

namespace KomTracker.Infrastructure.Tests.Strava.Services;

public class AthleteServiceTests
{
    #region TestData
    private int TestAthleteId = 1;

    private TokenEntity TestValidToken => new TokenEntity
    {
        ExpiresAt = DateTime.UtcNow.AddMinutes(60)
    };

    private TokenEntity TestInvalidToken => new TokenEntity
    {
        ExpiresAt = DateTime.UtcNow.AddMinutes(-60)
    };

    #endregion

    private readonly IMapper _mapper;
    private readonly IAthleteApi _athleteApi;
    private readonly IClubApi _clubApi;

    private readonly AthleteService _athleteService;

    public AthleteServiceTests()
    {
        _mapper = Substitute.For<IMapper>();
        _athleteApi = Substitute.For<IAthleteApi>();
        _clubApi = Substitute.For<IClubApi>();

        _athleteService = new AthleteService(_mapper, _athleteApi, _clubApi);
    }

    #region Get athlete koms
    [Fact]
    public async Task Get_athlete_koms_calls_api_and_returns_only_public_koms()
    {
        // Arrange
        var effortsCount = 5;
        var privateCount = 2;
        var fixture = new Fixture();
        fixture.Customize<SegmentEffortEntity>(x => x.Without(p => p.KomSummaries));

        var apiKoms = fixture.CreateMany<ApiModel.Segment.SegmentEffortDetailedModel>(effortsCount);
        var expectedKoms = new List<(SegmentEffortEntity, SegmentEntity)>();
        apiKoms.ForEach((x, i) => {                
            if (!(x.Segment.Private = i < privateCount))
            {
                var segmentEffort = fixture.Create<SegmentEffortEntity>();
                var segment = fixture.Create<SegmentEntity>();
                _mapper.Map<SegmentEffortEntity>(x).Returns(segmentEffort);
                _mapper.Map<SegmentEntity>(x.Segment).Returns(segment);

                expectedKoms.Add((segmentEffort, segment));
            }
        });

        var token = TestValidToken.AccessToken;
        _athleteApi.GetKomsAsync(TestAthleteId, token).Returns(Result.Ok(apiKoms.AsEnumerable()));

        // Act
        var res = await _athleteService.GetAthleteKomsAsync(TestAthleteId, token);

        // Assert
        res.Should().BeSuccess();
        var actualKoms = res.Value;

        actualKoms.Should().BeEquivalentTo(expectedKoms);
    }

    [Theory]
    [InlineData(ApiModel.Segment.Error.GetKomsError.Unauthorized, GetAthleteKomsError.Unauthorized)]
    [InlineData(ApiModel.Segment.Error.GetKomsError.TooManyRequests, GetAthleteKomsError.TooManyRequests)]
    [InlineData(ApiModel.Segment.Error.GetKomsError.UnknownError, GetAthleteKomsError.UnknownError)]
    public async Task Get_athlete_koms_passes_error(string apiError, string serviceError)
    {
        // Arrange
        var token = TestInvalidToken.AccessToken;
        _athleteApi.GetKomsAsync(TestAthleteId, token).Returns(Result.Fail<IEnumerable<ApiModel.Segment.SegmentEffortDetailedModel>>(new ApiModel.Segment.Error.GetKomsError(apiError)));

        // Act
        var res = await _athleteService.GetAthleteKomsAsync(TestAthleteId, token);

        // Assert
        res.Should().BeFailure();
        res.HasError<GetAthleteKomsError>(x => x.Message == serviceError).Should().BeTrue();
    }
    #endregion
}
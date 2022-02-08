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
using System.Text;
using System.Threading.Tasks;
using Xunit;
using ApiModel = Strava.API.Client.Model;
using MoreLinq;
using KomTracker.Domain.Entities.Segment;
using KomTracker.Application.Interfaces.Services.Strava;
using KomTracker.Infrastructure.Strava.Services;
using KomTracker.Application.Errors.Strava.Segment;

namespace KomTracker.Infrastructure.Tests.Strava.Services;

public class SegmentServiceTests
{
    #region TestData
    private const int TEST_SEGMENT_ID = 1;
    private const int TEST_SEGMENT_NOT_EXISTS_ID = 404;
    private const string TEST_TOKEN_VALID = "token123";
    private const string TEST_TOKEN_INVALID = "tokeninvalid123";
    #endregion

    private readonly IMapper _mapper;
    private readonly ISegmentApi _segmentApi;

    private readonly SegmentService _segmentService;

    public SegmentServiceTests()
    {
        _mapper = Substitute.For<IMapper>();
        _segmentApi = Substitute.For<ISegmentApi>();

        _segmentService = new SegmentService(_mapper, _segmentApi);
    }

    #region Get segment
    [Fact]
    public async Task Get_segment_call_api_and_returns_mapped_segment()
    {
        // Arrange
        var fixture = new Fixture();
        var apiSegment = fixture.Create<ApiModel.Segment.SegmentDetailedModel>();
        var expectedSegment = fixture.Create<SegmentEntity>();

        _segmentApi.GetSegmentAsync(TEST_SEGMENT_ID, TEST_TOKEN_VALID).Returns(Result.Ok(apiSegment));
        _mapper.Map<SegmentEntity>(apiSegment).Returns(expectedSegment);

        // Act
        var res = await _segmentService.GetSegmentAsync(TEST_SEGMENT_ID, TEST_TOKEN_VALID);

        // Assert
        res.Should().BeSuccess();
        var actualSegment = res.Value;
        actualSegment.Should().Be(expectedSegment);
    }

    [Theory]
    [InlineData(ApiModel.Segment.Error.GetSegmentError.Unauthorized, GetSegmentError.Unauthorized)]
    [InlineData(ApiModel.Segment.Error.GetSegmentError.NotFound, GetSegmentError.NotFound)]
    [InlineData(ApiModel.Segment.Error.GetSegmentError.UnknownError, GetSegmentError.UnknownError)]
    public async Task Get_segment_pass_error(string apiError, string serviceError)
    {
        // Arrange
        _segmentApi.GetSegmentAsync(TEST_SEGMENT_ID, TEST_TOKEN_VALID).Returns(Result.Fail<ApiModel.Segment.SegmentDetailedModel>(new ApiModel.Segment.Error.GetSegmentError(apiError)));

        // Act
        var res = await _segmentService.GetSegmentAsync(TEST_SEGMENT_ID, TEST_TOKEN_VALID);

        // Assert
        res.Should().BeFailure();
        res.HasError<GetSegmentError>(x => x.Message == serviceError).Should().BeTrue();
    }
    #endregion
}

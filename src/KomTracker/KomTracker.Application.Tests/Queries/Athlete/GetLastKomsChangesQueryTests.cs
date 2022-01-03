using AutoFixture;
using FluentAssertions;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Queries.Athlete;
using KomTracker.Application.Services;
using KomTracker.Application.Tests.Common;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KomTracker.Application.Tests.Queries.Athlete;

public class GetLastKomsChangesQueryTests
{
    private const int TestAthleteId = 1;

    private readonly ISegmentService _segmentService;
    private readonly CancellationToken _cancellationToken;
    private readonly GetLastKomsChangesQueryHandler _getLastKomsChangesQueryHandler;

    public GetLastKomsChangesQueryTests()
    {
        _segmentService = Substitute.For<ISegmentService>();
        _cancellationToken = new CancellationTokenSource().Token;

        _getLastKomsChangesQueryHandler = new GetLastKomsChangesQueryHandler(_segmentService);
    }

    [Fact]
    public async Task Get_all_koms_query_call_segment_service()
    {
        // Arrange
        var fixture = FixtureHelper.GetTestFixture();
        var dateFrom = DateTime.Today;
        var efforts = fixture.CreateMany<EffortModel>(5);
        _segmentService.GetLastKomsChangesAsync(TestAthleteId, dateFrom).Returns(efforts);

        // Act
        var res = await _getLastKomsChangesQueryHandler.Handle(new GetLastKomsChangesQuery { AthleteId = TestAthleteId, DateFrom = dateFrom }, _cancellationToken);

        // Assert
        res.Should().BeEquivalentTo(efforts);
    }
}

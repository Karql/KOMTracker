using AutoFixture;
using FluentAssertions;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
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
using static MoreLinq.Extensions.ForEachExtension;

namespace KomTracker.Application.Tests.Queries.Athlete;

public class GetAllKomsQueryTests
{
    private const int TestAthleteId = 1;

    private readonly ISegmentService _segmentService;
    private readonly IKOMUnitOfWork _komUoW;
    private readonly CancellationToken _cancellationToken;
    private readonly GetAllKomsQueryHandler _getAllKomsQueryHandler;

    public GetAllKomsQueryTests()
    {
        _segmentService = Substitute.For<ISegmentService>();
        _komUoW = Substitute.For<IKOMUnitOfWork>();
        _komUoW.GetRepository<IAthleteRepository>().Returns(Substitute.For<IAthleteRepository>());
        _cancellationToken = new CancellationTokenSource().Token;

        _getAllKomsQueryHandler = new GetAllKomsQueryHandler(_segmentService, _komUoW);
    }

    [Fact]
    public async Task Get_all_koms_query_calls_segment_service_and_returns_only_koms()
    {
        // Arrange
        var fixture = FixtureHelper.GetTestFixture();
        var efforts = fixture.CreateMany<EffortModel>(5);
        efforts.ForEach((x, i) => x.SummarySegmentEffort.Kom = i % 2 == 0);
        _segmentService.GetLastKomsSummaryEffortsAsync(TestAthleteId).Returns(efforts);

        // Act
        var res = await _getAllKomsQueryHandler.Handle(new GetAllKomsQuery { AthleteId = TestAthleteId }, _cancellationToken);

        // Assert
        res.Should().BeEquivalentTo(efforts.Where(x => x.SummarySegmentEffort.Kom));
    }
}

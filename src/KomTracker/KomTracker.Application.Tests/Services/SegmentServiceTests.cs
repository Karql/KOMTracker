using AutoFixture;
using FluentAssertions;
using FluentResults;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Errors.Strava.Athlete;
using KomTracker.Application.Errors.Strava.Token;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Services;
using KomTracker.Application.Tests.Persistence;
using KomTracker.Domain.Entities.Segment;
using KomTracker.Domain.Entities.Token;
using KomTracker.Domain.Errors.Athlete;
using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Tests.Logging;
using Utils.Tests.UserManager;
using Utils.UnitOfWork.Abstract;
using Xunit;
using Utils.Tests.Extensions;
using IStravaAthleteService = KomTracker.Application.Interfaces.Services.Strava.IAthleteService;
using KomTracker.Application.Tests.Common;

namespace KomTracker.Application.Tests.Infrastructure.Services;

public class SegmentServiceTests
{
    #region TestData
    private const int TestAthleteId = 1;
    private const string TestToken = "token123";
    #endregion

    private readonly IKOMUnitOfWork _komUoW;
    private readonly ITestLogger<SegmentService> _logger;
    private readonly IStravaAthleteService _stravaAthleteService;
    private readonly ISegmentRepository _segmentRepository;

    private readonly SegmentService _segmentService;

    public SegmentServiceTests(ITestLogger<SegmentService> logger)
    {
        _segmentRepository = Substitute.For<ISegmentRepository>();
        _komUoW = new TestKOMUnitOfWork(new Dictionary<Type, IRepository>
        {
            { typeof(ISegmentRepository), _segmentRepository }
        });
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stravaAthleteService = Substitute.For<IStravaAthleteService>();

        _segmentService = new SegmentService(_komUoW, _logger, _stravaAthleteService);
    }

    #region Get actual koms
    [Fact]
    public async Task Get_actual_koms_calls_strava_service()
    {
        // Arange
        var fixture = FixtureHelper.GetTestFixture();
        var koms = fixture.CreateMany<(SegmentEffortEntity, SegmentEntity)>(2);

        _stravaAthleteService.GetAthleteKomsAsync(TestAthleteId, TestToken).Returns(Result.Ok(koms));

        // Act
        var res = await _segmentService.GetActualKomsAsync(TestAthleteId, TestToken);

        // Arrange
        res.Should().BeEquivalentTo(koms);
    }

    [Fact]
    public async Task Get_actual_koms_returns_null_when_failed()
    {
        // Arange
        _stravaAthleteService.GetAthleteKomsAsync(TestAthleteId, TestToken).Returns(
            Result.Fail<IEnumerable<(SegmentEffortEntity, SegmentEntity)>>(new GetAthleteKomsError(GetAthleteKomsError.UnknownError)));

        // Act
        var res = await _segmentService.GetActualKomsAsync(TestAthleteId, TestToken);

        // Arrange
        res.Should().BeNull();
    }
    #endregion

    #region Get last koms summary efforts
    [Fact]
    public async Task Get_last_koms_summary_efforts_calls_repo()
    {
        // Arrange
        var fixture = FixtureHelper.GetTestFixture();
        var lastEfforts = fixture.CreateMany<EffortModel>(2);
        _segmentRepository.GetLastKomsSummaryEffortsAsync(TestAthleteId).Returns(lastEfforts);

        // Act
        var res = await _segmentService.GetLastKomsSummaryEffortsAsync(TestAthleteId);

        // Assert
        res.Should().BeEquivalentTo(lastEfforts);
    }

    [Fact]
    public async Task Get_last_koms_summary_efforts_returns_empty_list_when_no_data_in_db()
    {
        // Arrange
        _segmentRepository.GetLastKomsSummaryEffortsAsync(TestAthleteId).Returns((IEnumerable<EffortModel>)null);

        // Act
        var res = await _segmentService.GetLastKomsSummaryEffortsAsync(TestAthleteId);

        // Assert
        res.Should().NotBeNull();
        res.Any().Should().BeFalse();
    }
    #endregion

    #region Get last koms changes
    [Fact]
    public async Task Get_last_koms_changes_calls_repo()
    {
        // Arrange
        var dateFrom = DateTime.Today;
        var fixture = FixtureHelper.GetTestFixture();
        var lastChanges = fixture.CreateMany<EffortModel>(2);
        _segmentRepository.GetLastKomsChangesAsync(TestAthleteId, dateFrom).Returns(lastChanges);

        // Act
        var res = await _segmentService.GetLastKomsChangesAsync(TestAthleteId, dateFrom);

        // Assert
        res.Should().BeEquivalentTo(lastChanges);
    }

    [Fact]
    public async Task Get_last_koms_changes_returns_empty_list_when_no_data_in_db()
    {
        // Arrange
        var dateFrom = DateTime.Today;
        _segmentRepository.GetLastKomsChangesAsync(TestAthleteId, dateFrom).Returns((IEnumerable<EffortModel>)null);

        // Act
        var res = await _segmentService.GetLastKomsChangesAsync(TestAthleteId, dateFrom);

        // Assert
        res.Should().NotBeNull();
        res.Any().Should().BeFalse();
    }

    [Fact]
    public async Task Get_last_koms_changes_returns_last_change_per_segment()
    {
        // Arrange
        var dateFrom = DateTime.Today;
        var lastChanges = new EffortModel[]
        {
            new EffortModel
            {
                SegmentEffort = new() { SegmentId = 1, StartDate = DateTime.Today.AddMinutes(20) },
                SummarySegmentEffort = new() { }
            },
            new EffortModel
            {
                SegmentEffort = new() { SegmentId = 1, StartDate = DateTime.Today.AddMinutes(15) },
                SummarySegmentEffort = new() { }
            },
            new EffortModel
            {
                SegmentEffort = new() { SegmentId = 2, StartDate = DateTime.Today.AddMinutes(10) },
                SummarySegmentEffort = new() { }
            },
            new EffortModel
            {
                SegmentEffort = new() { SegmentId = 2, StartDate = DateTime.Today.AddMinutes(15) },
                SummarySegmentEffort = new() { }
            },
        };

        _segmentRepository.GetLastKomsChangesAsync(TestAthleteId, dateFrom).Returns(lastChanges);

        var expectedChanges = new EffortModel[]
        {
            lastChanges[0],
            lastChanges[3]
        };

        // Act
        var res = await _segmentService.GetLastKomsChangesAsync(TestAthleteId, dateFrom);

        // Assert
        res.Should().BeEquivalentTo(expectedChanges);
    }
    #endregion

    #region Compare efforts
    [Fact]
    public void Compare_efforts_returns_correct_compared_model()
    {
        // Arrange
        var lastKomsEffots = new SegmentEffortEntity[]
        {
            new() { Id = 1, SegmentId = 1 },
            new() { Id = 2, SegmentId = 2 },
            new() { Id = 3, SegmentId = 3 },
        };

        var actualKomsEffots = new SegmentEffortEntity[]
        {
            // No Id 1 - lost kom
            new() { Id = 2, SegmentId = 2 }, // no change 
            new() { Id = 4, SegmentId = 3 }, // improved kom
            new() { Id = 5, SegmentId = 4 }, // new kom
        };

        // Act
        var comparedEffots = _segmentService.CompareEfforts(actualKomsEffots, lastKomsEffots);

        // Assert
        comparedEffots.Should().NotBeNull();
        comparedEffots.KomsCount.Should().Be(3);
        comparedEffots.NewKomsCount.Should().Be(1);
        comparedEffots.ImprovedKomsCount.Should().Be(1);
        comparedEffots.LostKomsCount.Should().Be(1);
        comparedEffots.AnyChanges.Should().BeTrue();

        var efforts = comparedEffots.Efforts;
        efforts.Should().NotBeNull();
        efforts.Count.Should().Be(4); // 3 kom + 1 lost kom

        var lostKom = efforts.FirstOrDefault(x => x.SegmentEffort.Id == 1);
        var kom = efforts.FirstOrDefault(x => x.SegmentEffort.Id == 2);
        var improvedKom = efforts.FirstOrDefault(x => x.SegmentEffort.Id == 4);
        var newKom = efforts.FirstOrDefault(x => x.SegmentEffort.Id == 5);

        AssertComparedEffortLink(lostKom, true, false, false, false);
        AssertComparedEffortLink(kom, false, true, false, false);
        AssertComparedEffortLink(improvedKom, false, true, false, true);
        AssertComparedEffortLink(newKom, false, true, true, false);
    }

    public void Compare_efforts_returns_no_changes()
    {
        // Arrange
        var lastKomsEffots = new SegmentEffortEntity[]
        {
            new() { Id = 1, SegmentId = 1 },
            new() { Id = 2, SegmentId = 2 },
            new() { Id = 3, SegmentId = 3 },
        };

        var actualKomsEffots = new SegmentEffortEntity[]
        {
            new() { Id = 1, SegmentId = 1 },
            new() { Id = 2, SegmentId = 2 },
            new() { Id = 3, SegmentId = 3 },
        };

        // Act
        var comparedEffots = _segmentService.CompareEfforts(actualKomsEffots, lastKomsEffots);

        // Assert
        comparedEffots.Should().NotBeNull();
        comparedEffots.KomsCount.Should().Be(3);
        comparedEffots.NewKomsCount.Should().Be(0);
        comparedEffots.ImprovedKomsCount.Should().Be(0);
        comparedEffots.LostKomsCount.Should().Be(0);
        comparedEffots.AnyChanges.Should().BeFalse();
    }

    private void AssertComparedEffortLink(EffortModel model, bool lostKom, bool kom, bool newKom, bool improvedKom)
    {
        var (effort, link) = (model.SegmentEffort, model.SummarySegmentEffort);

        link.SegmentEffortId.Should().Be(effort.Id);

        link.LostKom.Should().Be(lostKom);
        link.Kom.Should().Be(kom);
        link.NewKom.Should().Be(newKom);
        link.ImprovedKom.Should().Be(improvedKom);
    }
    #endregion

    #region Add_new_koms_summary
    [Fact]
    public async Task Add_new_koms_summary_calls_repositories()
    {
        //Arrange
        var comparedEfforts = new ComparedEffortsModel
        {
            Efforts = new List<EffortModel>()
            {
                new()
                {
                    SegmentEffort = new(),
                    SummarySegmentEffort = new() { LostKom = true }
                },
                new()
                {
                    SegmentEffort = new(),
                    SummarySegmentEffort = new() { Kom = true }
                },
                new()
                {
                    SegmentEffort = new(),
                    SummarySegmentEffort = new() { Kom = true, NewKom = true }
                },
                new()
                {
                    SegmentEffort = new(),
                    SummarySegmentEffort = new() { Kom = true, ImprovedKom = true }
                },
            },
            KomsCount = 3,
            NewKomsCount = 1,
            ImprovedKomsCount = 1,
            LostKomsCount = 1
        };

        // Act
        await _segmentService.AddNewKomsSummaryWithEffortsAsync(TestAthleteId, comparedEfforts);

        // Assert
        var addKomsSummaryAsyncCalls = _segmentRepository.ReceivedCalls().FilterByName(nameof(_segmentRepository.AddKomsSummaryAsync));
        addKomsSummaryAsyncCalls.Count().Should().Be(1);
        var addKomsSummaryAsyncCall = addKomsSummaryAsyncCalls.First();
        var komsSummaryArg = addKomsSummaryAsyncCall.GetArgument<KomsSummaryEntity>();

        komsSummaryArg.AthleteId.Should().Be(TestAthleteId);
        komsSummaryArg.Koms.Should().Be(comparedEfforts.KomsCount);
        komsSummaryArg.NewKoms.Should().Be(comparedEfforts.NewKomsCount);
        komsSummaryArg.ImprovedKoms.Should().Be(comparedEfforts.ImprovedKomsCount);
        komsSummaryArg.LostKoms.Should().Be(comparedEfforts.LostKomsCount);

        var addKomsSummariesSegmentEffortsAsyncCalls = _segmentRepository.ReceivedCalls().FilterByName(nameof(_segmentRepository.AddKomsSummariesSegmentEffortsAsync));
        addKomsSummariesSegmentEffortsAsyncCalls.Count().Should().Be(1);
        var addKomsSummariesSegmentEffortsAsyncCall = addKomsSummariesSegmentEffortsAsyncCalls.First();
        var komsSummariesSegmentEffortsArgs = addKomsSummariesSegmentEffortsAsyncCall.GetArgument<IEnumerable<KomsSummarySegmentEffortEntity>>();

        komsSummariesSegmentEffortsArgs.Should().BeEquivalentTo(comparedEfforts.Efforts.Select(x => x.SummarySegmentEffort));
        komsSummariesSegmentEffortsArgs.All(x => x.KomsSummary == komsSummaryArg).Should().BeTrue();
    }
    #endregion

    #region Get segments to refresh
    [Fact]
    public async Task Get_segments_to_refresh_calls_repo()
    {
        // Arrange
        var fixture = FixtureHelper.GetTestFixture();
        var segments = fixture.CreateMany<SegmentEntity>(2);
        _segmentRepository.GetSegmentsToRefreshAsync(Arg.Any<int>(), Arg.Any<TimeSpan?>()).Returns(segments);

        // Act
        var res = await _segmentService.GetSegmentsToRefreshAsync();

        // Assert
        res.Should().BeEquivalentTo(segments);
    }
    #endregion

    #region Update segments
    [Fact]
    public async Task Update_segments_calls_repo()
    {
        // Arrange
        var fixture = FixtureHelper.GetTestFixture();
        var segments = fixture.CreateMany<SegmentEntity>(2);

        // Act
        await _segmentService.UpdateSegmentsAsync(segments);

        // Assert
        await _segmentRepository.Received().UpdateSegmentsAsync(Arg.Is<IEnumerable<SegmentEntity>>(x =>
            x.SequenceEqual(segments)
        ));
    }
    #endregion
}
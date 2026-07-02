using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Services;
using KomTracker.Application.Tests.Persistence;
using KomTracker.Domain.Entities.Segment;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Tests.Logging;
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
    public async Task Get_last_koms_summary_efforts_returns_null_when_no_data_in_db()
    {
        // Arrange
        _segmentRepository.GetLastKomsSummaryEffortsAsync(TestAthleteId).Returns((IEnumerable<EffortModel>)null);

        // Act
        var res = await _segmentService.GetLastKomsSummaryEffortsAsync(TestAthleteId);

        // Assert
        res.Should().BeNull();
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
        _segmentRepository.GetLastKomsChangesAsync(Arg.Is<IEnumerable<int>>(x => x.First() == TestAthleteId), dateFrom, null).Returns(lastChanges);

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
        _segmentRepository.GetLastKomsChangesAsync(Arg.Is<IEnumerable<int>>(x => x.First() == TestAthleteId), dateFrom, null).Returns((IEnumerable<EffortModel>)null);

        // Act
        var res = await _segmentService.GetLastKomsChangesAsync(TestAthleteId, dateFrom);

        // Assert
        res.Should().NotBeNull();
        res.Any().Should().BeFalse();
    }
    #endregion

    #region Compare efforts
    [Fact]
    public void Compare_efforts_returns_correct_compared_model()
    {
        // Arrange
        var lastKomsEffots = new SegmentEffortEntity[]
        {
            new() { Id = 1, SegmentId = 1, ElapsedTime = 10 },
            new() { Id = 2, SegmentId = 2, ElapsedTime = 10 },
            new() { Id = 3, SegmentId = 3, ElapsedTime = 10 },
            new() { Id = 4, SegmentId = 4, ElapsedTime = 10 }
        };

        var actualKomsEffots = new SegmentEffortEntity[]
        {
            // No Id 1 - lost kom
            new() { Id = 2, SegmentId = 2, ElapsedTime = 10 }, // no change 
            new() { Id = 5, SegmentId = 3, ElapsedTime = 5 },  // improved kom (time has to be better to mark as improved)
            new() { Id = 6, SegmentId = 4, ElapsedTime = 10 }, // no change (e.g. refreshed efforts, change privacy zone creates same efforts with diffrent ids)
            new() { Id = 7, SegmentId = 5, ElapsedTime = 10 }, // new kom
        };

        // Act
        var comparedEffots = _segmentService.CompareEfforts(actualKomsEffots, lastKomsEffots, false);

        // Assert
        comparedEffots.Should().NotBeNull();
        comparedEffots.KomsCount.Should().Be(4);
        comparedEffots.NewKomsCount.Should().Be(1);
        comparedEffots.ImprovedKomsCount.Should().Be(1);
        comparedEffots.LostKomsCount.Should().Be(1);
        comparedEffots.PreviousKomsCount.Should().Be(lastKomsEffots.Length);
        comparedEffots.AnyChanges.Should().BeTrue();
        comparedEffots.FirstCompare.Should().BeFalse();

        var efforts = comparedEffots.Efforts;
        efforts.Should().NotBeNull();
        efforts.Count.Should().Be(5); // 4 kom + 1 lost kom

        var lostKom = efforts.FirstOrDefault(x => x.SegmentEffort.Id == 1);
        var kom = efforts.FirstOrDefault(x => x.SegmentEffort.Id == 2);
        var improvedKom = efforts.FirstOrDefault(x => x.SegmentEffort.Id == 5);
        var komWithDiffrentEffortId = efforts.FirstOrDefault(x => x.SegmentEffort.Id == 6);
        var newKom = efforts.FirstOrDefault(x => x.SegmentEffort.Id == 7);

        AssertComparedEffortLink(lostKom, true, false, false, false);
        AssertComparedEffortLink(kom, false, true, false, false);
        AssertComparedEffortLink(improvedKom, false, true, false, true);
        AssertComparedEffortLink(komWithDiffrentEffortId, false, true, false, false);
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
        var comparedEffots = _segmentService.CompareEfforts(actualKomsEffots, lastKomsEffots, false);

        // Assert
        comparedEffots.Should().NotBeNull();
        comparedEffots.KomsCount.Should().Be(3);
        comparedEffots.NewKomsCount.Should().Be(0);
        comparedEffots.ImprovedKomsCount.Should().Be(0);
        comparedEffots.LostKomsCount.Should().Be(0);
        comparedEffots.AnyChanges.Should().BeFalse();
        comparedEffots.FirstCompare.Should().BeFalse();
    }

    [Fact]
    public void Compare_efforts_does_not_mark_new_koms_in_first_compare()
    {
        // Arrange
        var lastKomsEffots = new SegmentEffortEntity[0];

        var actualKomsEffots = new SegmentEffortEntity[]
        {
            new() { Id = 1, SegmentId = 1 },
            new() { Id = 2, SegmentId = 2 },
            new() { Id = 3, SegmentId = 3 },
        };

        // Act
        var comparedEffots = _segmentService.CompareEfforts(actualKomsEffots, lastKomsEffots, true);

        // Assert
        comparedEffots.Should().NotBeNull();
        comparedEffots.KomsCount.Should().Be(3);
        comparedEffots.NewKomsCount.Should().Be(0);
        comparedEffots.ImprovedKomsCount.Should().Be(0);
        comparedEffots.LostKomsCount.Should().Be(0);
        comparedEffots.AnyChanges.Should().BeFalse(); // No changes for first compare
        comparedEffots.FirstCompare.Should().BeTrue();
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

    #region Check new koms are returned
    [InlineData(5, 3)]
    [InlineData(2, 0)]
    [InlineData(2, 2)]
    [Theory]
    public async Task Check_new_koms_are_returned_check_in_db_and_update_model_correctly(int newKomsCount, int returnedKomsCount)
    {
        // Arrange
        var newIds = Enumerable.Range(1, newKomsCount).Select(x => (long)x).ToHashSet();
        var returnedIds = Enumerable.Range(1, returnedKomsCount).Select(x => (long)x).ToHashSet();

        var comparedEfforts = new ComparedEffortsModel
        {
            KomsCount = newKomsCount,
            NewKomsCount = newKomsCount,
            Efforts = newIds.Select(id => new EffortModel
            {
                SegmentEffort = new SegmentEffortEntity { Id = id },
                SummarySegmentEffort = new KomsSummarySegmentEffortEntity { Kom = true, NewKom = true }
            }).ToList()
        };

        _segmentRepository.GetSegmentEffortsAsync(Arg.Is<HashSet<long>>(x => x.SequenceEqual(newIds)))
            .Returns(returnedIds.Select(id => new SegmentEffortEntity { Id = id }).ToList());

        // Act
        await _segmentService.CheckNewKomsAreReturnedAsync(comparedEfforts);

        // Assert
        await _segmentRepository.Received().GetSegmentEffortsAsync(Arg.Is<HashSet<long>>(x => x.SequenceEqual(newIds)));

        comparedEfforts.KomsCount.Should().Be(newKomsCount); // total koms should remain the same
        comparedEfforts.NewKomsCount.Should().Be(newKomsCount - returnedKomsCount);
        comparedEfforts.ReturnedKomsCount.Should().Be(returnedKomsCount);

        var returnedKoms = comparedEfforts.Efforts.Where(x => x.SummarySegmentEffort.ReturnedKom == true).ToList();
        returnedKoms.Count().Should().Be(returnedKomsCount);
        returnedKoms.Any(x => x.SummarySegmentEffort.NewKom == true).Should().BeFalse();
        returnedKoms.Select(x => x.SegmentEffort.Id).Should().BeEquivalentTo(returnedIds);
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
                new()
                {
                    SegmentEffort = new(),
                    SummarySegmentEffort = new() { Kom = true, ReturnedKom = true }
                },
            },
            KomsCount = 4,
            NewKomsCount = 1,
            ImprovedKomsCount = 1,
            LostKomsCount = 1,
            ReturnedKomsCount = 1
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
        komsSummaryArg.ReturnedKoms.Should().Be(comparedEfforts.ReturnedKomsCount);

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

    #region Resolve KOM takeovers
    private const long TakeoverSegment = 10;
    private const int TakeoverAthleteX = 1;
    private const int TakeoverAthleteOther = 2;

    private static KomTakeoverChangeModel TakeoverChange(KomChangeTypeEnum type, int athleteId, long segmentEffortId, int komsSummaryId, string? sex = "M", long segmentId = TakeoverSegment, DateTime trackDate = default)
        => new()
        {
            AthleteId = athleteId,
            Sex = sex,
            SegmentId = segmentId,
            SegmentEffortId = segmentEffortId,
            KomsSummaryId = komsSummaryId,
            TrackDate = trackDate,
            ChangeType = type
        };

    private ResolveTakeoversResult Resolve(
        IEnumerable<KomTakeoverChangeModel> summary,
        IEnumerable<KomTakeoverChangeModel> counterpart = null,
        IEnumerable<KomTakeoverEntity> active = null,
        ISet<long> existingTaken = null)
        => _segmentService.ResolveTakeovers(
            summary,
            counterpart ?? Enumerable.Empty<KomTakeoverChangeModel>(),
            active ?? Enumerable.Empty<KomTakeoverEntity>(),
            existingTaken ?? new HashSet<long>());

    [Fact]
    public void Resolve_takeovers_new_kom_paired_with_counterpart_lost_creates_takeover()
    {
        // Arrange - X gained, A lost the same segment (same sex)
        var takenAt = new DateTime(2026, 6, 30, 12, 0, 0, DateTimeKind.Utc);
        var summary = new[] { TakeoverChange(KomChangeTypeEnum.New, TakeoverAthleteX, segmentEffortId: 100, komsSummaryId: 1000, trackDate: takenAt) };
        var counterpart = new[] { TakeoverChange(KomChangeTypeEnum.Lost, TakeoverAthleteOther, segmentEffortId: 200, komsSummaryId: 999) };

        // Act
        var res = Resolve(summary, counterpart);

        // Assert
        res.NewTakeovers.Should().ContainSingle();
        var t = res.NewTakeovers.Single();
        t.TakenSegmentEffortId.Should().Be(100);
        t.LostSegmentEffortId.Should().Be(200);
        t.TakenKomsSummaryId.Should().Be(1000);
        t.LostKomsSummaryId.Should().Be(999);
        t.TrackDate.Should().Be(takenAt); // from the taking (gain) side
        t.Reverted.Should().BeFalse();
        res.RevertedTakeoverIds.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_takeovers_lost_kom_paired_with_counterpart_new_creates_takeover()
    {
        // Arrange - X lost, B gained
        var summary = new[] { TakeoverChange(KomChangeTypeEnum.Lost, TakeoverAthleteX, segmentEffortId: 200, komsSummaryId: 1000) };
        var counterpart = new[] { TakeoverChange(KomChangeTypeEnum.New, TakeoverAthleteOther, segmentEffortId: 100, komsSummaryId: 999) };

        // Act
        var res = Resolve(summary, counterpart);

        // Assert
        var t = res.NewTakeovers.Single();
        t.TakenSegmentEffortId.Should().Be(100); // B's effort
        t.LostSegmentEffortId.Should().Be(200);  // X's effort
        t.TakenKomsSummaryId.Should().Be(999);
        t.LostKomsSummaryId.Should().Be(1000);
    }

    [Fact]
    public void Resolve_takeovers_different_sex_does_not_create_takeover()
    {
        var summary = new[] { TakeoverChange(KomChangeTypeEnum.New, TakeoverAthleteX, 100, 1000, sex: "M") };
        var counterpart = new[] { TakeoverChange(KomChangeTypeEnum.Lost, TakeoverAthleteOther, 200, 999, sex: "F") };

        var res = Resolve(summary, counterpart);

        res.NewTakeovers.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_takeovers_null_sex_on_both_sides_is_treated_as_equal()
    {
        var summary = new[] { TakeoverChange(KomChangeTypeEnum.New, TakeoverAthleteX, 100, 1000, sex: null) };
        var counterpart = new[] { TakeoverChange(KomChangeTypeEnum.Lost, TakeoverAthleteOther, 200, 999, sex: null) };

        var res = Resolve(summary, counterpart);

        res.NewTakeovers.Should().ContainSingle();
    }

    [Fact]
    public void Resolve_takeovers_no_counterpart_does_not_create_takeover()
    {
        var summary = new[] { TakeoverChange(KomChangeTypeEnum.New, TakeoverAthleteX, 100, 1000) };

        var res = Resolve(summary);

        res.NewTakeovers.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_takeovers_multiple_counterparts_picks_newest_by_koms_summary_id()
    {
        var summary = new[] { TakeoverChange(KomChangeTypeEnum.New, TakeoverAthleteX, 100, 1000) };
        var counterpart = new[]
        {
            TakeoverChange(KomChangeTypeEnum.Lost, TakeoverAthleteOther, segmentEffortId: 200, komsSummaryId: 998),
            TakeoverChange(KomChangeTypeEnum.Lost, TakeoverAthleteOther, segmentEffortId: 300, komsSummaryId: 999) // newest
        };

        var res = Resolve(summary, counterpart);

        var t = res.NewTakeovers.Single();
        t.LostSegmentEffortId.Should().Be(300);
        t.LostKomsSummaryId.Should().Be(999);
    }

    [Fact]
    public void Resolve_takeovers_returned_kom_marks_matching_takeover_reverted()
    {
        // Arrange - X regains effort 200; a takeover lost via effort 200 exists
        var summary = new[] { TakeoverChange(KomChangeTypeEnum.Returned, TakeoverAthleteX, segmentEffortId: 200, komsSummaryId: 1000) };
        var active = new[]
        {
            new KomTakeoverEntity { Id = 5, TakenSegmentEffortId = 100, LostSegmentEffortId = 200 }
        };

        // Act
        var res = Resolve(summary, active: active);

        // Assert
        res.RevertedTakeoverIds.Should().BeEquivalentTo(new[] { 5 });
        res.NewTakeovers.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_takeovers_returned_kom_without_matching_effort_does_nothing()
    {
        var summary = new[] { TakeoverChange(KomChangeTypeEnum.Returned, TakeoverAthleteX, segmentEffortId: 999, komsSummaryId: 1000) };
        var active = new[]
        {
            new KomTakeoverEntity { Id = 5, TakenSegmentEffortId = 100, LostSegmentEffortId = 200 }
        };

        var res = Resolve(summary, active: active);

        res.RevertedTakeoverIds.Should().BeEmpty();
    }

    [Fact]
    public void Resolve_takeovers_existing_taken_effort_is_not_duplicated()
    {
        var summary = new[] { TakeoverChange(KomChangeTypeEnum.New, TakeoverAthleteX, segmentEffortId: 100, komsSummaryId: 1000) };
        var counterpart = new[] { TakeoverChange(KomChangeTypeEnum.Lost, TakeoverAthleteOther, 200, 999) };

        var res = Resolve(summary, counterpart, existingTaken: new HashSet<long> { 100 });

        res.NewTakeovers.Should().BeEmpty();
    }
    #endregion
}
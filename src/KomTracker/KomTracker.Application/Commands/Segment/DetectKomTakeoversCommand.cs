using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KomTracker.Application.Commands.Segment;

/// <summary>
/// Detects KOM takeovers for a single koms_summary (the "who took whose KOM" feature).
/// Always processes one summary - the incremental trigger passes the freshly created
/// summary id; backfill calls it per id.
/// </summary>
public class DetectKomTakeoversCommand : IRequest<Result>
{
    public int KomsSummaryId { get; set; }

    /// <summary>Backward window for matching the counterpart change (default 24h).</summary>
    public TimeSpan Window { get; set; } = TimeSpan.FromHours(24);
}

public class DetectKomTakeoversCommandHandler : IRequestHandler<DetectKomTakeoversCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ILogger<DetectKomTakeoversCommandHandler> _logger;
    private readonly ISegmentService _segmentService;

    public DetectKomTakeoversCommandHandler(IKOMUnitOfWork komUoW, ILogger<DetectKomTakeoversCommandHandler> logger, ISegmentService segmentService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
    }

    public async Task<Result> Handle(DetectKomTakeoversCommand request, CancellationToken cancellationToken)
    {
        var segmentRepo = _komUoW.GetRepository<ISegmentRepository>();

        var summaryChanges = (await segmentRepo.GetSummaryChangesForTakeoverAsync(request.KomsSummaryId)).ToList();
        if (summaryChanges.Count == 0)
        {
            return Result.Ok(); // nothing to detect (no changes or summary removed)
        }

        var athleteId = summaryChanges[0].AthleteId;

        // Counterpart candidates only for segments with a New/Lost change.
        var pairableSegmentIds = summaryChanges
            .Where(x => x.ChangeType is KomChangeTypeEnum.New or KomChangeTypeEnum.Lost)
            .Select(x => x.SegmentId)
            .Distinct()
            .ToList();

        var counterpartChanges = pairableSegmentIds.Count > 0
            ? await segmentRepo.GetCounterpartChangesAsync(request.KomsSummaryId, pairableSegmentIds, request.Window, athleteId)
            : Enumerable.Empty<KomTakeoverChangeModel>();

        // Returned koms revert a prior takeover matched by lost effort.
        var returnedEffortIds = summaryChanges
            .Where(x => x.ChangeType == KomChangeTypeEnum.Returned)
            .Select(x => x.SegmentEffortId)
            .ToList();

        var activeTakeovers = (await segmentRepo.GetActiveTakeoversByLostEffortAsync(returnedEffortIds)).ToList();

        // DB unique on taken_segment_effort_id is the cross-run idempotency guard,
        // so the resolver can produce freely here.
        var resolved = _segmentService.ResolveTakeovers(
            summaryChanges, counterpartChanges, activeTakeovers, new HashSet<long>());

        if (resolved.RevertedTakeoverIds.Count > 0)
        {
            foreach (var takeover in activeTakeovers.Where(x => resolved.RevertedTakeoverIds.Contains(x.Id)))
            {
                takeover.Reverted = true;
            }
        }

        await segmentRepo.AddTakeoversIfNotExistsAsync(resolved.NewTakeovers);
        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}

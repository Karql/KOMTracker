using KomTracker.Application.Commands.Segment;
using KomTracker.Application.Commands.Stats;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Services.Identity;
using KomTracker.Application.Interfaces.Services.Mail;
using KomTracker.Application.Models.Mail;
using KomTracker.Application.Models.Segment;
using KomTracker.Domain.Entities.Athlete;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Notifications.Tracking;
public class TrackKomsCompletedNotification : INotification
{
    public AthleteEntity Athlete { get; set; } = default!;
    public ComparedEffortsModel ComparedEfforts { get; set; } = default!;
    public int KomsSummaryId { get; set; }
}

public class TrackKomsCompletedNotificationSendEmailHandler : INotificationHandler<TrackKomsCompletedNotification>
{
    private readonly ILogger _logger;
    private readonly IUserService _userService;
    private readonly IMailService _mailService;    

    public TrackKomsCompletedNotificationSendEmailHandler(ILogger<TrackKomsCompletedNotificationSendEmailHandler> logger, IUserService userService, IMailService mailService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
    }

    public async Task Handle(TrackKomsCompletedNotification notification, CancellationToken cancellationToken)
    {
        var logPrefix = $"{nameof(TrackKomsCompletedNotificationSendEmailHandler)} ";

        try
        {
            if (notification.ComparedEfforts.FirstCompare)
            {
                return; // Do not send mail for first tracking
            }

            var athleteId = notification.Athlete.AthleteId;
            var user = await _userService.GetUserAsync(athleteId);

            if (user == null)
            {
                _logger.LogWarning(logPrefix + "User not fount for athletedId: {athleteId}", athleteId);
                return;
            }

            if (user.EmailConfirmed && !string.IsNullOrEmpty(user.Email))
            {
                await _mailService.SendTrackKomsNotificationAsync(new SendTrackKomsNotificationParamsModel
                {
                    To = user.Email,
                    FirstName = notification.Athlete.FirstName,
                    ComparedEfforts = notification.ComparedEfforts
                });
            }
        }
        catch (Exception ex)
        {
            // Best-effort side effect - a failure must not break tracking or sibling handlers.
            _logger.LogError(ex, logPrefix + "failed for athlete {athleteId}", notification.Athlete.AthleteId);
        }
    }
}

public class TrackKomsCompletedNotificationRefreshStatsHandler : INotificationHandler<TrackKomsCompletedNotification>
{
    private readonly ILogger _logger;
    private readonly IMediator _medaitor;

    public TrackKomsCompletedNotificationRefreshStatsHandler(ILogger<TrackKomsCompletedNotificationRefreshStatsHandler> logger, IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _medaitor = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Handle(TrackKomsCompletedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await _medaitor.Send(new RefreshStatsCommand { AthleteId = notification.Athlete.AthleteId }, cancellationToken);
        }
        catch (Exception ex)
        {
            // Best-effort side effect - recoverable via admin/refresh-stats.
            _logger.LogError(ex, "{handler} failed for athlete {athleteId}", nameof(TrackKomsCompletedNotificationRefreshStatsHandler), notification.Athlete.AthleteId);
        }
    }
}

public class TrackKomsCompletedNotificationDetectTakeoversHandler : INotificationHandler<TrackKomsCompletedNotification>
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;

    public TrackKomsCompletedNotificationDetectTakeoversHandler(ILogger<TrackKomsCompletedNotificationDetectTakeoversHandler> logger, IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Handle(TrackKomsCompletedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(new DetectKomTakeoversCommand { KomsSummaryId = notification.KomsSummaryId }, cancellationToken);
        }
        catch (Exception ex)
        {
            // Best-effort side effect - recoverable via admin/detect-takeovers.
            _logger.LogError(ex, "{handler} failed for koms summary {komsSummaryId}", nameof(TrackKomsCompletedNotificationDetectTakeoversHandler), notification.KomsSummaryId);
        }
    }
}
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

    public Task Handle(TrackKomsCompletedNotification notification, CancellationToken cancellationToken)
    {
        return _medaitor.Send(new RefreshStatsCommand { AthleteId = notification.Athlete.AthleteId }, cancellationToken);
    }
}
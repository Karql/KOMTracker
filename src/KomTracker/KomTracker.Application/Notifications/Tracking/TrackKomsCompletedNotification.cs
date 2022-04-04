using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Services.Identity;
using KomTracker.Application.Interfaces.Services.Mail;
using KomTracker.Application.Models.Mail;
using KomTracker.Application.Models.Segment;
using KomTracker.Domain.Entities.Athlete;
using MediatR;
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
    private readonly IUserService _userService;
    private readonly IMailService _mailService;

    public TrackKomsCompletedNotificationSendEmailHandler(IUserService userService, IMailService mailService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
    }

    public async Task Handle(TrackKomsCompletedNotification notification, CancellationToken cancellationToken)
    {
        var athleteId = notification.Athlete.AthleteId;
        var user = await _userService.GetUserAsync(athleteId);

        if (user == null)
        {
            throw new Exception($"User not fount for athletedId: {athleteId}");
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
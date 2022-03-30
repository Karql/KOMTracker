using KomTracker.Application.Models.Segment;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Notifications.Tracking;
public class TrackKomsCompletedNotification : INotification
{
    public int AthleteId { get; set; }
    public ComparedEffortsModel ComparedEfforts { get; set; } = default!;
}

public class TrackKomsCompletedNotificationSendEmailHandler : INotificationHandler<TrackKomsCompletedNotification>
{
    public async Task Handle(TrackKomsCompletedNotification notification, CancellationToken cancellationToken)
    {
        ;
    }
}
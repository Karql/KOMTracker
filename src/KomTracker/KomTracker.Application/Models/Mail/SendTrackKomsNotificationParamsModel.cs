using KomTracker.Application.Models.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Models.Mail;
public class SendTrackKomsNotificationParamsModel
{
    public string To { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public ComparedEffortsModel ComparedEfforts { get; set; } = default!;
}

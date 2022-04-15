using KomTracker.Application.Models.Mail;
using KomTracker.Application.Models.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Interfaces.Services.Mail;
public interface IMailService
{
    Task SendChangeEmailConfirmationAsync(SendChangeEmailConfirmationParamsModel p);
    Task SendTrackKomsNotificationAsync(SendTrackKomsNotificationParamsModel p);
}

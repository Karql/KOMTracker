using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Interfaces.Services.Mail;
public interface IMailService
{
    Task SendTrackKomsNotification();
}

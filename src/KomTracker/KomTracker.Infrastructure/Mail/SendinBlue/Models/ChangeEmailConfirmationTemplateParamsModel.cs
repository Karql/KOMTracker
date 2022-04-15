using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Mail.SendinBlue.Models;
internal class ChangeEmailConfirmationTemplateParamsModel
{
    public string firstName { get; set; }
    public string url { get; set; }
}

using KomTracker.Application.Models.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Models.Mail;
public class SendChangeEmailConfirmationParamsModel
{
    public string To { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string Url { get; set; } = default!;
}

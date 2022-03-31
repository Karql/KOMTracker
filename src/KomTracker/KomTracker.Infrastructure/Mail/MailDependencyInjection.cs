using KomTracker.Application.Interfaces.Services.Mail;
using KomTracker.Infrastructure.Mail.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Mail;
public static class MailDependencyInjection
{
    public static IServiceCollection AddMail(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IMailService, MailService>();

        return services;
    }
}

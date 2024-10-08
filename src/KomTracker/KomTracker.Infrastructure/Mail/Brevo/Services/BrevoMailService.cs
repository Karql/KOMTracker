using KomTracker.Application.Interfaces.Services.Mail;
using KomTracker.Application.Models.Configuration;
using KomTracker.Application.Models.Mail;
using brevo_csharp.Api;
using brevo_csharp.Client;
using brevo_csharp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using KomTracker.Infrastructure.Mail.Brevo.Models;

namespace KomTracker.Infrastructure.Mail.Brevo.Services;

public class BrevoMailService : IMailService
{
    private readonly ApplicationConfiguration _applicationConfiguration;

    public BrevoMailService(ApplicationConfiguration applicationConfiguration)
    {
        _applicationConfiguration = applicationConfiguration ?? throw new ArgumentNullException(nameof(applicationConfiguration));
        _ = applicationConfiguration.BrevoConfiguration ?? throw new ArgumentException("BrevoConfiguration not defined!", nameof(applicationConfiguration.BrevoConfiguration));

        var apiKey = applicationConfiguration.BrevoConfiguration.ApiKey;        

        if (string.IsNullOrEmpty(apiKey))
        {
            throw new ArgumentException("BrevoConfiguration.ApiKey not defined!", nameof(applicationConfiguration.BrevoConfiguration.ApiKey));
        }

        var changeEmailConfirmationTemplateId = applicationConfiguration.BrevoConfiguration.ChangeEmailConfirmationTemplateId;

        if (changeEmailConfirmationTemplateId <= 0)
        {
            throw new ArgumentException("BrevoConfiguration.ChangeEmailConfirmationTemplateId not defined!", nameof(applicationConfiguration.BrevoConfiguration.ChangeEmailConfirmationTemplateId));
        }

        var trackKomsTemplateId = applicationConfiguration.BrevoConfiguration.TrackKomsTemplateId;

        if (trackKomsTemplateId <= 0)
        {
            throw new ArgumentException("BrevoConfiguration.TrackKomsTemplateId not defined!", nameof(applicationConfiguration.BrevoConfiguration.TrackKomsTemplateId));
        }

        Configuration.Default.AddApiKey("api-key", apiKey);
    }

    public async System.Threading.Tasks.Task SendChangeEmailConfirmationAsync(SendChangeEmailConfirmationParamsModel p)
    {
        if (!_applicationConfiguration.BrevoConfiguration.Enabled)
        {
            return;
        }

        var to = p.To;

        if (!string.IsNullOrWhiteSpace(_applicationConfiguration.BrevoConfiguration.TestMail))
        {
            to = _applicationConfiguration.BrevoConfiguration.TestMail;
        }

        var apiInstance = new TransactionalEmailsApi();
        var sendSmtpEmail = new SendSmtpEmail
        {
            To = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(to) },
            TemplateId = _applicationConfiguration.BrevoConfiguration.ChangeEmailConfirmationTemplateId,
            Params = new ChangeEmailConfirmationTemplateParamsModel
            {
                firstName = p.FirstName,
                url = p.Url
            }
        };

        var result = await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
    }

    public async System.Threading.Tasks.Task SendTrackKomsNotificationAsync(SendTrackKomsNotificationParamsModel p)
    {
        if (!_applicationConfiguration.BrevoConfiguration.Enabled)
        {
            return;
        }

        var to = p.To;

        if (!string.IsNullOrWhiteSpace(_applicationConfiguration.BrevoConfiguration.TestMail))
        {
            to = _applicationConfiguration.BrevoConfiguration.TestMail;
        }

        var apiInstance = new TransactionalEmailsApi();
        var sendSmtpEmail = new SendSmtpEmail
        {
            To = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(to) },
            TemplateId = _applicationConfiguration.BrevoConfiguration.TrackKomsTemplateId,
            Params = ConvertToTemplateParams(p)
        };

        var result = await apiInstance.SendTransacEmailAsync(sendSmtpEmail);
    }

    private KomChangesTemplateParamsModel ConvertToTemplateParams(SendTrackKomsNotificationParamsModel p)
    {
        var comparedEfforts = p.ComparedEfforts;

        return new KomChangesTemplateParamsModel
        {
            firstName = p.FirstName,
            komsCount = comparedEfforts.KomsCount,
            newKomsCount = comparedEfforts.NewKomsCount,
            improvedKomsCount = comparedEfforts.ImprovedKomsCount,
            lostKomsCount = comparedEfforts.LostKomsCount,
            returnedKomsCount = comparedEfforts.ReturnedKomsCount,
            newKoms = comparedEfforts.Efforts.Where(x => x.SummarySegmentEffort.NewKom).Select(x => new KomChangesTemplateParamsSegmentModel
            {
                // TOOD: change to x.Segment after fixing CompareEfforts
                segmentId = x.SegmentEffort.SegmentId,
                segmentName = x.SegmentEffort.Name
            }).ToArray(),
            improvedKoms = comparedEfforts.Efforts.Where(x => x.SummarySegmentEffort.ImprovedKom).Select(x => new KomChangesTemplateParamsSegmentModel
            {
                segmentId = x.SegmentEffort.SegmentId,
                segmentName = x.SegmentEffort.Name
            }).ToArray(),
            lostKoms = comparedEfforts.Efforts.Where(x => x.SummarySegmentEffort.LostKom).Select(x => new KomChangesTemplateParamsSegmentModel
            {
                segmentId = x.SegmentEffort.SegmentId,
                segmentName = x.SegmentEffort.Name
            }).ToArray(),
            returnedKoms = comparedEfforts.Efforts.Where(x => x.SummarySegmentEffort.ReturnedKom).Select(x => new KomChangesTemplateParamsSegmentModel
            {
                segmentId = x.SegmentEffort.SegmentId,
                segmentName = x.SegmentEffort.Name
            }).ToArray(),
        };
    }
}
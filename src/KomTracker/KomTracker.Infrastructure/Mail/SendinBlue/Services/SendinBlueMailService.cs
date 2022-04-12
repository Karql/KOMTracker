using KomTracker.Application.Interfaces.Services.Mail;
using KomTracker.Application.Models.Configuration;
using KomTracker.Application.Models.Mail;
using KomTracker.Application.Models.Segment;
using KomTracker.Infrastructure.Mail.SendinBlue.Models;
using sib_api_v3_sdk.Api;
using sib_api_v3_sdk.Client;
using sib_api_v3_sdk.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Mail.SendinBlue.Services;

public class SendinBlueMailService : IMailService
{
    private readonly ApplicationConfiguration _applicationConfiguration;

    public SendinBlueMailService(ApplicationConfiguration applicationConfiguration)
    {
        _applicationConfiguration = applicationConfiguration ?? throw new ArgumentNullException(nameof(applicationConfiguration));
        _ = applicationConfiguration.SendinBlueConfiguration ?? throw new ArgumentException("SendinBlueConfiguration not defined!", nameof(applicationConfiguration.SendinBlueConfiguration));

        var apiKey = applicationConfiguration.SendinBlueConfiguration.ApiKey;        

        if (string.IsNullOrEmpty(apiKey))
        {
            throw new ArgumentException("SendinBlueConfiguration.ApiKey not defined!", nameof(applicationConfiguration.SendinBlueConfiguration.ApiKey));
        }

        var komChangesTemplateId = applicationConfiguration.SendinBlueConfiguration.KomChangesTemplateId;

        if (komChangesTemplateId <= 0)
        {
            throw new ArgumentException("SendinBlueConfiguration.KomChangesTemplateId not defined!", nameof(applicationConfiguration.SendinBlueConfiguration.KomChangesTemplateId));
        }

        Configuration.Default.AddApiKey("api-key", apiKey);
    }

    public async System.Threading.Tasks.Task SendTrackKomsNotificationAsync(SendTrackKomsNotificationParamsModel p)
    {
        var to = p.To;

        if (!string.IsNullOrWhiteSpace(_applicationConfiguration.SendinBlueConfiguration.TestMail))
        {
            to = _applicationConfiguration.SendinBlueConfiguration.TestMail;
        }

        var apiInstance = new TransactionalEmailsApi();
        var sendSmtpEmail = new SendSmtpEmail
        {
            To = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(to) },
            TemplateId = _applicationConfiguration.SendinBlueConfiguration.KomChangesTemplateId,
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
        };
    }
}
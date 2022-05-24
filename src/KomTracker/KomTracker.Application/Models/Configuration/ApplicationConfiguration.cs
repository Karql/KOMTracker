using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Models.Configuration;

public class SendinBlueConfiguration
{
    public bool Enabled { get; set; } = true;
    public string? TestMail { get; set; }
    public string? ApiKey { get; set; }
    public int ChangeEmailConfirmationTemplateId { get; set; }
    public int TrackKomsTemplateId { get; set; }
}

public class ApplicationConfiguration
{
    public bool TrackKomsJobEnabled { get; set; } = true;
    public bool RefreshSegmentsJobEnabled { get; set; } = true;

    /// <summary>
    /// Athlete id using to queries for data like segment details
    /// </summary>
    public int MasterStravaAthleteId { get; set; }

    public SendinBlueConfiguration? SendinBlueConfiguration { get; set; }
}
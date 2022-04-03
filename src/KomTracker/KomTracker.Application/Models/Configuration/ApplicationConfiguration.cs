using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Models.Configuration;

public class SendinBlueConfiguration
{
    public string? ApiKey { get; set; }
    public int KomChangesTemplateId { get; set; }
}

public class ApplicationConfiguration
{
    /// <summary>
    /// Athlete id using to queries for data like segment details
    /// </summary>
    public int MasterStravaAthleteId { get; set; }

    public SendinBlueConfiguration? SendinBlueConfiguration { get; set; }
}
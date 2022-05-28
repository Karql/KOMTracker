using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Mail.SendinBlue.Models;

internal class KomChangesTemplateParamsSegmentModel
{
    public long segmentId { get; set; }
    public string segmentName { get; set; }
}

internal class KomChangesTemplateParamsModel
{
    public string firstName { get; set; }
    public int komsCount { get; set; }
    public int newKomsCount { get; set; }
    public int improvedKomsCount { get; set; }
    public int lostKomsCount { get; set; }
    public int returnedKomsCount { get; set; }

    public IEnumerable<KomChangesTemplateParamsSegmentModel> newKoms { get; set; }
    public IEnumerable<KomChangesTemplateParamsSegmentModel> improvedKoms { get; set; }
    public IEnumerable<KomChangesTemplateParamsSegmentModel> lostKoms { get; set; }
    public IEnumerable<KomChangesTemplateParamsSegmentModel> returnedKoms { get; set; }
}

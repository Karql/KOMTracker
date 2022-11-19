using KomTracker.Application.Models.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Models.Stats;
public class KomsChangesModel
{
    public IEnumerable<EffortModel> NewKoms { get; set; }
    public IEnumerable<EffortModel> LostKoms { get; set; }
}

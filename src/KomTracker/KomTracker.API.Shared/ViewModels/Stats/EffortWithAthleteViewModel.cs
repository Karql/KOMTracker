using KomTracker.API.Shared.ViewModels.Athlete;
using KomTracker.API.Shared.ViewModels.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.API.Shared.ViewModels.Stats;
public class EffortWithAthleteViewModel
{
    public EffortViewModel Effort { get; set; } = default!;
    public AthleteViewModel Athlete { get; set; } = default!;    
}

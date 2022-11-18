using KomTracker.Application.Models.Segment;
using KomTracker.Domain.Entities.Athlete;

namespace KomTracker.Application.Models.Stats;
public class EffortWithAthleteModel
{
    public EffortModel Effort { get; set; } = default!;
    public AthleteEntity Athlete { get; set; } = default!;    

    public EffortWithAthleteModel(EffortModel effort, AthleteEntity athlete)
    {
        Effort = effort ?? throw new ArgumentNullException(nameof(effort));
        Athlete = athlete ?? throw new ArgumentNullException(nameof(athlete));        
    }
}

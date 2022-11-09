using KomTracker.Application.Models.Segment;
using KomTracker.Domain.Entities.Athlete;

namespace KomTracker.Application.Models.Stats;
public class LastKomsChangesModel
{
    public AthleteEntity Athlete { get; set; } = default!;
    public EffortModel Change { get; set; } = default!;

    public LastKomsChangesModel(AthleteEntity athlete, EffortModel change)
    {
        Athlete = athlete ?? throw new ArgumentNullException(nameof(athlete));
        Change = change ?? throw new ArgumentNullException(nameof(change));
    }
}

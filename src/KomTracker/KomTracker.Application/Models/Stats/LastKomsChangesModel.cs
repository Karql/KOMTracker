using KomTracker.Application.Models.Segment;
using KomTracker.Domain.Entities.Athlete;

namespace KomTracker.Application.Models.Stats;
public class LastKomsChangesModel
{
    public AthleteEntity Athletee { get; set; } = default!;
    public EffortModel Change { get; set; } = default!;

    public LastKomsChangesModel(AthleteEntity athletee, EffortModel change)
    {
        Athletee = athletee ?? throw new ArgumentNullException(nameof(athletee));
        Change = change ?? throw new ArgumentNullException(nameof(change));
    }
}

using KomTracker.Application.Models.Segment;
using KomTracker.Domain.Entities.Athlete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Models.Stats;
public record class AthleteStatsModel(
    AthleteEntity Athlete,
    IEnumerable<EffortModel> Koms,
    KomsChangesModel KomsChangesLast30Days,
    KomsChangesModel KomsChangesLastWeek,
    KomsChangesModel KomsChangesThisWeek);
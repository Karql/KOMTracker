using KomTracker.Domain.Contracts;
using KomTracker.Domain.Entities.Club;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Domain.Entities.Athlete;
public class AthleteClubEntity : BaseEntity
{
    public int AthleteId { get; set; }
    public long ClubId { get; set; }

    public AthleteEntity Athlete { get; set; }
    public ClubEntity Club { get; set; }
}

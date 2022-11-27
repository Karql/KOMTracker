using KomTracker.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace KomTracker.Domain.Entities.Athlete;
public class AthleteStatsEntity : BaseEntity
{
    public int AthleteId { get; set; }
    public string StatsJson { get; set; }
    [JsonIgnore]
    public virtual AthleteEntity Athlete { get; set; }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.API.Shared.ViewModels.Segment;

public class KomsSummaryViewModel
{
    public int Id { get; set; }
    public DateTime TrackDate { get; set; }
    public int AthleteId { get; set; }
    public int Koms { get; set; }
    public int NewKoms { get; set; }
    public int ImprovedKoms { get; set; }
    public int LostKoms { get; set; }
}

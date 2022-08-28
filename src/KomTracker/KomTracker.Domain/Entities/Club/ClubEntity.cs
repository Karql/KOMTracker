using KomTracker.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Domain.Entities.Club;
public class ClubEntity : BaseEntity
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string ProfileMedium { get; set; }
    public string Profile { get; set; }
    public string CoverPhoto { get; set; }
    public string CoverPhotoSmall { get; set; }
    public string ActivityTypesIcon { get; set; }
    public string SportType { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public bool Private { get; set; }
    public int MemberCount { get; set; }
    public bool Featured { get; set; }
    public bool Verified { get; set; }
    public string Url { get; set; }
}

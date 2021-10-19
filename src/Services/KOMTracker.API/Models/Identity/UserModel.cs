using KOMTracker.API.Models.Strava;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.Models.Identity
{
    public class UserModel : IdentityUser
    {
        public int AthleteId { get; set; }
        public virtual AthleteModel Athlete { get; set; }
    }
}

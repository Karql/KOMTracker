using KOMTracker.API.Models.Identity;
using KOMTracker.API.Models.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.Models.Athlete
{
    public class AthleteModel
    {
        public int AthleteId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Sex { get; set; }
        public float Weight { get; set; }
        public string Profile { get; set; }
        public string ProfileMedium { get; set; }
        public virtual UserModel User { get; set; }
        public virtual TokenModel Token { get; set; }
    }
}

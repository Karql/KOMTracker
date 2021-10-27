using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.Models.Strava
{
    public class TokenModel
    {
        public int AthleteId { get; set; }
        public string TokenType { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string Scope { get; set; }
        public virtual AthleteModel Athlete { get; set; }
    }
}

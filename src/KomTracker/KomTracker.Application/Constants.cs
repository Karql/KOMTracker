using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application;

public class Constants
{
    public class Strava
    {
        public const string ScopeRead = "read";
        public const string ScopeActivityRead = "activity:read";
        public const string ScopeActivityReadAll = "activity:read_all";
        public const string ScopeProfileReadAll = "profile:read_all";

        /// <summary>
        /// Scopes requested on the Strava authorize screen (login and the opt-in escalation both use this).
        /// We ask for activity:read_all up front (private / "Only You" rides + precise start/finish →
        /// accurate mileage), but also request activity:read so that if the user unchecks the private part
        /// they still keep basic activity access. Verification (below) only needs read + profile:read_all +
        /// one of the activity scopes, so declining activity:read_all is fine.
        /// </summary>
        public static readonly HashSet<string> AuthorizeScopes = new()
        {
            ScopeRead,
            ScopeActivityRead,
            ScopeActivityReadAll,
            ScopeProfileReadAll
        };

        /// <summary>Scopes without activity:read_all — requested by the "revoke private rides" flow to
        /// drop back to public-only activity access (re-auth with a narrower set replaces the token).</summary>
        public static readonly HashSet<string> BasicScopes = new()
        {
            ScopeRead,
            ScopeActivityRead,
            ScopeProfileReadAll
        };
    }

    public class Roles
    {
        public const string Admin = "admin";
    }
}

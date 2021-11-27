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
        public static readonly HashSet<string> RequiredScopes = new()
        {
            "read",
            "activity:read",
            "profile:read_all"
        };
    }
}

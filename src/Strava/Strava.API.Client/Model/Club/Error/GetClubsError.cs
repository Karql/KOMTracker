using Strava.API.Client.Model.Base.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Club.Error;

public class GetClubsError : BaseError
{

    public GetClubsError(string message) : base(message)
    {
    }
}

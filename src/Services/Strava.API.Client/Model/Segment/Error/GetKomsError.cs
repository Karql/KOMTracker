using Strava.API.Client.Model.Base.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Segment.Error
{
    public class GetKomsError : BaseError
    {

        public GetKomsError(string message) : base(message)
        {
        }
    }
}

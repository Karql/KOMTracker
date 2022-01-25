using Strava.API.Client.Model.Base.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Segment.Error;

public class GetSegmentError : BaseError
{
    public const string NotFound = "NotFound";

    public GetSegmentError(string message) : base(message)
    {
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Errors.Strava.Segment;

public class GetSegmentError : FluentResults.Error
{
    public const string Unauthorized = "Unauthorized";
    public const string UnknownError = "UnknownError";
    public const string NotFound = "NotFound";

    public GetSegmentError(string message)
    : base(message)
    {
    }
}

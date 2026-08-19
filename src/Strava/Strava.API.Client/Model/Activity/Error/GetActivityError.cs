using Strava.API.Client.Model.Base.Error;

namespace Strava.API.Client.Model.Activity.Error;

/// <summary>Error from GET /activities/{id}. Adds <see cref="NotFound"/> (404) to the shared base messages.</summary>
public class GetActivityError : BaseError
{
    public const string NotFound = "NotFound";

    public GetActivityError(string message) : base(message)
    {
    }
}

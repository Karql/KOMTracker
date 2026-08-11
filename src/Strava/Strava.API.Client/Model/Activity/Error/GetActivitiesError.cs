using Strava.API.Client.Model.Base.Error;

namespace Strava.API.Client.Model.Activity.Error;

public class GetActivitiesError : BaseError
{
    public GetActivitiesError(string message) : base(message)
    {
    }
}

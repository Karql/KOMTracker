using Strava.API.Client.Model.Base.Error;

namespace Strava.API.Client.Model.Athlete.Error;

public class GetAthleteError : BaseError
{
    public GetAthleteError(string message) : base(message)
    {
    }
}

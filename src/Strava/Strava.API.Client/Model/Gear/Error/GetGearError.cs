using Strava.API.Client.Model.Base.Error;

namespace Strava.API.Client.Model.Gear.Error;

public class GetGearError : BaseError
{
    public GetGearError(string message) : base(message)
    {
    }
}

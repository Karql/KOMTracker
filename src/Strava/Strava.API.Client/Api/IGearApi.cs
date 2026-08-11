using FluentResults;
using Strava.API.Client.Model.Gear;
using System.Threading.Tasks;

namespace Strava.API.Client.Api;

public interface IGearApi
{
    /// <summary>Gets a single gear (bike/shoe) by id — GET /gear/{id}.</summary>
    Task<Result<GearDetailedModel>> GetGearAsync(string gearId, string token);
}

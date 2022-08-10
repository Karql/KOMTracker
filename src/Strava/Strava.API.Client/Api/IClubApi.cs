using FluentResults;
using Strava.API.Client.Model.Club;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Api;
public interface IClubApi
{
    Task<Result<IEnumerable<ClubSummaryModel>>> GetClubsAsync(string token);
}

using FluentResults;
using Strava.API.Client.Model.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Api
{
    public interface ITokenApi
    {
        Task<Result<TokenWithAthleteModel>> ExchangeAsync(string code); 
    }
}

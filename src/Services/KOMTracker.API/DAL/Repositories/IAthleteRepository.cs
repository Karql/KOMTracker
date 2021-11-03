using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.UnitOfWork.Abstract;
using ApiModel = Strava.API.Client.Model;

namespace KOMTracker.API.DAL.Repositories
{
    public interface IAthleteRepository : IRepository
    {        
        Task<bool> IsAthleteExistsAsync(int athleteId);
        Task AddOrUpdateAthleteAsync(AthleteModel athlete);
        Task<TokenModel> GetTokenAsync(int athleteId);
        Task AddOrUpdateTokenAsync(TokenModel token);
    }
}

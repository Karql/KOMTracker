using KOMTracker.API.Models.Strava;
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
        Task AddOrUpdateTokenAsync(TokenModel token);
    }
}

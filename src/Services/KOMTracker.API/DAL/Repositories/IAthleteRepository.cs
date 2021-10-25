using KOMTracker.API.Models.Strava;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.UnitOfWork.Abstract;

namespace KOMTracker.API.DAL.Repositories
{
    public interface IAthleteRepository : IRepository
    {
        Task<AthleteModel> GetAthleteByIdAsync(int id);
    }
}

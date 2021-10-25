using KOMTracker.API.Models.Strava;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.Infrastructure.Services
{
    public interface IAthleteService
    {
        Task<AthleteModel> GetAthleteByIdAsync(int id);
    }
}

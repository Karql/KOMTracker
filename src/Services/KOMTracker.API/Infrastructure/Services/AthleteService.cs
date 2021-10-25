using KOMTracker.API.DAL;
using KOMTracker.API.DAL.Repositories;
using KOMTracker.API.Models.Strava;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.Infrastructure.Services
{
    public class AthleteService : IAthleteService
    {
        private readonly IKOMUnitOfWork _komUoW;

        public AthleteService(IKOMUnitOfWork komUoW)
        {
            _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        }

        public async Task<AthleteModel> GetAthleteByIdAsync(int id)
        {
            var athleteRepo = _komUoW.GetRepository<IAthleteRepository>();
            return await athleteRepo.GetAthleteByIdAsync(id);
        }
    }
}

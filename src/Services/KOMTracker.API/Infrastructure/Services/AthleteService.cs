using KOMTracker.API.DAL;
using KOMTracker.API.DAL.Repositories;
using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiModel = Strava.API.Client.Model;

namespace KOMTracker.API.Infrastructure.Services
{
    public class AthleteService : IAthleteService
    {
        private readonly IKOMUnitOfWork _komUoW;

        public AthleteService(IKOMUnitOfWork komUoW)
        {
            _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        }

        public Task<bool> IsAthleteExistsAsync(int athleteId)
        {
            return _komUoW
                .GetRepository<IAthleteRepository>()
                .IsAthleteExistsAsync(athleteId);
        }

        public Task AddOrUpdateAthleteAsync(AthleteModel athlete)
        {
            return _komUoW
                .GetRepository<IAthleteRepository>()
                .AddOrUpdateAthleteAsync(athlete);
        }

        public Task AddOrUpdateTokenAsync(TokenModel token)
        {
            return _komUoW
                .GetRepository<IAthleteRepository>()
                .AddOrUpdateTokenAsync(token);
        }
    }
}

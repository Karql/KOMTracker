using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Token;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.UnitOfWork.Concrete;
using FlexLabs.EntityFrameworkCore.Upsert;

namespace KOMTracker.API.DAL.Repositories
{
    public class EFAthleteRepository : EFRepositoryBase<KOMDBContext>, IAthleteRepository
    {
        public async Task<bool> IsAthleteExistsAsync(int athleteId)
        {
            return await _context.Athlete.AnyAsync(x => x.AthleteId == athleteId);
        }

        public Task AddOrUpdateAthleteAsync(AthleteModel athlete)
        {
            return _context
                .Athlete
                .Upsert(athlete)
                .RunAsync();
        }

        public Task<TokenModel> GetTokenAsync(int athleteId)
        {
            return _context.Token.FirstOrDefaultAsync(x => x.AthleteId == athleteId);
        }

        public Task AddOrUpdateTokenAsync(TokenModel token)
        {
            return _context
                .Token
                .Upsert(token)
                .RunAsync();
        }
    }
}

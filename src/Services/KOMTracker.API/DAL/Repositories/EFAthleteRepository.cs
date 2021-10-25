using KOMTracker.API.Models.Strava;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.UnitOfWork.Concrete;

namespace KOMTracker.API.DAL.Repositories
{
    public class EFAthleteRepository : EFRepositoryBase<KOMDBContext>, IAthleteRepository
    {
        public async Task<AthleteModel> GetAthleteByIdAsync(int id)
        {
            return await _context.Athlete.FirstOrDefaultAsync(x => x.AthleteId == id);
        }
    }
}

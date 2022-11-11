using KomTracker.Domain.Entities.Club;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.UnitOfWork.Abstract;

namespace KomTracker.Application.Interfaces.Persistence.Repositories;

public interface IClubRepository : IRepository
{
    Task AddOrUpdateClubsAsync(IEnumerable<ClubEntity> clubs);
}

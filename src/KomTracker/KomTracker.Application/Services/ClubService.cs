using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Club;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Services;
public class ClubService : IClubService
{
    private readonly IKOMUnitOfWork _komUoW;

    public ClubService(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public Task AddOrUpdateClubsAsync(IEnumerable<ClubEntity> clubs)
    {
        return _komUoW
            .GetRepository<IClubRepository>()
            .AddOrUpdateClubsAsync(clubs);
    }
}
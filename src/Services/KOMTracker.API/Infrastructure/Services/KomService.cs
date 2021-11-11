using KOMTracker.API.DAL;
using Microsoft.Extensions.Logging;
using MoreLinq;
using System;
using System.Threading.Tasks;

namespace KOMTracker.API.Infrastructure.Services;

public class KomService : IKomService
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ILogger<KomService> _logger;
    private readonly IAthleteService _athleteService;

    public KomService(IKOMUnitOfWork komUoW, ILogger<KomService> logger, IAthleteService athleteService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
    }

    public async Task TrackKomsAsync()
    {
        var athlets = await _athleteService.GetAllAthletesAsync();

        foreach (var athlete in athlets)
        {
            await TrackKomsForAthleteAsync(athlete.AthleteId);
        }
    }

    protected async Task TrackKomsForAthleteAsync(int athleteId)
    {
        ;
    }
}

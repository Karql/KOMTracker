using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Athlete;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Extensions;

namespace KomTracker.Application.Commands.Ranking;
public class CalculateRankingCommand : IRequest<Result>
{

}

public class CalculateRankingCommandHandler : IRequestHandler<CalculateRankingCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ILogger<CalculateRankingCommandHandler> _logger;
    private readonly IAthleteService _athleteService;

    public CalculateRankingCommandHandler(IKOMUnitOfWork komUoW, ILogger<CalculateRankingCommandHandler> logger, IAthleteService athleteService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
    }

    public async Task<Result> Handle(CalculateRankingCommand request, CancellationToken cancellationToken)
    {
        var athlets = await _athleteService.GetAllAthletesAsync();

        //foreach (var athlete in athlets)
        foreach (var athlete in athlets.Where(x => x.AthleteId == 2394302))
        {
            if (cancellationToken.IsCancellationRequested) return Result.Ok(); // TODO: OK?

            await CalculateRankingForAthleteAsync(athlete);
        }

        return Result.Ok();
    }

    protected async Task CalculateRankingForAthleteAsync(AthleteEntity athlete)
    {
        var athleteId = athlete.AthleteId;

        var now = DateTime.UtcNow;
        var beginningOfMonth = now.BeginningOfMonth();
        //var endOfMonth = now.EndOfMonth();

        var beginningOfWeek = now.BeginningOfWeek();
        //var endOfWeek = now.EndOfWeek();

        var beginningOfLastWeek = beginningOfWeek.AddDays(-1).BeginningOfWeek();
        var endOfLastWeek = beginningOfWeek.AddDays(-1).EndOfWeek();
    }
}
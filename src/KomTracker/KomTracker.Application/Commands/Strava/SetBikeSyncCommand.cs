using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using MediatR;

namespace KomTracker.Application.Commands.Strava;

/// <summary>Enable/disable per-athlete automatic bike (gear) sync — gates the bike sync job. Manual bike
/// sync stays available regardless. Does not touch activity sync.</summary>
public class SetBikeSyncCommand : IRequest<Result>
{
    public int AthleteId { get; set; }
    public bool Enabled { get; set; }
}

public class SetBikeSyncCommandHandler : IRequestHandler<SetBikeSyncCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public SetBikeSyncCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(SetBikeSyncCommand request, CancellationToken cancellationToken)
    {
        await _komUoW.GetRepository<IAthleteSyncRepository>()
            .SetBikesEnabledAsync(request.AthleteId, request.Enabled);

        return Result.Ok();
    }
}

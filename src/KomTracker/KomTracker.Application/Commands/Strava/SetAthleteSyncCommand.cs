using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Strava;
using MediatR;

namespace KomTracker.Application.Commands.Strava;

/// <summary>Enable/disable per-athlete Strava activity sync (temporary admin opt-in until the 1c UI).</summary>
public class SetAthleteSyncCommand : IRequest<Result>
{
    public int AthleteId { get; set; }
    public bool Enabled { get; set; }
}

public class SetAthleteSyncCommandHandler : IRequestHandler<SetAthleteSyncCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public SetAthleteSyncCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(SetAthleteSyncCommand request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IAthleteSyncRepository>();

        await repo.UpsertAsync(new AthleteSyncEntity
        {
            AthleteId = request.AthleteId,
            ActivitiesEnabled = request.Enabled
        });

        return Result.Ok();
    }
}

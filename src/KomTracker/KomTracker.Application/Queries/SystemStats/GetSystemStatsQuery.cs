using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.SystemStats;
using MediatR;

namespace KomTracker.Application.Queries.SystemStats;

/// <summary>The admin "System → Stats" snapshot of the whole app (D-SYS-1).</summary>
public class GetSystemStatsQuery : IRequest<SystemStatsReport>
{
}

public class GetSystemStatsQueryHandler : IRequestHandler<GetSystemStatsQuery, SystemStatsReport>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetSystemStatsQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public Task<SystemStatsReport> Handle(GetSystemStatsQuery request, CancellationToken cancellationToken)
        => _komUoW.GetRepository<ISystemStatsRepository>().GetReportAsync(DateTime.UtcNow);
}

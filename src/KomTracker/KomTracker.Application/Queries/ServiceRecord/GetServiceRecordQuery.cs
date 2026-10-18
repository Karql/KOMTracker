using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.ServiceRecord;
using MediatR;

namespace KomTracker.Application.Queries.ServiceRecord;

/// <summary>A single service of the caller with its targets; null when missing or owned by someone else (looks like "not found").</summary>
public class GetServiceRecordQuery : IRequest<ServiceRecordEntity?>
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
}

public class GetServiceRecordQueryHandler : IRequestHandler<GetServiceRecordQuery, ServiceRecordEntity?>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetServiceRecordQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<ServiceRecordEntity?> Handle(GetServiceRecordQuery request, CancellationToken cancellationToken)
    {
        var service = await _komUoW.GetRepository<IServiceRecordRepository>().GetServiceRecordAsync(request.Id);

        if (service is null || service.UserId != request.UserId)
        {
            return null;
        }

        await ServiceTargetNames.ResolveAsync(_komUoW, request.UserId, new[] { service });

        return service;
    }
}

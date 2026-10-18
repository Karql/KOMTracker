using FluentResults;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using MediatR;

namespace KomTracker.Application.Commands.ServiceRecord;

/// <summary>Deletes a service together with its targets (and their stored mileage).</summary>
public class DeleteServiceRecordCommand : IRequest<Result>
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
}

public class DeleteServiceRecordCommandHandler : IRequestHandler<DeleteServiceRecordCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;

    public DeleteServiceRecordCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result> Handle(DeleteServiceRecordCommand request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IServiceRecordRepository>();
        var service = await repo.GetServiceRecordAsync(request.Id);

        if (service is null)
        {
            return Result.Fail(new NotFoundError($"Service {request.Id} not found."));
        }

        if (service.UserId != request.UserId)
        {
            return Result.Fail(new ForbiddenError("Service does not belong to the current user."));
        }

        repo.DeleteServiceRecord(service);
        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }
}

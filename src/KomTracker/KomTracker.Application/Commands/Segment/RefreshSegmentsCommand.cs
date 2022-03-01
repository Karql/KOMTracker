using FluentResults;
using MediatR;

namespace KomTracker.Application.Commands.Tracking;

public class RefreshSegmentsCommand : IRequest<Result>
{

}

public class RefreshSegmentsCommandHandler : IRequestHandler<RefreshSegmentsCommand, Result>
{
    public Task<Result> Handle(RefreshSegmentsCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
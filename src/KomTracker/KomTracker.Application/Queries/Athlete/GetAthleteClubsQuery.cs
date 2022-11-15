using KomTracker.Application.Models.Segment;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Club;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KomTracker.Application.Queries.Athlete;

public class GetAthleteClubsQuery : IRequest<IEnumerable<ClubEntity>>
{
    public int AthleteId { get; set; }
}

public class GetAthleteClubsQueryHandler : IRequestHandler<GetAthleteClubsQuery, IEnumerable<ClubEntity>>
{
    private readonly IClubService _clubService;

    public GetAthleteClubsQueryHandler(IClubService clubService)
    {
        _clubService = clubService ?? throw new ArgumentNullException(nameof(clubService));
    }

    public Task<IEnumerable<ClubEntity>> Handle(GetAthleteClubsQuery request, CancellationToken cancellationToken)
    {
        return _clubService.GetAthleteClubsAsync(request.AthleteId);
    }
}

using KomTracker.Application.Services;
using KomTracker.Domain.Entities.CostAnalysis;
using MediatR;

namespace KomTracker.Application.Queries.CostAnalysis;

/// <summary>The caller's whole attributed cost ledger (Phase 4) — sliced and aggregated in the browser (D-COST-1).</summary>
public class GetCostAnalysisQuery : IRequest<CostAnalysisReport>
{
    public string UserId { get; set; } = default!;
}

public class GetCostAnalysisQueryHandler : IRequestHandler<GetCostAnalysisQuery, CostAnalysisReport>
{
    private readonly CostAnalysisService _costAnalysis;

    public GetCostAnalysisQueryHandler(CostAnalysisService costAnalysis)
    {
        _costAnalysis = costAnalysis ?? throw new ArgumentNullException(nameof(costAnalysis));
    }

    public Task<CostAnalysisReport> Handle(GetCostAnalysisQuery request, CancellationToken cancellationToken)
        => _costAnalysis.BuildAsync(request.UserId);
}

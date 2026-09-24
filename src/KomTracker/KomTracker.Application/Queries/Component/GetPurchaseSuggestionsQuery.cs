using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Component;
using MediatR;

namespace KomTracker.Application.Queries.Component;

/// <summary>Distinct Brand / Model / Purchase place values across the user's bikes and components (autocomplete hints).</summary>
public class GetPurchaseSuggestionsQuery : IRequest<PurchaseSuggestionsModel>
{
    public string UserId { get; set; } = default!;
}

public class GetPurchaseSuggestionsQueryHandler : IRequestHandler<GetPurchaseSuggestionsQuery, PurchaseSuggestionsModel>
{
    private readonly IKOMUnitOfWork _komUoW;

    public GetPurchaseSuggestionsQueryHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<PurchaseSuggestionsModel> Handle(GetPurchaseSuggestionsQuery request, CancellationToken cancellationToken)
    {
        // DISTINCT is pushed to the DB per field (only the values come back, not whole rows); the two already-small
        // per-source lists are then merged in memory (cross-source dedupe is case-insensitive + trimmed + sorted).
        var components = await _komUoW.GetRepository<IComponentRepository>().GetDistinctPurchaseFieldsAsync(request.UserId);
        var bikes = await _komUoW.GetRepository<IBikeRepository>().GetDistinctPurchaseFieldsAsync(request.UserId);

        return new PurchaseSuggestionsModel(
            Merge(components.Brands, bikes.Brands),
            Merge(components.Models, bikes.Models),
            Merge(components.PurchasePlaces, bikes.PurchasePlaces));
    }

    private static IReadOnlyList<string> Merge(IEnumerable<string> a, IEnumerable<string> b)
        => a.Concat(b)
            .Select(v => v.Trim())
            .Where(v => v.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
            .ToList();
}

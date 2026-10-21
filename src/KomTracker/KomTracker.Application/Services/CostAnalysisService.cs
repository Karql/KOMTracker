#nullable enable
using System.Globalization;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Bike;
using KomTracker.Domain.Entities.Component;
using KomTracker.Domain.Entities.CostAnalysis;
using KomTracker.Domain.Entities.Expense;
using KomTracker.Domain.Entities.ServiceRecord;

namespace KomTracker.Application.Services;

/// <summary>
/// Builds a user's cost ledger (Phase 4): every purchase, service, expense and resale as attributed lines.
/// <list type="bullet">
/// <item>Bikes: purchase / sale on the bike itself.</item>
/// <item>Components: price and resale split across bikes by the km done on each (D-COST-2); equal split when installed
/// but 0 km; "no bike" when never installed. Undated purchase → the first install date.</item>
/// <item>Services: cost split equally across targets; a component target goes to the bike(s) it was on at the service
/// date, directly or through its parent (D-COST-3).</item>
/// <item>Expenses: no bike ("General").</item>
/// </list>
/// Unknown (null) and zero amounts produce no line. All DB work is a fixed handful of batched queries.
/// </summary>
public class CostAnalysisService
{
    private static readonly NumberFormatInfo Nfi = new() { NumberGroupSeparator = " " };

    private readonly IKOMUnitOfWork _komUoW;
    private readonly ComponentMileageService _mileageService;

    public CostAnalysisService(IKOMUnitOfWork komUoW, ComponentMileageService mileageService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _mileageService = mileageService ?? throw new ArgumentNullException(nameof(mileageService));
    }

    public async Task<CostAnalysisReport> BuildAsync(string userId)
    {
        var bikes = (await _komUoW.GetRepository<IBikeRepository>().GetBikesAsync(userId, includeInactive: true)).ToList();
        var components = (await _komUoW.GetRepository<IComponentRepository>().GetComponentsAsync(userId, includeInactive: true)).ToList();
        var services = (await _komUoW.GetRepository<IServiceRecordRepository>().GetServiceRecordsAsync(userId)).ToList();
        var expenses = (await _komUoW.GetRepository<IExpenseRepository>().GetExpensesAsync(userId)).ToList();

        var componentIds = components.Select(c => c.Id).ToList();
        var kmByBike = await _mileageService.ComputeKmByBikeAsync(componentIds);
        var installations = componentIds.Count == 0
            ? new List<InstallationEntity>()
            : (await _komUoW.GetRepository<IInstallationRepository>().GetByComponentsAsync(componentIds)).ToList();

        var report = new CostAnalysisReport();

        foreach (var bike in bikes)
        {
            AddBikeLines(report.Lines, bike);
        }

        var installsByComponent = installations.GroupBy(i => i.ComponentId).ToDictionary(g => g.Key, g => g.ToList());
        foreach (var component in components)
        {
            var shares = ComponentShares(kmByBike.GetValueOrDefault(component.Id));
            var firstInstall = installsByComponent.GetValueOrDefault(component.Id)?
                .Where(i => i.DateFrom is not null).Min(i => i.DateFrom);
            AddComponentLines(report.Lines, component, shares, component.PurchaseDate ?? firstInstall);
        }

        var componentsById = components.ToDictionary(c => c.Id);
        var bikeNames = bikes.ToDictionary(b => b.Id, b => b.Name);
        foreach (var service in services)
        {
            AddServiceLines(report.Lines, service, componentsById, bikeNames, installsByComponent);
        }

        foreach (var expense in expenses)
        {
            AddExpenseLines(report.Lines, expense);
        }

        report.Bikes = await BuildBikesAsync(bikes);

        return report;
    }

    // ---- bikes ----------------------------------------------------------------------------------------------------

    private static void AddBikeLines(List<CostLine> lines, BikeEntity bike)
    {
        CostLine Line(CostKind kind, DateTime? date, decimal amount) => new()
        {
            Date = date,
            Source = CostSource.Bikes,
            Kind = kind,
            // No type categories (gravel / road…): there is usually one bike per type, so the drill-down goes from
            // "Bikes" straight to the bikes (single-slice levels are skipped).
            Group = "Bikes",
            Category = "Bikes",
            ItemId = bike.Id,
            ItemName = bike.Name,
            BikeId = bike.Id,
            Amount = kind == CostKind.Resale ? -amount : amount,
            FullAmount = amount
        };

        if (bike.Price is > 0)
        {
            lines.Add(Line(CostKind.Cost, bike.PurchaseDate, bike.Price.Value));
        }

        if (bike.SalePrice is > 0)
        {
            lines.Add(Line(CostKind.Resale, bike.SaleDate, bike.SalePrice.Value));
        }
    }

    // ---- components -----------------------------------------------------------------------------------------------

    internal sealed record Share(int? BikeId, decimal Fraction, string? Note);

    // Split by km per bike; equal when installed but never ridden; one "no bike" share when never installed.
    internal static IReadOnlyList<Share> ComponentShares(IReadOnlyDictionary<int, decimal>? kmByBike)
    {
        if (kmByBike is null || kmByBike.Count == 0)
        {
            return new[] { new Share(null, 1m, "Never installed on a bike") };
        }

        if (kmByBike.Count == 1)
        {
            return new[] { new Share(kmByBike.Keys.First(), 1m, null) };
        }

        var total = kmByBike.Values.Sum();
        if (total <= 0)
        {
            return kmByBike.Keys
                .Select(bikeId => new Share(bikeId, 1m / kmByBike.Count, $"Split equally — {kmByBike.Count} bikes, no km yet"))
                .ToList();
        }

        return kmByBike
            .Where(kv => kv.Value > 0)
            .Select(kv => new Share(kv.Key, kv.Value / total, $"{Km(kv.Value)} of {Km(total)} km on this bike"))
            .ToList();
    }

    private static void AddComponentLines(List<CostLine> lines, ComponentEntity component, IReadOnlyList<Share> shares, DateTime? purchaseDate)
    {
        var group = ComponentCategoryMetadata.DisplayName(ComponentCategoryMetadata.Group(component.Category));
        var category = ComponentCategoryMetadata.DisplayName(component.Category);

        void Add(CostKind kind, DateTime? date, decimal amount)
        {
            foreach (var share in shares)
            {
                var part = amount * share.Fraction;
                lines.Add(new CostLine
                {
                    Date = date,
                    Source = CostSource.Components,
                    Kind = kind,
                    Group = group,
                    Category = category,
                    ComponentCategory = component.Category,
                    ItemId = component.Id,
                    ItemName = component.Name,
                    BikeId = share.BikeId,
                    Share = share.Fraction,
                    Amount = kind == CostKind.Resale ? -part : part,
                    FullAmount = amount,
                    Note = share.Note
                });
            }
        }

        if (component.Price is > 0)
        {
            Add(CostKind.Cost, purchaseDate, component.Price.Value);
        }

        if (component.SalePrice is > 0)
        {
            Add(CostKind.Resale, component.SaleDate, component.SalePrice.Value);
        }
    }

    // ---- services -------------------------------------------------------------------------------------------------

    private static void AddServiceLines(List<CostLine> lines, ServiceRecordEntity service,
        IReadOnlyDictionary<int, ComponentEntity> components, IReadOnlyDictionary<int, string> bikeNames,
        IReadOnlyDictionary<int, List<InstallationEntity>> installsByComponent)
    {
        if (service.Cost is not > 0 || service.Targets.Count == 0)
        {
            return;
        }

        var cost = service.Cost.Value;
        var perTarget = 1m / service.Targets.Count;

        foreach (var target in service.Targets)
        {
            if (target.BikeId is int bikeId)
            {
                lines.Add(ServiceLine(service, cost, perTarget, bikeId, "Bike", null,
                    Note(service, perTarget, bikeNames.GetValueOrDefault(bikeId))));
                continue;
            }

            if (target.ComponentId is not int componentId || !components.TryGetValue(componentId, out var component))
            {
                continue;
            }

            var category = ComponentCategoryMetadata.DisplayName(component.Category);
            var bikesThen = BikesAt(componentId, service.Date, installsByComponent);

            if (bikesThen.Count == 0)
            {
                lines.Add(ServiceLine(service, cost, perTarget, null, category, component.Category,
                    Note(service, perTarget, $"{component.Name} (not on a bike then)")));
                continue;
            }

            foreach (var bikeThen in bikesThen)
            {
                var share = perTarget / bikesThen.Count;
                lines.Add(ServiceLine(service, cost, share, bikeThen, category, component.Category,
                    Note(service, share, $"{component.Name} on {bikeNames.GetValueOrDefault(bikeThen)}")));
            }
        }
    }

    private static CostLine ServiceLine(ServiceRecordEntity service, decimal cost, decimal share, int? bikeId,
        string category, ComponentCategory? componentCategory, string? note) => new()
    {
        Date = service.Date,
        Source = CostSource.Services,
        Kind = CostKind.Cost,
        Group = "Services",
        Category = category,
        ComponentCategory = componentCategory,
        ItemId = service.Id,
        ItemName = service.Name,
        BikeId = bikeId,
        Share = share,
        Amount = cost * share,
        FullAmount = cost,
        Note = note
    };

    private static string? Note(ServiceRecordEntity service, decimal share, string? what)
        => share >= 1m ? what : $"{Math.Round(share * 100)} % of the visit — {what}";

    // The bike(s) a component was mounted on at an instant: its own Tracked windows on a bike, or — when it sat in a
    // parent component — the parent's on-bike windows at that instant.
    internal static IReadOnlyList<int> BikesAt(int componentId, DateTime at, IReadOnlyDictionary<int, List<InstallationEntity>> installsByComponent)
    {
        static bool Covers(InstallationEntity i, DateTime at)
            => i.Type == ComponentInstallationType.Tracked && i.DateFrom is not null && i.DateFrom <= at && (i.DateTo is null || at < i.DateTo);

        var bikes = new HashSet<int>();
        foreach (var row in (installsByComponent.GetValueOrDefault(componentId) ?? new List<InstallationEntity>()).Where(i => Covers(i, at)))
        {
            if (row.BikeId is int bikeId)
            {
                bikes.Add(bikeId);
            }
            else if (row.ParentComponentId is int parentId)
            {
                foreach (var parentRow in (installsByComponent.GetValueOrDefault(parentId) ?? new List<InstallationEntity>())
                    .Where(p => p.BikeId is not null && Covers(p, at)))
                {
                    bikes.Add(parentRow.BikeId!.Value);
                }
            }
        }

        return bikes.ToList();
    }

    // ---- expenses -------------------------------------------------------------------------------------------------

    private static void AddExpenseLines(List<CostLine> lines, ExpenseEntity expense)
    {
        CostLine Line(CostKind kind, DateTime? date, decimal amount) => new()
        {
            Date = date,
            Source = CostSource.Expenses,
            Kind = kind,
            Group = ExpenseCategoryMetadata.DisplayName(ExpenseCategoryMetadata.Group(expense.Category)),
            Category = ExpenseCategoryMetadata.DisplayName(expense.Category),
            ExpenseCategory = expense.Category,
            ItemId = expense.Id,
            ItemName = expense.Name,
            BikeId = null,
            Amount = kind == CostKind.Resale ? -amount : amount,
            FullAmount = amount
        };

        if (expense.Price > 0)
        {
            lines.Add(Line(CostKind.Cost, expense.PurchaseDate, expense.Price));
        }

        if (expense.SalePrice is > 0)
        {
            lines.Add(Line(CostKind.Resale, expense.SaleDate, expense.SalePrice.Value));
        }
    }

    // ---- bikes' km ------------------------------------------------------------------------------------------------

    private async Task<List<CostBike>> BuildBikesAsync(IReadOnlyCollection<BikeEntity> bikes)
    {
        var bikeIds = bikes.Select(b => b.Id).ToList();
        var gearsByBike = bikeIds.Count == 0
            ? new Dictionary<int, List<string>>()
            : (await _komUoW.GetRepository<IBikeLinkRepository>().GetByBikeIdsAsync(bikeIds))
                .Where(l => l.ExternalService == ExternalService.Strava)
                .GroupBy(l => l.BikeId)
                .ToDictionary(g => g.Key, g => g.Select(l => l.ExternalId).Distinct().ToList());

        var gearIds = gearsByBike.Values.SelectMany(x => x).Distinct().ToList();
        var activitiesByGear = gearIds.Count == 0
            ? Enumerable.Empty<Models.Strava.ActivityAttributionModel>().ToLookup(a => a.GearId)
            : (await _komUoW.GetRepository<IActivityRepository>().GetActivitiesByGearAsync(gearIds)).ToLookup(a => a.GearId);

        return bikes.Select(bike => new CostBike
        {
            Id = bike.Id,
            Name = bike.Name,
            Lifecycle = bike.Lifecycle,
            SeedKm = bike.InitialDistanceKm,
            KmByYear = (gearsByBike.GetValueOrDefault(bike.Id) ?? new List<string>())
                .SelectMany(g => activitiesByGear[g])
                .GroupBy(a => a.StartDate.Year)
                .ToDictionary(g => g.Key, g => Math.Round((decimal)(g.Sum(a => a.DistanceMeters) / 1000.0), 1))
        }).ToList();
    }

    private static string Km(decimal km) => Math.Round(km).ToString("#,0", Nfi);
}

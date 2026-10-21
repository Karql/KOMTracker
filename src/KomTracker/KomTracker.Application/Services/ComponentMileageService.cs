#nullable enable
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Strava;
using KomTracker.Domain.Entities.Bike;
using KomTracker.Domain.Entities.Component;

namespace KomTracker.Application.Services;

/// <summary>
/// Computes component mileage by intersecting installation windows with activities (Phase 3).
/// Effective window of a Tracked installation: on a bike → its own [DateFrom, DateTo); in a parent component →
/// the overlap of that with each of the parent's Tracked on-bike windows (one-level nesting ⇒ ≤2 hops).
/// Half-open on the activity's UTC StartDate; DateTo == null = open. Manual rows contribute their static totals only.
/// The <c>...At</c> variants compute the same totals <em>as of</em> an instant (mileage at a service date): the windows
/// are clipped at it; seed + Manual baselines always count.
/// <para>
/// All DB work is batched into <see cref="BuildContextAsync"/> (a handful of set-based queries), then windowing runs
/// in memory — so recomputing many components, or resolving a page of installation rows, is a constant number of
/// round-trips regardless of how many components/rows/activities are involved.
/// </para>
/// </summary>
public class ComponentMileageService
{
    private readonly IKOMUnitOfWork _komUoW;

    private IInstallationRepository InstallationRepo => _komUoW.GetRepository<IInstallationRepository>();
    private IComponentRepository ComponentRepo => _komUoW.GetRepository<IComponentRepository>();
    private IBikeLinkRepository BikeLinkRepo => _komUoW.GetRepository<IBikeLinkRepository>();
    private IActivityRepository ActivityRepo => _komUoW.GetRepository<IActivityRepository>();
    private IBikeRepository BikeRepo => _komUoW.GetRepository<IBikeRepository>();

    public ComponentMileageService(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public record Totals(decimal DistanceKm, decimal MovingHours, decimal ElevationM, int ActivityCount)
    {
        public static readonly Totals Zero = new(0m, 0m, 0m, 0);
        public Totals Add(Totals other) => new(
            DistanceKm + other.DistanceKm,
            MovingHours + other.MovingHours,
            ElevationM + other.ElevationM,
            ActivityCount + other.ActivityCount);
    }

    /// <summary>
    /// Component totals for a set of components (recompute). One batch of queries, then in-memory windowing:
    /// total = Initial seed + Σ Manual baselines + Σ activities across the component's Tracked windows.
    /// </summary>
    public async Task<IReadOnlyDictionary<int, Totals>> ComputeTotalsAsync(IReadOnlyCollection<int> componentIds)
    {
        var result = new Dictionary<int, Totals>();
        if (componentIds is null || componentIds.Count == 0)
        {
            return result;
        }

        var installationsByComponent = (await InstallationRepo.GetByComponentsAsync(componentIds))
            .GroupBy(i => i.ComponentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var ctx = await BuildContextAsync(installationsByComponent.Values.SelectMany(x => x).ToList());
        var components = await ComponentRepo.GetByIdsAsync(componentIds);

        foreach (var component in components)
        {
            var total = new Totals(
                component.InitialDistanceKm,
                component.InitialMovingHours ?? 0m,
                component.InitialElevationM ?? 0m,
                0);

            if (installationsByComponent.TryGetValue(component.Id, out var installs))
            {
                foreach (var installation in installs)
                {
                    total = installation.Type == ComponentInstallationType.Manual
                        ? total.Add(new Totals(
                            installation.ManualDistanceKm ?? 0m,
                            installation.ManualMovingHours ?? 0m,
                            installation.ManualElevationM ?? 0m,
                            0))
                        : total.Add(RowTotals(installation, ctx));
                }
            }

            result[component.Id] = total;
        }

        return result;
    }

    /// <summary>
    /// Component totals <em>as of</em> given instants (mileage at a service date, D-SRV-4): seed + all Manual baselines
    /// (dateless = historical) + the Tracked windows clipped at the instant (rides with <c>StartDate &lt; at</c>).
    /// One batch of queries for all points, then in-memory windowing. Points for unknown components are left out.
    /// </summary>
    public async Task<IReadOnlyDictionary<(int ComponentId, DateTime At), Totals>> ComputeTotalsAtAsync(
        IReadOnlyCollection<(int ComponentId, DateTime At)> points)
    {
        var result = new Dictionary<(int ComponentId, DateTime At), Totals>();
        if (points is null || points.Count == 0)
        {
            return result;
        }

        var componentIds = points.Select(p => p.ComponentId).Distinct().ToList();
        var installationsByComponent = (await InstallationRepo.GetByComponentsAsync(componentIds))
            .GroupBy(i => i.ComponentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var ctx = await BuildContextAsync(installationsByComponent.Values.SelectMany(x => x).ToList());
        var components = (await ComponentRepo.GetByIdsAsync(componentIds)).ToDictionary(c => c.Id);

        foreach (var point in points.Distinct())
        {
            if (!components.TryGetValue(point.ComponentId, out var component))
            {
                continue;
            }

            var total = new Totals(
                component.InitialDistanceKm,
                component.InitialMovingHours ?? 0m,
                component.InitialElevationM ?? 0m,
                0);

            if (installationsByComponent.TryGetValue(component.Id, out var installs))
            {
                foreach (var installation in installs)
                {
                    total = installation.Type == ComponentInstallationType.Manual
                        ? total.Add(new Totals(
                            installation.ManualDistanceKm ?? 0m,
                            installation.ManualMovingHours ?? 0m,
                            installation.ManualElevationM ?? 0m,
                            0))
                        : total.Add(RowTotals(installation, ctx, point.At));
                }
            }

            result[point] = total;
        }

        return result;
    }

    /// <summary>
    /// Bike totals <em>as of</em> given instants (mileage at a service date, D-SRV-4): the bike's seed + its Strava gears'
    /// rides with <c>StartDate &lt; at</c>, via the same <see cref="BikeTotalsCalculator"/> as the live bike totals.
    /// One batch of queries for all points. Points for unknown bikes are left out.
    /// </summary>
    public async Task<IReadOnlyDictionary<(int BikeId, DateTime At), Totals>> ComputeBikeTotalsAtAsync(
        IReadOnlyCollection<(int BikeId, DateTime At)> points)
    {
        var result = new Dictionary<(int BikeId, DateTime At), Totals>();
        if (points is null || points.Count == 0)
        {
            return result;
        }

        var bikeIds = points.Select(p => p.BikeId).Distinct().ToList();
        var bikes = (await BikeRepo.GetByIdsAsync(bikeIds)).ToDictionary(b => b.Id);
        var linksByBike = (await BikeLinkRepo.GetByBikeIdsAsync(bikeIds))
            .GroupBy(l => l.BikeId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<BikeLinkEntity>)g.ToList());

        var gearIds = linksByBike.Values.SelectMany(x => x)
            .Where(l => l.ExternalService == ExternalService.Strava)
            .Select(l => l.ExternalId)
            .Distinct()
            .ToList();
        var activitiesByGear = gearIds.Count == 0
            ? Enumerable.Empty<ActivityAttributionModel>().ToLookup(a => a.GearId)
            : (await ActivityRepo.GetActivitiesByGearAsync(gearIds)).ToLookup(a => a.GearId);

        foreach (var point in points.Distinct())
        {
            if (!bikes.TryGetValue(point.BikeId, out var bike))
            {
                continue;
            }

            bike.Links = linksByBike.TryGetValue(bike.Id, out var links) ? links : Array.Empty<BikeLinkEntity>();

            var totalsByGear = bike.Links
                .Select(l => l.ExternalId)
                .Distinct()
                .ToDictionary(gearId => gearId, gearId =>
                {
                    var rides = activitiesByGear[gearId].Where(a => a.StartDate < point.At).ToList();
                    return new GearTotalsModel
                    {
                        GearId = gearId,
                        DistanceMeters = rides.Sum(a => a.DistanceMeters),
                        MovingTimeSeconds = rides.Sum(a => (long)a.MovingTimeSeconds),
                        ElevationMeters = rides.Sum(a => a.ElevationMeters),
                        ActivityCount = rides.Count
                    };
                });

            var totals = BikeTotalsCalculator.Compute(bike, totalsByGear);
            result[point] = new Totals(totals.DistanceKm, totals.MovingHours, totals.ElevationM, totals.ActivityCount);
        }

        return result;
    }

    /// <summary>
    /// Distance each component did <em>on each bike</em> (cost attribution, D-COST-2): Tracked windows on a bike, the
    /// in-parent overlaps credited to the parent's bike, Manual rows on a bike, and Manual rows in a parent split by the
    /// parent's own per-bike distance. A bike the component was installed on but rode 0 km on is still listed (0), so
    /// callers can fall back to an equal split. The initial seed (km before the purchase) belongs to no bike.
    /// Components never on a bike get an empty map. One batch of queries for the whole set.
    /// </summary>
    public async Task<IReadOnlyDictionary<int, IReadOnlyDictionary<int, decimal>>> ComputeKmByBikeAsync(IReadOnlyCollection<int> componentIds)
    {
        var result = new Dictionary<int, IReadOnlyDictionary<int, decimal>>();
        if (componentIds is null || componentIds.Count == 0)
        {
            return result;
        }

        var installationsByComponent = (await InstallationRepo.GetByComponentsAsync(componentIds))
            .GroupBy(i => i.ComponentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var ctx = await BuildContextAsync(installationsByComponent.Values.SelectMany(x => x).ToList());

        foreach (var componentId in componentIds.Distinct())
        {
            var byBike = new Dictionary<int, decimal>();
            void Add(int bikeId, decimal km) => byBike[bikeId] = byBike.GetValueOrDefault(bikeId) + km;

            foreach (var installation in installationsByComponent.GetValueOrDefault(componentId) ?? new List<InstallationEntity>())
            {
                if (installation.Type == ComponentInstallationType.Manual)
                {
                    var manualKm = installation.ManualDistanceKm ?? 0m;
                    if (installation.BikeId is int manualBike)
                    {
                        Add(manualBike, manualKm);
                    }
                    else if (installation.ParentComponentId is int manualParent
                        && ctx.ParentBikeInstalls.TryGetValue(manualParent, out var parentRows))
                    {
                        foreach (var (bikeId, share) in BikeShares(parentRows, ctx))
                        {
                            Add(bikeId, manualKm * share);
                        }
                    }

                    continue;
                }

                if (installation.DateFrom is null)
                {
                    continue;
                }

                if (installation.BikeId is int bike)
                {
                    Add(bike, SumWindow(bike, installation.DateFrom.Value, installation.DateTo, ctx).DistanceKm);
                }
                else if (installation.ParentComponentId is int parentId
                    && ctx.ParentBikeInstalls.TryGetValue(parentId, out var parentInstalls))
                {
                    foreach (var parentInstall in parentInstalls)
                    {
                        var overlap = Overlap(installation.DateFrom.Value, installation.DateTo, parentInstall.DateFrom!.Value, parentInstall.DateTo);
                        if (overlap is not null)
                        {
                            Add(parentInstall.BikeId!.Value, SumWindow(parentInstall.BikeId!.Value, overlap.Value.From, overlap.Value.To, ctx).DistanceKm);
                        }
                    }
                }
            }

            result[componentId] = byBike;
        }

        return result;
    }

    // A parent's own split across its bikes (by distance; equal when it has no km) — for Manual rows inside it.
    private static IEnumerable<(int BikeId, decimal Share)> BikeShares(IReadOnlyCollection<InstallationEntity> parentOnBikeRows, AttributionContext ctx)
    {
        var km = parentOnBikeRows
            .GroupBy(r => r.BikeId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(r => SumWindow(g.Key, r.DateFrom!.Value, r.DateTo, ctx).DistanceKm));
        var total = km.Values.Sum();

        return km.Select(kv => (kv.Key, total > 0 ? kv.Value / total : 1m / km.Count));
    }

    /// <summary>
    /// Set each Tracked row's per-window mileage in place (detail views). One batch of queries for the whole set.
    /// Manual rows are left at zero (the UI shows their static Manual* values instead).
    /// </summary>
    public async Task ResolveWindowTotalsAsync(IReadOnlyCollection<InstallationEntity> installations)
    {
        if (installations is null || installations.Count == 0)
        {
            return;
        }

        var ctx = await BuildContextAsync(installations);

        foreach (var installation in installations)
        {
            if (installation.Type != ComponentInstallationType.Tracked)
            {
                continue;
            }

            var totals = RowTotals(installation, ctx);
            installation.WindowDistanceKm = totals.DistanceKm;
            installation.WindowMovingHours = totals.MovingHours;
            installation.WindowElevationM = totals.ElevationM;
            installation.WindowActivityCount = totals.ActivityCount;
        }
    }

    // ---- internals ----------------------------------------------------------

    private sealed record AttributionContext(
        IReadOnlyDictionary<int, IReadOnlyList<string>> GearIdsByBike,
        IReadOnlyDictionary<int, List<InstallationEntity>> ParentBikeInstalls,
        ILookup<string, ActivityAttributionModel> ActivitiesByGear);

    private async Task<AttributionContext> BuildContextAsync(IReadOnlyCollection<InstallationEntity> installations)
    {
        // Parent components referenced by any comp-in-comp row → their on-bike windows (one batch). Manual rows in a
        // parent are included too: the per-bike split (ComputeKmByBikeAsync) follows the parent's bikes for them.
        var parentIds = installations
            .Where(i => i.ParentComponentId is not null)
            .Select(i => i.ParentComponentId!.Value)
            .Distinct()
            .ToList();

        var parentBikeInstalls = parentIds.Count == 0
            ? new Dictionary<int, List<InstallationEntity>>()
            : (await InstallationRepo.GetByComponentsAsync(parentIds))
                .Where(i => i.Type == ComponentInstallationType.Tracked && i.BikeId is not null && i.DateFrom is not null)
                .GroupBy(i => i.ComponentId)
                .ToDictionary(g => g.Key, g => g.ToList());

        // Every bike referenced (directly or via a parent) → its Strava gear ids (one batch).
        var bikeIds = installations.Where(i => i.BikeId is not null).Select(i => i.BikeId!.Value)
            .Concat(parentBikeInstalls.Values.SelectMany(x => x).Select(i => i.BikeId!.Value))
            .Distinct()
            .ToList();

        var gearIdsByBike = bikeIds.Count == 0
            ? new Dictionary<int, IReadOnlyList<string>>()
            : (await BikeLinkRepo.GetByBikeIdsAsync(bikeIds))
                .Where(l => l.ExternalService == ExternalService.Strava)
                .GroupBy(l => l.BikeId)
                .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(l => l.ExternalId).Distinct().ToList());

        // All activities for those gear ids (one query) → bucketed in memory.
        var allGearIds = gearIdsByBike.Values.SelectMany(x => x).Distinct().ToList();
        var activitiesByGear = allGearIds.Count == 0
            ? Enumerable.Empty<ActivityAttributionModel>().ToLookup(a => a.GearId)
            : (await ActivityRepo.GetActivitiesByGearAsync(allGearIds)).ToLookup(a => a.GearId);

        return new AttributionContext(gearIdsByBike, parentBikeInstalls, activitiesByGear);
    }

    // Pure: sum the activities that fall in this Tracked row's effective window(s). With a cutoff, every effective
    // window is clipped to end at it (mileage "as of" that instant).
    private static Totals RowTotals(InstallationEntity installation, AttributionContext ctx, DateTime? cutoff = null)
    {
        if (installation.Type != ComponentInstallationType.Tracked || installation.DateFrom is null)
        {
            return Totals.Zero;
        }

        var from = installation.DateFrom.Value;
        var to = installation.DateTo;
        var totals = Totals.Zero;

        if (installation.BikeId is int bikeId)
        {
            var window = Clip(from, to, cutoff);
            if (window is not null)
            {
                totals = totals.Add(SumWindow(bikeId, window.Value.From, window.Value.To, ctx));
            }
        }
        else if (installation.ParentComponentId is int parentId
            && ctx.ParentBikeInstalls.TryGetValue(parentId, out var parentInstalls))
        {
            foreach (var parentInstall in parentInstalls)
            {
                var overlap = Overlap(from, to, parentInstall.DateFrom!.Value, parentInstall.DateTo);
                var window = overlap is null ? null : Clip(overlap.Value.From, overlap.Value.To, cutoff);
                if (window is not null)
                {
                    totals = totals.Add(SumWindow(parentInstall.BikeId!.Value, window.Value.From, window.Value.To, ctx));
                }
            }
        }

        return totals;
    }

    // The window cut to end at the cutoff (no cutoff = unchanged); null = nothing left before it.
    private static (DateTime From, DateTime? To)? Clip(DateTime from, DateTime? to, DateTime? cutoff)
        => cutoff is null ? (from, to) : Overlap(from, to, DateTime.MinValue, cutoff);

    private static Totals SumWindow(int bikeId, DateTime from, DateTime? to, AttributionContext ctx)
    {
        if (!ctx.GearIdsByBike.TryGetValue(bikeId, out var gearIds))
        {
            return Totals.Zero;
        }

        double meters = 0;
        long seconds = 0;
        double elevation = 0;
        var count = 0;

        foreach (var gearId in gearIds)
        {
            foreach (var activity in ctx.ActivitiesByGear[gearId])
            {
                if (activity.StartDate >= from && (to is null || activity.StartDate < to.Value))
                {
                    meters += activity.DistanceMeters;
                    seconds += activity.MovingTimeSeconds;
                    elevation += activity.ElevationMeters;
                    count++;
                }
            }
        }

        return new Totals(
            (decimal)(meters / 1000.0),
            (decimal)(seconds / 3600.0),
            (decimal)elevation,
            count);
    }

    // Intersection of two half-open windows; null = empty (from >= to). null bound = +infinity.
    private static (DateTime From, DateTime? To)? Overlap(DateTime aFrom, DateTime? aTo, DateTime bFrom, DateTime? bTo)
    {
        var from = aFrom > bFrom ? aFrom : bFrom;

        DateTime? to = (aTo, bTo) switch
        {
            (null, null) => null,
            (null, _) => bTo,
            (_, null) => aTo,
            _ => (aTo < bTo ? aTo : bTo)
        };

        if (to is not null && from >= to.Value)
        {
            return null;
        }

        return (from, to);
    }
}

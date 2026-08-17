# BikeTracker — Phase 1d: bike mileage (initial + Σ activities)

**Status:** Done — Infrastructure + WEB compile; Application tests green (143, incl. BikeTotalsCalculatorTests). No migration. Pending: API rebuild + live smoke once the running app is stopped.
**Date:** 2026-08-14

## Context
Bikes are linked to Strava gear (`bt.bike_link.ExternalId` ↔ `strava.activity.gear_id`) and activities are synced, but nothing yet shows a bike's mileage. Phase 1d computes and displays each bike's **totals = initial seed + Σ of its attributed activities** for the three wear metrics (distance / moving time / elevation, D-6), and shows them in the garage + on the bike detail page.

Totals are **computed on-the-fly from source** (D-14 — always correct after edits/re-syncs, no drift; no stored counters). Precomputed projection tables are only needed for the component/installation *chain* (Phase 3); a bike's own total is a simple per-gear sum, cheap at this scale. All activities with the gear id count (incl. trainer/virtual — OQ-17 filtering deferred). Totals are computed regardless of lifecycle, so archived/sold bikes still show their mileage.

Builds on 1c (`.ai/plans/2026-08-13…`, `2026-08-14…`). Backend + WEB, **no schema change, no migration**.

## Decisions (rationale)
- **D-1d-1 On-the-fly totals, not a projection table.** A bike total = `initial + Σ activities` matched by `gear_id` — one grouped query. Recompute-from-source (D-14) keeps it correct after any edit/re-sync. Projections (Phase 3) are for the component chain, not needed here.
- **D-1d-2 All attributed activities count** (any `strava.activity` whose `gear_id` = one of the bike's Strava link ids). Trainer/virtual included for now; OQ-17 (which rides count) deferred.
- **D-1d-3 Totals carried on `BikeEntity` as `[NotMapped]` read-model fields**, set by the bike queries — consistent with the existing `[NotMapped] Links` pattern; keeps `BikeViewModel` the single WEB contract with minimal churn. The sum itself is a small pure helper (`BikeTotalsCalculator`) so it's unit-testable.
- **D-1d-4 Three metrics** (distance km / moving hours / elevation m), each `initial(+0 if null) + synced`. Also expose `AttributedActivityCount` ("from N rides").

## Checklist

### Application
- [ ] `Models/Strava/GearTotalsModel.cs` — `{ string GearId; double DistanceMeters; long MovingTimeSeconds; double ElevationMeters; int ActivityCount; }`.
- [ ] `IActivityRepository.GetGearTotalsAsync(IReadOnlyCollection<string> gearIds)` → `Task<IEnumerable<GearTotalsModel>>`; `EFActivityRepository` impl: group `strava.activity` by `GearId` (where `GearId` in the set), `Sum(Distance)`, `Sum((long)MovingTime)`, `Sum(TotalElevationGain)`, `Count()`. Empty set → empty.
- [ ] `Services/BikeTotalsCalculator.cs` — pure `Compute(BikeEntity bike, IReadOnlyDictionary<string, GearTotalsModel> byGearId)` → `(decimal DistanceKm, decimal MovingHours, decimal ElevationM, int Count)`: sum the bike's Strava-link gear totals, `DistanceKm = InitialDistanceKm + Σm/1000`, `MovingHours = (InitialMovingHours ?? 0) + Σs/3600`, `ElevationM = (InitialElevationM ?? 0) + Σm`.
- [ ] Extend `BikeEntity` with `[NotMapped]` `TotalDistanceKm`, `TotalMovingHours`, `TotalElevationM`, `AttributedActivityCount` (defaults = initial / 0).
- [ ] `GetBikesQuery` + `GetBikeQuery` handlers: after loading bikes+links, collect distinct Strava link `ExternalId`s → `GetGearTotalsAsync` → dict; apply `BikeTotalsCalculator.Compute` per bike onto the `[NotMapped]` fields.

### API
- [ ] `BikeViewModel` += `TotalDistanceKm`, `TotalMovingHours`, `TotalElevationM`, `AttributedActivityCount`; map them in `BikeViewModelMappings.ToViewModel`.

### WEB
- [ ] `Pages/BikeDetails.razor` — add a **"Mileage"** `MudPaper` panel: Distance `TotalDistanceKm` km, Moving time `TotalMovingHours` h, Elevation `TotalElevationM` m, plus a caption "from N Strava rides" when `AttributedActivityCount > 0` (else "manual only — no Strava rides yet"). Keep the existing "Initial metrics" panel (the seed).
- [ ] `Pages/Bikes.razor` — garage card: a distance line/chip (`TotalDistanceKm` km); table: a "Distance" column. Format with `InvariantCulture` like the detail page.

### Tests
- [ ] `BikeTotalsCalculatorTests` — initial-only (no links/activities); initial + one gear; multiple links summed; null initial treated as 0; gear id missing from dict ignored.
- [ ] (`GetGearTotalsAsync` group-by is DB-side → manual verification.)

### Docs
- [ ] `CHANGELOG.md` `## UPCOMMING`; `.ai/README.md` (bike totals = initial + Σ activities via `gear_id`↔`bike_link`, on-the-fly); persist as `.ai/plans/2026-08-14-biketracker-phase-1d-bike-mileage.md`.

## Verification
- `dotnet build src/KomTracker.sln` + targeted `dotnet test` green (new `BikeTotalsCalculatorTests`).
- Manual: a bike linked to a Strava gear with synced rides → **Bikes** shows its total distance; **bike detail** shows Distance/Moving time/Elevation = initial + rides, "from N Strava rides". Edit the initial seed → totals shift by exactly that. Unlink → totals drop back to initial (0 rides). A manual bike with no link shows initial only.

## Out of scope
- OQ-17 ride filtering (trainer/virtual/manual/flagged); per-component mileage + the install-chain attribution + projection tables (Phase 2/3); webhooks (Phase 6).

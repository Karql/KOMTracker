# BikeTracker — retired-gear import (union athlete bikes[] with activity gear_ids)

**Status:** Done — Infrastructure + WEB compile; Application tests green (138). No migration. Pending: API rebuild + live smoke once the running app is stopped.
**Date:** 2026-08-14

## Context
Strava's `GET /athlete` `bikes[]` returns only **active** gear — retired bikes are omitted, so they never reach `strava.bike`. But `GET /gear/{id}` returns a retired bike fine (incl. `retired: true`) if you know its id. We already know historical bike ids from `strava.activity.gear_id`. So at bike-sync time we should fetch details not just for the current `bikes[]` ids but for the **union** of `bikes[]` ids and the DISTINCT bike `gear_id`s seen in the athlete's synced activities. This surfaces retired/historical bikes (a stepping stone to the Excel-import idea) without any extra Strava list endpoint.

Because activities are synced separately, on the very first activation the activity table may be empty → retired bikes appear on a **later** "Sync from Strava" (once rides have imported). That's the intended "come back later and click sync" flow — reinforced by a small UI hint on the Strava-bikes page.

Builds on 1c-i (`.ai/plans/2026-08-13-biketracker-phase-1c-i-strava-bikes.md`). WEB + backend, no schema change, no migration.

## Decisions (rationale)
- **D-ret-1 Union `bikes[]` ids with DISTINCT activity `gear_id`s** at bike sync, then hydrate each via `GET /gear/{id}`. Retired bikes have no summary in `bikes[]`, but `DetailedGear` carries all needed fields (name/nickname/primary/retired/distance/brand/model/frame_type/weight), so hydrating from the id alone is sufficient.
- **D-ret-2 Filter activity gear_ids to bikes only (`b*` prefix).** Strava gear ids are `b<n>` for bikes, `g<n>` for shoes; we sync all activities (incl. runs), so unfiltered gear_ids would drag shoes into `strava.bike`. Filter in the DB query.
- **D-ret-3 No activation reordering.** Keep `ActivateStravaSyncCommand` = gear sync → enable+backfill. First activation imports only active bikes (no activities yet); the next sync (rides now present) imports retired ones. Matches the intended UX; avoids longer/again-coupled first-click.
- **D-ret-4 Small UI hint when "Show retired" is on** — explain Strava doesn't list retired bikes and that they're discovered from synced activities (so sync again later if some are missing). Low-noise, only shown when the toggle is on.

## Checklist

### Backend
- [ ] `IActivityRepository.GetDistinctBikeGearIdsAsync(int athleteId)` → `Task<IEnumerable<string>>`; `EFActivityRepository` impl: `Activity.AsNoTracking().Where(x => x.AthleteId == id && x.GearId != null && x.GearId.StartsWith("b")).Select(x => x.GearId!).Distinct().ToListAsync()`.
- [ ] `IGearService.GetAthleteBikesAsync(int athleteId, string token, IReadOnlyCollection<string> extraGearIds)` — add the param. `GearService`: build a summaries-by-id dict from `athlete.Bikes`; iterate the **union** of `summaries.Keys` and `extraGearIds` (distinct); `GET /gear/{id}` → map `ToStravaBikeEntity` (detailed); on 429/Unauthorized → fail (terminal); on other error → summary fallback if a summary exists for that id, else log + skip (activity-derived ids have no summary).
- [ ] `SyncStravaBikesCommand` handler: fetch `extraGearIds` via `IActivityRepository.GetDistinctBikeGearIdsAsync(athleteId)` and pass to `gearService.GetAthleteBikesAsync(...)`.

### WEB
- [ ] `Pages/StravaBikes.razor` — when `_showRetired` is on, show a short caption/dense alert near the toolbar: "Strava doesn't list retired bikes — they're discovered from your synced activities. If some are missing, sync again after your rides have imported."

### Tests
- [ ] `SyncStravaBikesCommandHandlerTests` — update for the new `GetAthleteBikesAsync` arg + mock `IActivityRepository.GetDistinctBikeGearIdsAsync`; assert the distinct activity gear ids are passed through to the gear service (union input).
- [ ] (Repo LIKE query is DB-side → covered by manual verification, not a unit test.)

### Docs
- [ ] `CHANGELOG.md` `## UPCOMMING`; `.ai/README.md` (bike sync now unions `bikes[]` with activity `gear_id`s → retired bikes); persist this as `.ai/plans/2026-08-14-biketracker-retired-gear-import.md`.

## Verification
- `dotnet build src/KomTracker.sln` + targeted `dotnet test` green.
- Manual: athlete with a retired bike used in older rides → **Sync from Strava** once (backfills activities), then **Sync from Strava** again → the retired bike now appears in `strava.bike` (with `retired = true`); toggle **Show retired** to see it + the hint. A running-only gear (`g…`) never appears as a bike.

## Out of scope
- Excel/manual historical-bike import (future); auto re-sync ordering; **1d** mileage display.

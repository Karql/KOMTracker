# BikeTracker — Phase 1b: activity persistence + sync engine

**Status:** Done — build clean, migration `AddStravaActivitySyncTables` applied to dev DB, tests green (Application 115, Infrastructure 24, Strava 46). (The .NET 10 SDK briefly disappeared mid-session during a VS update — now 10.0.400.) Pending: live smoke via the admin endpoints.
**Date:** 2026-08-11
**Concept:** `docs/biketracker/CONCEPT.md` §4/§6, D-10/D-15. Reality: `docs/strava-api-notes.md`.

## Context
1a added the Strava client. 1b builds the server pipeline: pull each opted-in athlete's activities → store 1:1 in `strava.activity`, two-tier schedule, gated by `strava.athlete_sync`. Mirrors KOM polling (`RefreshSegmentsCommand`/`TrackKomsCommand`). **No UI / gear import / scope-escalation (1c), no attribution (1d).**

## Checklist
- [ ] **Client:** `PolylineMapModel` + `summary_polyline` (activities' list map uses it, not `polyline`) + its test `ToJson`.
- [ ] **Domain** `Entities/Strava/`: `ActivityEntity : BaseEntity` (1:1 with `ActivitySummaryModel`; `long Id` + `int AthleteId` + `gear_id`; `start_date` UTC + `utc_offset` + `timezone`, NO `start_date_local`; `double` metrics; lat/lng split into 4 cols; `summary_polyline`); `AthleteSyncEntity : BaseEntity` (gate: `AthleteId`, **`ActivitiesEnabled`** — generic table, per-capability flag, no telemetry); **`ActivitySyncHistoryEntity : BaseEntity`** (one row per sync run: `Id`, `AthleteId`, `RunAt`, `Duration`, `SyncFrom` (DateTime? — null ⇒ full, date ⇒ window start), `Status`, `UpsertedCount`, `DeletedCount`, `ActivitiesCount` (int? — total stored after run; diagnostics)).
- [ ] **Infra config** `Configurations/Strava/`: `ActivityEntityTypeConfiguration` (`activity`, key `Id` `ValueGeneratedNever`, FK→athlete, index athlete_id+gear_id) + `AthleteSyncEntityTypeConfiguration` (`athlete_sync`, key `AthleteId`, `activities_enabled`) + `ActivitySyncHistoryEntityTypeConfiguration` (`activity_sync_history`, key `Id` generated, FK→athlete, index athlete_id). DbSets + ApplyConfiguration in `KOMDBContext`.
- [ ] **Repos**: `IActivityRepository`/`EFActivityRepository` (`UpsertAthleteActivitiesAsync(...) → Task<int>` deleted count; load in-window ids, stamp audit, `BulkInsertOrUpdateAsync` excl. `AuditCD`, `ExecuteDeleteAsync` diff); `IAthleteSyncRepository`/`EFAthleteSyncRepository` (`GetActivitiesEnabledAthleteIdsAsync`, `GetAsync`, `UpsertAsync`); `IActivitySyncHistoryRepository`/`EFActivitySyncHistoryRepository` (`Add`). Register in `PersistenceDependencyInjection`.
- [ ] **Migration** `AddStravaActivitySyncTables` (EnsureSchema strava, 3 tables, timestamptz, FKs).
- [ ] **Infra Strava**: `ActivityMappings.ToEntity(this ActivitySummaryModel, int athleteId)` (explicit, no AutoMapper — split latlng, map.summary_polyline, utc_offset); `IActivityService`/`ActivityService.GetAthleteActivitiesAsync(athleteId, token, after?)` (+ `GetAthleteActivitiesError`), error-translate like `AthleteService.GetAthleteKomsAsync`. Register in `StravaDependencyInjection`.
- [ ] **App** `Commands/Strava/SyncActivitiesCommand { DateTime? After }` + handler (loop `GetActivitiesEnabledAthleteIdsAsync`: ClearChangeTracker + try/catch + 429-stop; token via GetValidTokenAsync; NO activity:read_all gate — D-1b-8a; upsert with `deleteFrom=After`; **record a `strava.activity_sync_history` row** per athlete: RunAt, SyncFrom (null=full), Status Ok/Error/NoValidToken/RateLimited, counts). `SetAthleteSyncCommand { AthleteId, Enabled }` sets `ActivitiesEnabled`.
- [ ] **Jobs** `SyncActivitiesFullJob` (After=null) + `SyncActivitiesRecentJob` (After=now-7d, const on job); `Startup` triggers gated by `SyncActivitiesJobEnabled`: recent `0 35 1 ? * MON-SAT`, full `0 35 1 ? * SUN`; move clubs → `0 35 0,12 * * ?`. `ApplicationConfiguration.SyncActivitiesJobEnabled` (default true).
- [ ] **FAQ** (WEB): add sync entries, fix clubs time, remove stray "between".
- [ ] **Admin**: `PUT /admin/sync-activities?afterDays=` + `PUT /admin/athlete-sync?athleteId=&enabled=`.
- [ ] **Tests**: handler (enabled-only, 429-stop, After→service+deleteFrom, happy); jobs (After values); `ToEntity` mapping; client `summary_polyline` round-trip.
- [ ] **Docs**: CHANGELOG UPCOMMING; `.ai/README.md` strava schema + pipeline.

## Verification
- `dotnet build src/KomTracker.sln` + `dotnet test` green.
- Migration inspected + `database update`.
- Manual (works with current `activity:read` token — no re-auth): `PUT /admin/athlete-sync?athleteId=<me>&enabled=true` → `PUT /admin/sync-activities` → rows in `strava.activity`; re-run no dupes; delete on Strava + full → row gone; windowed touches only its window. "Only You" rides need 1c's `activity:read_all`.

## Decisions (rationale) — see approved plan for full text
- **D-1b-10 Gate holds only per-capability toggles (`activities_enabled`), telemetry → `strava.activity_sync_history`** (one row per run). *Why:* the gate is a generic per-athlete capability table (room for `gears_enabled` later); full history lets the UI show "last N syncs" and, crucially, the window each run covered (`SyncFrom`: null ⇒ full, else "from <date>"), so a mid-week windowed run doesn't look like the athlete only has 7 days. `RunAt` is the run time regardless of outcome — `SyncedAt` would falsely imply success on `Error`/`RateLimited` rows (AuditCD stays purely diagnostic). Dropped `ActivatedAt`/`LastSyncAt`/`LastStatus` from the gate.
- D-1b-1 `strava` schema. D-1b-2 activity 1:1, no `start_date_local`. D-1b-3 athlete_sync opt-in gate. D-1b-4 single command `{ After }` (no Full/Recent enum; job sets window). D-1b-5 window-scoped delete-detection (works full+recent). D-1b-6 bulk upsert + manual audit. D-1b-7 explicit mapping. D-1b-8 per-athlete isolation + 429-stop. **D-1b-8a no `activity:read_all` gate in 1b** (existing `activity:read` lists all but "Only You" → testable now; read_all completeness is 1c). D-1b-9 `bt.bike_link` deferred to 1c.

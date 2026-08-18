# BikeTracker — Phase 1e: Strava activities page + split sync management
n**Status:** Done (iteration 1) — full solution builds; tests green (Application 150, Infrastructure 35). No migration. Iteration 2 (single-activity refresh) pending. Live smoke pending an app restart.
**Date:** 2026-08-14

## Context
Activities are synced and attributed to bikes, but there's no way to *see* them, and the sync opt-in is fused into one "Sync from Strava" button that both imports gear and turns on activity sync. This phase adds a **Strava activities page** (a big server-paged table) and **splits sync into two independent, user-managed capabilities** — activity auto-sync and bike auto-sync — both toggled on the **Account → Strava** tab (consistent with scope management), each with its own recurring job. Single-activity refresh (the row "Akcje" button) is **iteration 2** (needs a new Strava `GET /activities/{id}` client) — its column is left empty for now.

Design lock: the empty-state / banner logic is driven purely by **(auto-flag, data count)** — no "ever synced" flag needed. `athlete_sync.activities_enabled` / `bikes_enabled` are the auto-sync gates (Account toggles + job gates); the **manual** bike-sync button is independent and does NOT flip the flag.

Builds on 1c/1d. Backend + API + WEB. **No schema change / no migration** (all fields exist).

## Decisions (rationale)
- **D-1e-1 Two independent auto-sync flags, toggled on Account.** `activities_enabled` gates the activity jobs; `bikes_enabled` gates a new bike job. Bike sync no longer turns on activity sync (drop `ActivateStravaSyncCommand`). Enable/disable + state live on the Account Strava tab.
- **D-1e-2 Manual bike sync is independent of the flag.** The bikes-page "Sync bikes from Strava" button runs a quick gear sync anytime and does NOT set `bikes_enabled` (that's the Account toggle's job). Empty-states use `(bikes_enabled, StravaBikeCount)`; activities use `(activities_enabled, activityCount)`.
- **D-1e-3 First activity-enable → immediate full backfill, in the background, once.** Gate on **no history exists** (not the flag flip) so toggling off/on can't hammer Strava's rate limit. The enable endpoint schedules a **Quartz one-shot** `BackfillActivitiesJob(athleteId)` (`StartNow`) so the request returns instantly; the UI says "sync started — may take a while". The nightly full job is the backstop.
- **D-1e-4 Bike auto-sync job at 02:35** (`"0 35 2 * * ?"`, `SyncBikesJobEnabled` gate) loops `bikes_enabled` athletes (per-athlete isolation + 429-stop, mirroring the activity job). Plus the manual button (quick).
- **D-1e-5 Server-side paging** for activities (thousands of rows) — the codebase's first `MudTable ServerData`. New paged repo method + `PagedResult<T>` VM + endpoint. Default sort `StartDate` desc; page sizes 20/50/100.
- **D-1e-6 Activities page is read-only display** (no summaries per user). "Last updated" header (latest history `RunAt`) opens a **sync-history dialog**. Sync-state shown like the bikes page; enable action links to Account. "Rower" column: linked bike → name link to `/bikes/{id}`; gear present but unlinked → "Nieprzypisany" + link to the Strava-bikes page; no gear → "—".

## Checklist

### Application — sync split + jobs + queries
- [ ] **Drop `ActivateStravaSyncCommand`**. `SyncStravaBikesCommand`: remove the `SetBikesEnabledAsync(true)` line (manual sync ≠ enabling auto).
- [ ] Rename `SetAthleteSyncCommand` → **`SetActivitySyncCommand { AthleteId, Enabled }`**; handler sets `activities_enabled` (via a new `IAthleteSyncRepository.SetActivitiesEnabledAsync` mirroring `SetBikesEnabledAsync`, so it preserves `bikes_enabled`), and returns a result flag **`BackfillNeeded`** = `Enabled && !await historyRepo.AnyForAthleteAsync(athleteId)`. It does NOT run the backfill itself.
- [ ] **`SetBikeSyncCommand { AthleteId, Enabled }`** → `SetBikesEnabledAsync`.
- [ ] `IActivitySyncHistoryRepository`: add `Task<bool> AnyForAthleteAsync(int athleteId)` + `Task<IEnumerable<ActivitySyncHistoryEntity>> GetRecentByAthleteAsync(int athleteId, int take)` (order `RunAt` desc); EF impls.
- [ ] `IAthleteSyncRepository.GetBikesEnabledAthleteIdsAsync()` + EF impl (`Where(x => x.BikesEnabled)`), and `SetActivitiesEnabledAsync`.
- [ ] **`SyncBikesCommand { int? AthleteId }`** + handler: loop `GetBikesEnabledAthleteIdsAsync()` (or the one athlete), per-athlete `try/catch` + `ClearChangeTracker`, run the gear-sync (reuse the `SyncStravaBikesCommand` body/service), stop the whole run on a rate-limit failure (mirror `SyncActivitiesCommand`). Used by the bike job.
- [ ] **Queries:** `GetStravaActivitiesQuery { AthleteId, UserId, Page, PageSize }` → `PagedResultModel<ActivityListItemModel>` (count via `CountAthleteActivitiesAsync`; page via a new `IActivityRepository.GetActivitiesPageAsync(athleteId, skip, take)` ordered `StartDate` desc; resolve each row's bike via `IBikeLinkRepository.GetByExternalIdsAsync` + `IBikeRepository.GetBikesAsync` names — mirror `GetStravaBikesQuery`). `GetActivitySyncHistoryQuery { AthleteId, Take }` → history rows.
- [ ] `ActivityListItemModel` (Id, Name, SportType, DistanceMeters, MovingTimeSeconds, AverageSpeedMps, ElevationMeters, StartDateUtc, UtcOffset, GearId, LinkedBikeId?, LinkedBikeName?).

### Infrastructure — jobs
- [ ] `Infrastructure/Jobs/SyncBikesJob` (copy `SyncActivitiesRecentJob` shape) → `SyncBikesCommand{}`. `Infrastructure/Jobs/BackfillActivitiesJob` → reads `athleteId` from `JobDataMap` → `SyncActivitiesCommand{ After=null, AthleteId }`.
- [ ] `Startup.cs`: `AddTransient` both jobs; cron-schedule `SyncBikesJob` `"0 35 2 * * ?"` `InTimeZone(tz)` gated by `SyncBikesJobEnabled`. `ApplicationConfiguration.SyncBikesJobEnabled = true`.

### API
- [ ] `PagedResultViewModel<T> { T[] Items; int TotalCount; }` + `ActivityViewModel` (mapped km/kmh/date-local server-side or raw + WEB formats) + `ActivitySyncHistoryViewModel`, in `API.Shared/ViewModels/BikeTracker/`.
- [ ] `StravaBikesController`: replace `POST sync` (was `ActivateStravaSyncCommand`) with **`POST sync-bikes`** → `SyncStravaBikesCommand` (manual). Add **`PUT activity-sync {enabled}`** → `SetActivitySyncCommand`; if `BackfillNeeded`, schedule the one-shot `BackfillActivitiesJob` via `ISchedulerFactory` and return `{ backfillStarted: true }`. Add **`PUT bike-sync {enabled}`** → `SetBikeSyncCommand`. Add **`GET activities?page=&pageSize=`** → `GetStravaActivitiesQuery`. Add **`GET activity-sync-history`** → `GetActivitySyncHistoryQuery`. (Admin `athlete-sync` endpoint → point at `SetActivitySyncCommand`.)

### WEB
- [ ] **`Pages/StravaActivities.razor`** (+ `.razor.cs`), route `/bike-tracker/strava-activities` — `MudTable` with `ServerData` (first server-paged table; reuse the `QueryHelpers.AddQueryString` + `Http.GetFromJsonAsync<PagedResult>` convention). Columns: **Date**(local `StartDate+UtcOffset`, `MudLink Target=_blank` → `https://www.strava.com/activities/{Id}`), **Name (SportType)**, **Distance** (`N1` km), **Moving time** (h/m), **Avg speed** (`N1` km/h = m/s·3.6), **Elevation** (m), **Rower** (linked → `/bikes/{id}`; unlinked-gear → "Nieprzypisany" + link to `bike-tracker/strava-bikes`; no gear → "—"), **Akcje** (empty — iteration 2). Header: "Last updated {relative}" (from history) → opens `StravaSyncHistoryDialog`; activity-sync state chip. Placeholder when `!activities_enabled && count==0` (info + "Enable on Account" → `account?tab=strava`); top info banner when `!activities_enabled && count>0` (same link). No summary tiles.
- [ ] `Shared/StravaSyncHistoryDialog.razor` — table of recent history (RunAt local, Duration, SyncFrom (full/from date), Status, Upserted/Deleted, ActivitiesCount) from `GET activity-sync-history`.
- [ ] **`Pages/Account.razor` (Strava tab)** — add two `MudSwitch`es: "Sync activities automatically" (`activities_enabled`) and "Sync bikes automatically" (`bikes_enabled`), bound to the new `PUT activity-sync` / `PUT bike-sync`. On activity enable returning `backfillStarted` → Snackbar "Activity sync started — this may take a while." Reload status after toggle.
- [ ] **`Pages/StravaBikes.razor`** — remove the "Activity sync on/off" chip; keep the manual **Sync bikes from Strava** button (always available → `POST sync-bikes`). Empty-state: `!bikes_enabled && count==0` → placeholder (with the manual sync button); `!bikes_enabled && count>0` → info banner "Automatic bike sync is off — enable on Account" (→ `account?tab=strava`) + list. Keep the `!HasActivityReadAll` alert.
- [ ] `Shared/NavMenu.razor` — add `<MudNavLink Href="bike-tracker/strava-activities">Strava activities</MudNavLink>` in the Bike Tracker group.

### Tests
- [ ] `SetActivitySyncCommandHandlerTests` (sets flag; `BackfillNeeded` true only when enabling with no history; false when history exists / when disabling). `SetBikeSyncCommandHandlerTests`. `SyncBikesCommandHandlerTests` (loops enabled athletes; 429 stops). Update `SyncStravaBikesCommandHandlerTests` (no longer sets bikes_enabled). Remove `ActivateStravaSyncCommandHandlerTests`. `GetStravaActivitiesQueryHandlerTests` (paging + bike resolution).

### Docs
- [ ] `CHANGELOG.md` `## UPCOMMING`; `.ai/README.md` (activities page + split sync + bike job + history dialog); persist as `.ai/plans/2026-08-14-biketracker-phase-1e-activities-page.md`.

## Verification
- `dotnet build src/KomTracker.sln` + targeted `dotnet test` green.
- Manual: **Account → Strava** shows both auto-sync switches. Enable activity sync (fresh) → snackbar "may take a while", history fills in the background, activities appear. **Bike Tracker → Strava activities**: paged table (change page/size), Date links to Strava, Rower links to the bike (or Strava-bikes when unlinked), "Last updated" opens the history dialog. Disable activity sync → banner appears over the (still-listed) activities; with none synced → placeholder. Bikes page: manual "Sync bikes" works with auto off; enabling bike auto-sync on Account makes the 02:35 job pick it up.

## Out of scope (iteration 2+)
- **Single-activity refresh** (row "Akcje"): Strava `GET /activities/{id}` client + service + command + button.
- Activity detail view; column sorting/filtering on the activities table; per-component mileage (Phase 2/3); webhooks (Phase 6).

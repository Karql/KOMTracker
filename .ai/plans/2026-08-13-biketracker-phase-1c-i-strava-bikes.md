# BikeTracker — Phase 1c-i: Strava bikes (gear) sync + opt-in activation + link/create

**Status:** Done — build + migration applied, tests green (Application 129, Infrastructure 33). Pending: live smoke via the WEB page.
**Date:** 2026-08-13
**Concept:** `docs/biketracker/CONCEPT.md` §4 (Bike/BikeLink/Activity), §6 (opt-in activation), D-2, D-10, D-16. Reality: `docs/strava-api-notes.md`.

## Context
Phase 1b (done, tested) lands server-side **activity** sync (`strava.activity`, the `strava.athlete_sync` gate, history, jobs) but activation is admin-only and nothing connects a synced activity's `gear_id` to a user's garage. Phase 1c-i delivers the user-facing layer, following the concept's first-class-Strava-data rule (D-10): sync the athlete's Strava **gear** into a raw `strava.bike` mirror, show it on a dedicated **"Strava bikes"** page, and let the user **create** a BikeTracker bike from a Strava one (copying fields) **or link** a Strava bike to an existing `bt.bike` — the coupling lives in `bt.bike_link`.

Gear is NOT imported straight into `bt.bike`. It is mirrored 1:1 into `strava.bike` (first-class Strava record, like `strava.activity`); `bt.bike_link` is created only when the user materializes/links a bike — the WatchMyBike "separate Strava-bikes tab with a create-from-this action" pattern.

**Split:** the **scope-escalation re-auth** (`activity:read_all` via IdentityServer4 `prompt=login`+`acr_values` reconnect) is deferred to **Phase 1c-ii**. 1c-i works with the current token (gear needs only the already-granted `profile:read_all`; per D-1b-8a `activity:read` syncs all-but-"Only You"), and only *surfaces* whether `activity:read_all` is present.

## Checklist

### Domain
- [ ] `Entities/Strava/StravaBikeEntity.cs : BaseEntity` (`strava.bike`) — raw gear mirror: `string Id` (gear id, e.g. `b1234567`), `int AthleteId` (FK→athlete), `Name`, `Nickname?`, `bool Primary`, `bool Retired`, `double Distance` (m), `double ConvertedDistance` (km), `BrandName?`, `ModelName?`, `int? FrameType`, `Description?`, `double? Weight` (kg).
- [ ] `Entities/Bike/BikeLinkEntity.cs : BaseEntity` — `int Id`, `int BikeId` (FK→`bt.bike`), `ExternalService ExternalService`, `string ExternalId`.
- [ ] `Entities/Bike/ExternalService.cs` — enum `{ Strava, Other }` (string, D-13 style).
- [ ] Extend `Entities/Strava/AthleteSyncEntity.cs` (1b) with `bool BikesEnabled` (D-1c-10).

### Infrastructure — persistence
- [ ] `Configurations/Strava/StravaBikeEntityTypeConfiguration.cs` — `ToTable("bike","strava")`, key `Id` `ValueGeneratedNever`, FK→athlete, snake columns, index `AthleteId`.
- [ ] `Configurations/Bike/BikeLinkEntityTypeConfiguration.cs` — `ToTable("bike_link","bt")`, key `Id` generated, FK→`BikeEntity`, **unique index `(ExternalService, ExternalId)`** + index `BikeId`, maxlen 50.
- [ ] `KOMDBContext` — DbSets `StravaBike`, `BikeLink` + `ApplyConfiguration`s.
- [ ] Repos: `IStravaBikeRepository`/`EFStravaBikeRepository` (`UpsertAthleteBikesAsync(athleteId, bikes)` bulk upsert + manual audit like `EFActivityRepository`; `GetByAthleteAsync(athleteId)`); `IBikeLinkRepository`/`EFBikeLinkRepository` (`Add`, `GetByBikeIdsAsync`, `ExistsAsync(service, externalId)`). Register in `PersistenceDependencyInjection`.
- [ ] `AthleteSyncEntityTypeConfiguration` (1b) — add `bikes_enabled` column.
- [ ] **Migration** `AddStravaBikeAndBikeLinkTables` (also adds `strava.athlete_sync.bikes_enabled`; no token change).

### Infrastructure — Strava gear service + mapping
- [ ] `Interfaces/Services/Strava/IGearService.cs` (App) + `GetAthleteBikesError` (Unauthorized/TooManyRequests/UnknownError).
- [ ] `Strava/Services/GearService.cs : IGearService` — `GetAthleteBikesAsync(athleteId, token)`: `_athleteApi.GetAthleteAsync(token)` → `Bikes[]` summaries (incl. retired), hydrate each via `_gearApi.GetGearAsync(id, token)`, map to `StravaBikeEntity`. Error-translate like `ActivityService`. Register in `StravaDependencyInjection`.
- [ ] `Strava/Mappings/GearMappings.cs` — `ToStravaBikeEntity(athleteId)`; `FrameTypeToBikeType(int?)`; `ToNewBikeEntity(this StravaBikeEntity, userId)` (field copy + seeding, D-1c-8).

### Application — commands (composable internals, D-1c-5)
- [ ] `Commands/Strava/SyncStravaBikesCommand.cs { int AthleteId }` (**gear only**): token → `gearService.GetAthleteBikesAsync` → `stravaBikeRepo.UpsertAthleteBikesAsync` (all, incl retired) → set `athlete_sync.BikesEnabled=true` (does NOT touch `ActivitiesEnabled`). 429/unauthorized surfaced.
- [ ] **Activity-sync opt-in** — `SetActivitySyncCommand { int AthleteId; bool Enabled }` (or extend 1b `SetAthleteSyncCommand`): upserts `ActivitiesEnabled`; **on a fresh enable** sends `SyncActivitiesCommand{ After=null, AthleteId }`. Idempotent.
- [ ] `Commands/Strava/ActivateStravaSyncCommand.cs { int AthleteId; string UserId }` — orchestrator for the single "Sync from Strava" button: `SyncStravaBikesCommand` then the activity opt-in (enable).
- [ ] `Commands/Strava/LinkStravaBikeCommand.cs { int BikeId; string StravaGearId; string UserId; int AthleteId }` — validate the `bt.bike` is the caller's + the gear is the caller's `strava.bike` + not already linked → add `BikeLinkEntity`.
- [ ] Extend `SaveBikeCommand`/`SaveBikeViewModel` with optional `string? StravaGearId` — on **create** with it set, also add the `bt.bike_link`. Ignored on update.
- [ ] Extend `SyncActivitiesCommand` with optional `int? AthleteId` (null = all enabled; set = that one).

### API
- [ ] `Controllers/StravaBikesController.cs` (`[BearerAuthorize]`, `GetCurrentUser()` → UserId+AthleteId):
  - `POST /bike-tracker/strava/sync` → `ActivateStravaSyncCommand`.
  - `GET  /bike-tracker/strava/bikes` → `strava.bike` list; each item carries `LinkedBikeId?`/`LinkedBikeName?`.
  - `GET  /bike-tracker/strava/sync-status` → `{ bikesEnabled, activitiesEnabled, hasActivityReadAll, stravaBikeCount }`.
  - `POST /bike-tracker/strava/bikes/{gearId}/link` → `LinkStravaBikeCommand{ BikeId }`.
- [ ] ViewModels in `KomTracker.API.Shared/ViewModels/BikeTracker/` (`StravaBikeViewModel`, `StravaSyncStatusViewModel`); add `StravaGearId?` to `SaveBikeViewModel`.

### WEB
- [ ] `Pages/StravaBikes.razor` (+ `.razor.cs`), route `/bike-tracker/strava-bikes`:
  - Primary **"Sync from Strava"** button (POST `/strava/sync`), status line (activities on/off + count), `!hasActivityReadAll` info `MudAlert`.
  - **Empty-state:** `bikesEnabled==false` → placeholder + CTA in place of the list; `bikesEnabled==true` & 0 bikes → "no bikes on your Strava"; else list.
  - `strava.bike` list modelled on garage `Bikes.razor` (search, "Show retired" switch, card/table toggle via `IPreferenceService` new key). Per item: name, brand/model, distance, primary/retired chips; **"Linked" badge** → `/bikes/{linkedBikeId}` when linked, else **"Create bike"** (pre-fill `AddEditBikeDialog` + smuggled `StravaGearId`) + **"Link to existing"**.
  - Errors via `ShowProblemAsync`.
- [ ] `AddEditBikeDialog` — optional pre-fill defaults + hidden `StravaGearId` passed to `SaveBikeViewModel`.
- [ ] `Shared/NavMenu.razor` — `<MudNavLink Href="bike-tracker/strava-bikes">Strava bikes</MudNavLink>` in the Bike Tracker group.
- [ ] `Pages/Faq.razor` — one line: activity sync is opt-in via the Strava bikes page.

### Tests
- [ ] `SyncStravaBikesCommandHandlerTests`; activity-sync opt-in test; `ActivateStravaSyncCommandHandlerTests`; `LinkStravaBikeCommandHandlerTests`; `SaveBikeCommandTests` (StravaGearId link on create); `GearMappingsTests`; `SyncActivitiesCommandHandlerTests` (AthleteId filter).

### Docs
- [ ] `CHANGELOG.md` `## UPCOMMING`; `.ai/README.md` (`strava.bike`, `bt.bike_link`, sync/create/link flow).

## Verification
- `dotnet build src/KomTracker.sln` + targeted `dotnet test` green.
- Migration inspected (`strava.bike`, `bt.bike_link` FK + unique index, `athlete_sync.bikes_enabled`) + `dotnet ef database update`.
- Manual: **Bike Tracker → Strava bikes** → empty-state placeholder → **Sync from Strava** → `strava.bike` rows (incl retired), `ActivitiesEnabled=true`, activities backfilled with matching `gear_id` → **Create bike** → `bt.bike` + `bt.bike_link`; garage shows it → **Link to existing** → link only → re-sync → gear refreshed, links intact, activities NOT re-paged. Info alert flags missing `activity:read_all`.

## Decisions (rationale)
- **D-1c-1 Split** — 1c-i (sync + create/link) now, scope-escalation re-auth (1c-ii) later. Gear + activation work with the current token; isolating the IdentityServer4 re-auth de-risks the testable work.
- **D-1c-2 Raw gear → `strava.bike`** (first-class `strava.*`, D-10), like `strava.activity`. Split by type now (`strava.bike`; `strava.shoe` future) rather than `strava.gear`+discriminator — bike/shoe carry different fields.
- **D-1c-3 `bt.bike_link` = the ONLY bt↔external coupling** (D-2). `{ BikeId, ExternalService, ExternalId }`, 1 bike→N links, unique `(ExternalService, ExternalId)`, created only at create/link. `bike_link`→`bike` is a real same-schema FK; `bt`↔`strava` stays a soft `gear_id`↔`ExternalId` match (D-10).
- **D-1c-4 Sync mirrors ALL gear incl. retired**; materializing a `bt.bike` is user-initiated per bike.
- **D-1c-5 One user-facing activation action, composable internals.** Single "Sync from Strava" = gear sync + enable activities + backfill; underlying `SyncStravaBikesCommand` vs activity opt-in stay separate so a future "activities" tab / gear-only refresh need no redesign. Backfill-smart re-run.
- **D-1c-6 Create = reuse `AddEditBikeDialog` pre-filled + smuggled gear id** (save writes the link too); "Link to existing" is a separate no-copy action.
- **D-1c-7 Dedicated "Strava bikes" page** (not a garage button) — distinct model; keeps the garage focused on `bt.bike`; matches WatchMyBike.
- **D-1c-8 `frame_type→BikeType` + pre-fill at CREATE** (sync stores raw `frame_type` int). Map the full Strava set {1 Mountain, 2 Cyclocross, 3 Road, 4 TimeTrial, 5 Gravel}, else Other. Pre-fill Brand/Model/`WeightKg` + Name from the gear. **Do NOT seed `InitialDistanceKm`** — mileage accrues from synced activities (1d), so seeding gear distance would double-count.
- **D-1c-11 Linked state is visible + reversible on both sides.** `bt.bike` carries its links (loaded by the bike queries, `[NotMapped]`, not an EF nav) → `BikeViewModel.StravaGearId`; garage list/detail show a "Strava" chip, the Strava-bikes list badges the linked bike. `UnlinkStravaBikeCommand` (`DELETE /bike-tracker/strava/bikes/{gearId}/link`) removes the `bt.bike_link`, callable from the garage (bike menu/detail) and the Strava-bikes page.
- **D-1c-9 `SyncActivitiesCommand.AthleteId?` filter** — activation backfills only the activating athlete.
- **D-1c-10 New `athlete_sync.bikes_enabled` flag + empty-state** — disambiguates never-synced vs synced-but-no-bikes; gating a periodic gear-refresh job on it is deferred.

## Out of scope
- **1c-ii** scope-escalation re-auth; **1d** bike mileage; activity sync triggering linked-bike projection refresh; periodic gear-refresh job on `bikes_enabled`; `strava.shoe`; webhooks (Phase 6); components/installations (Phase 2/3).

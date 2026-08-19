# BikeTracker — Phase 1e (iteration 2): single-activity refresh + Strava API notes

## Context
Phase 1e shipped the Strava activities page, but the per-row **"Actions"** column was intentionally left empty (iteration 1 note: *"single-activity refresh is iteration 2 — needs a Strava `GET /activities/{id}` client"*). This iteration adds that button: a user viewing a stale row (e.g. they just re-assigned the bike on Strava, or renamed the ride) can pull the latest state of **one** activity on demand, without waiting for the nightly full sync.

Per the user: build a proper **DetailedActivity** model in the Strava client (`GET /activities/{id}`), but **persist exactly as today** — reuse the existing summary→entity mapping, no new DB columns, no migration. The single-activity upsert must **NOT** run the list-sync's delete-detection (that would wipe every other activity in the window).

Plus two documentation tasks: (a) record in `docs/strava-api-notes.md` the previously-discovered fact that `GET /athlete` `bikes[]` omits **retired** gear (only `GET /gear/{id}` returns them); (b) add the DetailedActivity sample. Other samples (exchange athlete, `GET /athlete`, `GET /gear/{id}`, clubs) are already in the notes.

Backend + Strava client + API + WEB. **No schema change / no migration** (persistence reuses `ActivityEntity` unchanged).

## Decisions (rationale)
- **D-1e2-1 The client's DetailedActivity model reflects the FULL API payload, not a trimmed subset.** `GET /activities/{id}` returns a superset of SummaryActivity; model it as `ActivityDetailedModel : ActivitySummaryModel` with **every field the endpoint returns**, including the nested collections (`segment_efforts[]`, `best_efforts[]`, `splits_metric[]`, `splits_standard[]`, `laps[]`, `photos`, `gear`, `similar_activities`, `stats_visibility[]`, `available_zones[]`, `embed_token`, `calories`, `description`, `perceived_exertion`, …). *Why: the Strava client is a universal connector — it mirrors the API so any future consumer (activity detail view, webhooks, analytics) can use it without touching the client. That we currently persist only summary fields is irrelevant to the client's shape (same principle as `AthleteDetailedModel` carrying `bikes[]`/`shoes[]`).* Persistence is unaffected: because `ActivityDetailedModel : ActivitySummaryModel`, the existing `ActivityMappings.ToEntity(this ActivitySummaryModel, athleteId)` maps it verbatim. **Reuse** existing client types: `segment_efforts[]`/`best_efforts[]` → `List<SegmentEffortDetailedModel>`, `gear` → `GearSummaryModel`, `map` → `PolylineMapModel`, `athlete` → `AthleteMetaModel` (both already on the summary).
- **D-1e2-2 Single-activity upsert has no delete-detection.** New `IActivityRepository.UpsertActivityAsync(ActivityEntity)` upserts one row (audit-stamped like the bulk path) and never calls `ExecuteDelete`. *Why: `UpsertAthleteActivitiesAsync` deletes window rows absent from the fetched set — with a single fetched activity that would delete everything else.*
- **D-1e2-3 Cross-athlete guard.** The service verifies the fetched activity's `athlete.id` equals the requesting athlete; a mismatch returns **NotFound** rather than writing a foreign row under the caller's `AthleteId`. *Why: the id comes from the client; without the check a user could pollute their own `strava.activity` with another athlete's public ride.*
- **D-1e2-4 Refresh does not write sync history.** A targeted refresh is not a "sync run"; it stays out of `activity_sync_history` so the "Last updated" header keeps reflecting real batch syncs.
- **D-1e2-5 WEB reloads the current page after refresh.** The button POSTs, then calls `_table.ReloadServerData()` to re-pull the visible page (which re-resolves bike links) instead of patching a single row client-side. *Why: bike-link/name resolution already lives server-side in `GetStravaActivitiesQuery`.*
- **D-1e2-6 The command is a self-contained, webhook-reusable primitive.** `SyncActivityCommand { AthleteId, ActivityId }` — keyed purely by ids, acquiring its own token via `_athleteService.GetValidTokenAsync(athleteId)`, with **no dependency on the HTTP user / `GetCurrentUser`**. The UI "refresh" endpoint is just one caller; a future Strava webhook handler (Phase 6) receives `{ owner_id, object_id, aspect_type }` and, for `create`/`update`, dispatches the **same** command via MediatR — no rework. Webhook **delete** is a separate future command over `IActivityRepository.DeleteActivityAsync(athleteId, activityId)` (out of scope now).

## Checklist

### Strava client (`src/Strava/Strava.API.Client`)
- [ ] `Model/Activity/ActivityDetailedModel.cs` — `ActivityDetailedModel : ActivitySummaryModel` covering the full payload; reuses `SegmentEffortDetailedModel`, `GearSummaryModel`.
- [ ] New nested models under `Model/Activity/`: `SplitModel`, `LapModel`, `PhotosSummaryModel` (+ `PrimaryPhotoModel`), `SimilarActivitiesModel` (+ `ActivityTrendModel`), `StatVisibilityModel`.
- [ ] `Model/Activity/Error/GetActivityError.cs` — `Unauthorized`/`TooManyRequests`/`NotFound`/`UnknownError`.
- [ ] `Api/IActivityApi.cs` + `Api/ActivityApi.cs` — `GetActivityAsync(long activityId, string token)` → `GET /activities/{id}`, mirroring `GearApi.GetGearAsync` (adds 404 → `NotFound`).

### Application
- [ ] `IActivityService.GetAthleteActivityAsync(int athleteId, string token, long activityId)` + `NotFound` on `GetAthleteActivitiesError`.
- [ ] `IActivityRepository.UpsertActivityAsync(ActivityEntity)`.
- [ ] `Commands/Strava/SyncActivityCommand.cs` — command + handler (token → fetch → map errors to `NotFoundError`/generic → upsert).

### Infrastructure
- [ ] `ActivityService.GetAthleteActivityAsync` — athlete guard + error translation + `ToEntity`.
- [ ] `EFActivityRepository.UpsertActivityAsync` — single-row `BulkInsertOrUpdateAsync`, audit stamp, no delete.

### API
- [ ] `StravaBikesController` — `POST activities/{id}/refresh` → `SyncActivityCommand`.

### WEB
- [ ] `StravaActivities.razor` (+ `.cs`) — Actions column, per-row refresh button w/ spinner, Snackbar + `ReloadServerData`.

### Docs
- [ ] `docs/strava-api-notes.md` — DetailedActivity section + retired-bikes note + `frame_type` map.
- [ ] `CHANGELOG.md` `## UPCOMMING`.
- [ ] `.ai/README.md` — activity refresh + `SyncActivityCommand` webhook note.

### Tests
- [ ] `SyncActivityCommandHandlerTests`; `ActivityApi`/`ActivityService` tests (full-payload deserialize, 404/401/mismatch).

## Verification
- `dotnet build src/KomTracker.sln`; `dotnet test src/KomTracker.sln` green.
- Manual (needs app restart): Strava activities → row Refresh → spinner → Snackbar + row re-renders; other rows untouched; foreign activity id → 404.

## Out of scope (later)
- Activity detail view; column sorting/filtering.
- **Webhooks (Phase 6)** — reuse `SyncActivityCommand` for create/update; delete command over a future `DeleteActivityAsync`.

# BikeTracker — Phase 1a: Strava activity + gear client

**Status:** Done — build clean, Strava client tests 44 green (33 existing + 11 new). No app wiring (that's 1b).
**Date:** 2026-08-11
**Concept:** `docs/biketracker/CONCEPT.md` (§4 Activity, §6 Strava import, D-10/D-15). Reality: `docs/strava-api-notes.md`.

## Context
Phase 0 (bikes CRUD) done. Phase 1 (Strava integration) is large → sliced. **1a = extend the standalone `Strava.API.Client`** with the activity + gear endpoints BikeTracker needs. No DB, no jobs, no OAuth. Fully unit-testable (MockHttp). Unblocks 1b (persistence + sync), 1c (opt-in + scope escalation), 1d (bike mileage).

Today: only `Model/Activity/ActivityMetaModel.cs` (id + resource_state) and no gear. Mirror the existing client conventions (`Api/AthleteApi.cs` = template).

## Checklist
- [ ] Persist this spec (done by writing this file).
- [ ] `Model/Activity/ActivitySummaryModel.cs : ActivityMetaModel` — full field set from the real payload; dates per D-15; **`utc_offset` (float) hand-added** (absent from OpenAPI); reuse `PolylineMapModel`.
- [ ] `Model/Activity/Error/GetActivitiesError.cs : BaseError`.
- [ ] `Api/IActivityApi.cs` + `Api/ActivityApi.cs` — `GetActivitiesAsync(token, after?, before?)`: paginate `per_page=200`, **stop on a non-full page**, materialize to `List` → `IEnumerable`; 401/429/unknown handling cloned from `AthleteApi.GetKomsAsync`; `after`/`before` epoch query params when set.
- [ ] `Model/Gear/GearSummaryModel.cs` (id string, resource_state, primary, name, distance metres) + `GearDetailedModel.cs : GearSummaryModel` (brand_name, model_name, frame_type int, description).
- [ ] `AthleteSummaryModel.cs` — add `bikes[]` (+ `shoes[]`).
- [ ] `Api/IGearApi.cs` + `Api/GearApi.cs` — `GetGearAsync(gearId, token)` → `GET /gear/{id}`; `Model/Gear/Error/GetGearError.cs : BaseError`.
- [ ] `DependencyInjection.cs` — register `IActivityApi`/`IGearApi`.
- [ ] Tests: `ActivitySummaryModelExtensions.ToJson()` (+ list) & `GearDetailedModelExtensions.ToJson()`; `Api/ActivityApiTests.cs` (paginate/short-page/empty/401/429/unknown/after+before params); `Api/GearApiTests.cs` (happy/401/unknown).
- [ ] `dotnet test` Strava client green + existing 33 pass; `dotnet build src/KomTracker.sln` clean.
- [ ] `CHANGELOG.md` `## UPCOMMING`.

## Decisions
- **D-1a-1** Mirror existing client conventions exactly (POCO + `[JsonPropertyName]`, meta→summary inheritance, `Result<T>`, `XxxError : BaseError`, hardcoded v3 URLs, `IHttpClientFactory` + Bearer). *Why:* consistency; `AthleteApi` is the proven template.
- **D-1a-2** `utc_offset` (+ any real-but-unspecced fields) added by hand (float). *Why:* missing from the OpenAPI `SummaryActivity` schema but present in real responses (D-15).
- **D-1a-3** SummaryActivity only (no DetailedActivity). *Why:* the list endpoint returns Summary; per-activity detail isn't needed until a feature requires it.
- **D-1a-4** Paginate **until an empty page** — identical to `AthleteApi.GetKomsAsync` (consistency + trivial tests; the one extra empty request only happens on the rare full pass). Error `Result` short-circuits so a 429 stops the pull.
- **D-1a-5** Materialize all pages into a `List` → `IEnumerable` (like `GetKomsAsync`); no streaming, no DB page-cursor. *Why:* ~8k activities ≈ 40 req / ~30–40 MB, only on the rare weekly full pass (two-tier design; daily pass is `after=now-7d`); a persisted cursor is the complication the concept rejects. Private per-page method remains to build on if memory ever bites.
- **D-1a-6** Gear on a dedicated `GearApi`; `bikes[]`/`shoes[]` on `AthleteSummaryModel`. *Why:* one Api per resource (matches Athlete/Segment/Club).
- **D-1a-7** No `strava.*`/DB/OAuth in 1a. *Why:* mapping→entity, sync job, opt-in, scope escalation are 1b/1c.
- **D-1a-8** Athlete model correctness: `GET /athlete` returns Strava's **DetailedAthlete**, so `AthleteApi.GetAthleteAsync` now returns a new **`AthleteDetailedModel : AthleteSummaryModel`** carrying `bikes[]`/`shoes[]` (moved off `AthleteSummaryModel`, where I had wrongly put them). *Why:* the model was conflated. `weight`/`bio`/`username` stay on `AthleteSummaryModel` because the **token-exchange** athlete (`TokenWithAthleteModel`) is a fat payload that carries them (so Summary keeps them; only the true Detailed-only `bikes`/`shoes` move). The `StravaApiClientProfile` `AthleteSummaryModel→AthleteEntity` map gets `.IncludeAllDerived()` so the Detailed result still maps in the KOM refresh flow. Naming stays entity-first (`AthleteSummary`/`AthleteDetailed`). No behavior change for KOM (build + 176 tests green).

## Verification
- `dotnet build src/KomTracker.sln` clean.
- `dotnet test src/Strava/Strava.API.Client.Tests/Strava.API.Client.Tests.csproj` — new tests green, existing 33 pass.
- No DB/app-run in 1a (nothing wired into API/WEB yet — that's 1b).

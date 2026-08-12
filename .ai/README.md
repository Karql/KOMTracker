# KOMTracker — project overview (for AI assistants)

> Living overview kept up to date as the project evolves. Conversation with the maintainer is in Polish; **everything committed to the repo is in English** (code, comments, docs, plans, commit messages, CHANGELOG).

## What it is

Web app that integrates with **Strava** and tracks changes to users' **KOMs** (segment course records). It periodically polls Strava, compares each athlete's current KOMs with the previous snapshot, detects **new / lost / improved / returned** KOMs, and notifies by e-mail. On top of that: dashboard, KOM list, map, ranking, clubs, and a head‑to‑head "Battle Field" (who took whose KOM).

Only app users are known to the system — Strava hid public segment leaderboards, so rivals outside the app are invisible.

## Tech stack

- **Backend API**: ASP.NET Core (**.NET 10**), Swagger.
- **Frontend**: **Blazor WebAssembly** + MudBlazor 9, Leaflet (maps).
- **DB**: **PostgreSQL 18** + EF Core (Npgsql), FlexLabs.Upsert, EFCore.BulkExtensions.
- **Auth**: **IdentityServer4** (OIDC / OAuth2 Authorization Code + PKCE), JWT Bearer. (IdentityServer4 is EOL — future migration candidate.)
- **Scheduling**: **Quartz.NET** (Europe/Warsaw).
- **CQRS/mediation**: **MediatR** (Commands / Queries / Notifications).
- **Mapping**: AutoMapper. **Results**: FluentResults. **Mail**: Brevo.
- **Logging**: Serilog (console + monthly rolling error file — see `CommonProgram`).
- **Deploy**: Docker Compose + nginx reverse proxy + certbot; Ansible for host provisioning.

## Solution layout (`src/KomTracker.sln`, Clean Architecture)

- `KomTracker.Domain` — entities (Athlete, Segment, SegmentEffort, KomsSummary, KomsSummarySegmentEffort, KomTakeover, Club, Token; **Bike** + `BikeType`/`BikeLifecycle` enums for BikeTracker); `BaseEntity` (AuditCD/AuditMD).
- `KomTracker.Application` — business logic: Commands/Queries/Services/Notifications (MediatR); interfaces for persistence & services.
- `KomTracker.Infrastructure` — EF Core (`KOMDBContext`, configurations, migrations, repositories), Identity (IdentityServer4), Strava services, Brevo mail.
- `KomTracker.API` — REST controllers + Quartz jobs + `Startup`.
- `KomTracker.WEB` — Blazor WASM frontend.
- `Strava/Strava.API.Client` — standalone Strava HTTP client.
- `Utils`, `*.Shared` — helpers, view models, shared constants (`CommonProgram` bootstraps host + Serilog).
- Test projects: `*.Application.Tests`, `*.Infrastructure.Tests`, `Strava.API.Client.Tests`, `Utils.Tests` (xUnit + FluentAssertions + NSubstitute + AutoFixture; `ITestLogger` via Xunit.DependencyInjection).

## Core flow — KOM tracking (`TrackKomsCommand`, hourly)

Per active athlete:
1. Get a valid token (auto-refresh; on failure the athlete is skipped/deactivated).
2. Fetch **all** current KOMs from Strava (`/athletes/{id}/koms`, paginated, private segments filtered out).
3. `SegmentService.CompareEfforts` — full outer join of current vs last KOM efforts by `SegmentId`:
   - present now, not before → **NewKom**; better time on a different effort → **ImprovedKom**; was there, gone → **LostKom**.
   - `CheckNewKomsAreReturnedAsync` reclassifies a "new" kom whose effort already existed in DB as **ReturnedKom** (e.g. a car ride was flagged and the KOM came back).
4. **Safeguard** (`ComparedEffortsModel.IsSuspiciousApiResponse`) — skip persisting suspicious Strava responses (empty/partial lists). `PreviousKomsCount` is set from the previous KOM count. Rules: empty (`koms==0 && lost>20`) or partial (`koms>0 && lost>50 && lost/previous>0.35`).
5. If there are changes, persist a `KomsSummary` + its `KomsSummarySegmentEffort` rows, then publish `TrackKomsCompletedNotification` → best‑effort handlers: send mail, refresh stats, detect KOM takeovers.

Resilience: the per-athlete loop isolates failures (try/catch + `ClearChangeTracker` per athlete; a `429` still intentionally stops the whole run). Notification handlers are best-effort (log + swallow). Missed side effects are recoverable via admin backfill/refresh.

## KOM takeovers (Battle Field) — `kom_takeover`

Detects, among app users, "who took whose KOM". `DetectKomTakeoversCommand` processes one `koms_summary` (triggered per athlete via the notification; full history via admin backfill `PUT /admin/detect-takeovers?from&to`, ascending by id).

- Two-sided pairing (`SegmentService.ResolveTakeovers`, pure/tested): a `NewKom` (taker) is matched to a `LostKom` (loser) on the same segment (same `sex`, `null==null`), searched only in earlier summaries within a **24h backward window** (by `TrackDate`). The pair is created by whichever side is processed later; idempotent via `UNIQUE(taken_segment_effort_id)`.
- `ReturnedKom` marks a prior takeover `reverted` (matched by `lost_segment_effort_id`) — the car-flag / deleted / privatized case.
- Table is lean: two effort ids + two summary ids + `TrackDate` (when the takeover happened, for time-based ranking) + `reverted` + audit. Athletes/segment are derived via the referenced efforts.

## BikeTracker — Strava activity sync (`strava` schema, Phase 1)

Auto-mileage source. **1a** extended `Strava.API.Client` with the activity (`ActivitySummaryModel`, incl. hand-added `utc_offset`) + gear endpoints (`IActivityApi.GetActivitiesAsync`, `IGearApi.GetGearAsync`, athlete `bikes[]`); athlete model corrected to `Meta→Summary→Detailed` (`GET /athlete` = Detailed). **1b** added the server pipeline:
- **`strava.activity`** — Strava activities synced **1:1** (all fields; key = Strava activity id; FK to `athlete`; `gear_id` for later bike attribution). Stores `start_date` (UTC) + `utc_offset` + `timezone` — **not** `start_date_local` (bogus `Z`).
- **`strava.athlete_sync`** — per-athlete opt-in gate (generic table; `activities_enabled` flag, room for more capabilities); the sync job processes only athletes with activities enabled. **`strava.activity_sync_history`** — one row per sync run (`RunAt`, `Duration`, `SyncFrom` null=full/else window start, status, counts, `ActivitiesCount` snapshot) for "last N syncs" on the UI.
- **`SyncActivitiesCommand { DateTime? After }`** (Application) — loops opted-in athletes (per-athlete isolation + 429-stops-the-run, like `TrackKomsCommand`); `EFActivityRepository.UpsertAthleteActivitiesAsync` bulk-upserts (EFCore.BulkExtensions, manual audit) and **window-scoped delete-detects** (`After==null` ⇒ full; else the recent window). Explicit `ActivitySummaryModel.ToEntity` mapping (no AutoMapper, D-P0-13). `IActivityService` wraps the client + translates errors.
- **Jobs**: `SyncActivitiesRecentJob` (`After=now-7d`, Mon–Sat 01:35) + `SyncActivitiesFullJob` (`After=null`, Sun 01:35), gated by `SyncActivitiesJobEnabled`. **Admin**: `PUT /admin/sync-activities?afterDays=`, `PUT /admin/athlete-sync?athleteId=&enabled=` (temporary opt-in until the 1c UI).
- **1b needs only `activity:read` (existing tokens)** — it lists all the athlete's activities except "Only You"; `activity:read_all` (private/Only-You completeness) comes with 1c's re-auth.
- Out of Phase 1b: opt-in UI + gear import (`bt.bike`/`bt.bike_link`) + scope escalation (1c); bike mileage display (1d); webhooks (Phase 6).

## BikeTracker (`bt` schema) — sibling product, Phase 0

A bike-maintenance journal being built inside the existing projects (no new solution/front-end). Full design: `docs/biketracker/CONCEPT.md` (Decisions D-1..D-19); Phase 0 spec: `.ai/plans/2026-08-09-biketracker-phase-0.md`.

- **Phase 0 (done):** a "Bikes" garage — `bt.bike` table (first use of a Postgres **schema** in this repo; `ToTable("bike","bt")` + `EnsureSchema`), DB-generated key. CRUD + lifecycle (Active/Archived/Sold) + hard delete.
- **Owner = platform User, not the Strava athlete.** `bt.bike.user_id` → `AspNetUsers.Id` (string FK). Everything is scoped to the signed-in user via the JWT `sub` (`GetCurrentUser().UserId`) — so ownership survives future integrations (Garmin, etc.). (Strava's athlete-centric linkage is the historical exception; new data hangs off the User.)
- **Enums stored as strings** (`HasConversion<string>()`) — readable in the DB, order-independent. Enums live in `Domain`; `KomTracker.API.Shared` references `Domain` so ViewModels are enum-typed (and WEB gets a typed `MudSelect<BikeType>`).
- **Dates** are `DateTime` UTC → `timestamptz` (consistent with the rest of the app); UI shows date only.
- **No AutoMapper** here — explicit `BikeEntity.ToViewModel()` extension (compile-time safe). CQRS: one `SaveBikeCommand` (nullable `Id` = create/update) + `ChangeLifecycle`/`Delete` (→ `Result`); queries return `BikeEntity`. Handlers hold only existence/authz guards (`NotFoundError`/`ForbiddenError`); input validation is in FluentValidation validators (forced to `en`; error keys camelCased to match the JSON).
- **Request body** `SaveBikeViewModel` (`API.Shared`) = the create/update body + WEB form (editable fields only). `SaveBikeCommand` carries the same fields flat (+ server-set `Id`/`UserId`); the controller maps body→command, and `BikeContractParityTests` fails if the two drift. `BikeType`/`BikeLifecycle` carry `[JsonConverter(typeof(JsonStringEnumConverter))]` so they're strings on the wire (matching the DB), targeted to just these enums.
- **WEB:** `Pages/Bikes.razor` (card + table toggle), `Pages/BikeDetails.razor` (all fields + "Components" placeholder), `Shared/AddEditBikeDialog.razor` (progressive form), `Shared/SellBikeDialog.razor`; grouped under a "Bike Tracker" `MudNavGroup`.
- **Out of Phase 0:** components, installations, mileage attribution, Strava `gear_id` sync, alerts, cost analysis (later phases).

## Validation & error mapping (app-wide, introduced with BikeTracker)

- **FluentValidation** + a MediatR `ValidationBehavior<TRequest,TResponse> where TResponse : ResultBase`: runs validators before the handler and, on failure, returns a **failed `Result` carrying a `ValidationError`** (no exceptions). The `ResultBase` constraint means only Result-returning commands are validated; queries pass through. Registered via `AddOpenBehavior` + `AddValidatorsFromAssembly` in `Application/DependencyInjection`.
- **Semantic errors** (`Application/Errors/`): `AppError` base + `ValidationError`/`NotFoundError`/`ForbiddenError`/`ConflictError`. The API's `ResultExtensions.ToActionResult` switches on the first error's type → **422 / 404 / 403 / 409**, else **400**; bodies are built by the framework `ProblemDetailsFactory` (RFC 7807 `application/problem+json`; content-type is auto for ProblemDetails-derived values, 422 registered in `ClientErrorMapping` for its `type`). Existing `XxxError : FluentResults.Error` types are unchanged (→ 400).
- **Where validation runs:** all command validation is in the **MediatR `ValidationBehavior`** (guaranteed from any caller). Commands are **flat** (`SaveBikeCommand` mirrors the `SaveBikeViewModel` request body; a reflection parity test guards drift), so error keys are the flat field names camelCased (`name`, `saleDate`) matching the posted JSON. (The command can't double as the request body: WEB is a separate WASM assembly that doesn't reference `Application`.) `ValidationError.From` builds the dict; FluentValidation messages forced to `en`.

## Background jobs (Quartz, Europe/Warsaw)

- `TrackKomsJob` — hourly `:00` → `TrackKomsCommand`.
- `RefreshSegmentsJob` — `:30` → refresh segment details (batch).
- `RefreshClubsJob` — `00:45` & `12:45` → sync club membership.
- `RefreshStatsJob` — `23:55` → recompute athlete stats JSON (ranking source).
All toggled by `ApplicationConfiguration.*JobEnabled`; also triggerable via `AdminController`.

## API controllers

`Athletes` (koms, koms-changes, summaries, clubs, change-email), `Ranking`, `Stats` (koms-changes), `KomTakeovers` (pairs, efforts), `Admin` (job triggers, takeover backfill), `Playground` (dev). All `[BearerAuthorize]`; Admin endpoints require the `admin` role.

## Frontend pages

Dashboard, Koms, Map, Ranking, Koms changes, Battle Field, Account, FAQ, Login. Pattern: MudBlazor, `MudSelect` filter combos (activity type from `ActivityTypeHelper`, clubs from `athletes/{id}/clubs`), `MudTable` with client-side filtering/paging, athlete/segment rendered as Strava links, `IDialogService` for modals. WEB reads its own `wwwroot/appsettings.json` (`KomTrackerApiUrl`, `IdentityConfiguration`, `StartYear`).

## Conventions & gotchas

- **Repo = English; chat = Polish.** Plans live in `.ai/plans/` (dated, checklist format with a Decisions section). This file is the canonical overview.
- Shared **`DbContext`/`IKOMUnitOfWork` scope per Quartz run** — mind cross-athlete state (`ClearChangeTracker`).
- `EFAthleteRepository` writes (athlete/token/stats) use immediate FlexLabs `Upsert().RunAsync()` (not tracked).
- Audit columns auto-stamped in `EFKOMUnitOfWork.SaveChangesAsync`.
- `GetLastKomsChangesAsync` orders by `koms_summary_id` (PK index), not the unindexed `audit_cd` — do not "fix" back.
- `CHANGELOG.md` uses a top `## UPCOMMING` section; releases are dated.
- EF migrations: `dotnet ef migrations add <Name> --project src/KomTracker/KomTracker.Infrastructure --startup-project src/KomTracker/KomTracker.API`.

## Notes / docs

- `NOTES.md` — design notes & history (safeguard rationale, extended climb categories, battle-field detection rules, API-glitch fixes).
- `DEV.md` — local dependencies (`stuff/docker-dependiencies`).
- `.ai/plans/` — feature specs/checklists.

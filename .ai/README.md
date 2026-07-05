# KOMTracker — project overview (for AI assistants)

> Living overview kept up to date as the project evolves. Conversation with the maintainer is in Polish; **everything committed to the repo is in English** (code, comments, docs, plans, commit messages, CHANGELOG).

## What it is

Web app that integrates with **Strava** and tracks changes to users' **KOMs** (segment course records). It periodically polls Strava, compares each athlete's current KOMs with the previous snapshot, detects **new / lost / improved / returned** KOMs, and notifies by e-mail. On top of that: dashboard, KOM list, map, ranking, clubs, and a head‑to‑head "Battle Field" (who took whose KOM).

Only app users are known to the system — Strava hid public segment leaderboards, so rivals outside the app are invisible.

## Tech stack

- **Backend API**: ASP.NET Core (**.NET 10**), Swagger.
- **Frontend**: **Blazor WebAssembly** + MudBlazor 8, Plotly, Leaflet (maps).
- **DB**: **PostgreSQL 18** + EF Core (Npgsql), FlexLabs.Upsert, EFCore.BulkExtensions.
- **Auth**: **IdentityServer4** (OIDC / OAuth2 Authorization Code + PKCE), JWT Bearer. (IdentityServer4 is EOL — future migration candidate.)
- **Scheduling**: **Quartz.NET** (Europe/Warsaw).
- **CQRS/mediation**: **MediatR** (Commands / Queries / Notifications).
- **Mapping**: AutoMapper. **Results**: FluentResults. **Mail**: Brevo.
- **Logging**: Serilog (console + monthly rolling error file — see `CommonProgram`).
- **Deploy**: Docker Compose + nginx reverse proxy + certbot; Ansible for host provisioning.

## Solution layout (`src/KomTracker.sln`, Clean Architecture)

- `KomTracker.Domain` — entities (Athlete, Segment, SegmentEffort, KomsSummary, KomsSummarySegmentEffort, KomTakeover, Club, Token); `BaseEntity` (AuditCD/AuditMD).
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

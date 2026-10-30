# System → Stats (admin dashboard)

## Context
The maintainer wants an admin-only **System → Stats** page to walk in and see what's happening in the app: global counts, who joined / was active recently, what's being added in Bike Tracker and who uses it, and whether the background machinery (syncs, webhooks, jobs) is healthy.
- The **`admin`** role already exists (`Application/Constants.cs` `Roles.Admin`) and gates `AdminController` (`admin/*` PUTs that trigger jobs). Roles reach the access token (ClaimsFactory + ApiScope) but not the ID token, so the WEB couldn't tell an admin.
- Every `BaseEntity` has `audit_cd` (created) / `audit_md`; the identity `user` table has neither.
- Jobs run on an in-memory Quartz scheduler (no run history); persisted traces: `strava.activity_sync_history`, `strava.webhook_event`, and the refresh timestamps of segments / athletes / stats / clubs / KOM summaries.
- `StatsController` / `Queries/Stats` already mean KOM stats → everything here is named **SystemStats**.

## Decisions
- **D-SYS-1 One admin endpoint, one snapshot.** `GET admin/system-stats` on `AdminController` returns the whole page's data, built by sequential EF queries (counts / max / small top-N lists). *Why: an admin page opened now and then — a few dozen `count(*)` / `max()` queries are cheap; one round-trip, nothing cached or precomputed that could go stale.*
- **D-SYS-2 The role reaches the WEB through the ID token.** `role` is added to the `profile` identity resource's user claims (the client already has `AlwaysIncludeUserClaimsInIdToken`), and the WEB sets `UserOptions.RoleClaim = "role"`. The menu group sits in `<AuthorizeView Roles="admin">`, the page has `[Authorize(Roles = "admin")]`; the API attribute stays the real gate. *Why: the standard OIDC way; hiding the menu is only UX, security is the API. Existing sessions need one re-login to see the menu.*
- **D-SYS-3 "Joined" = `athlete.audit_cd`, "active" = `user.last_seen_at`.** *Why: `user` has no created-at; the athlete row is created on the first Strava connect; `last_seen_at` (2.1.1) is the only activity signal that moves without a Strava login.*
- **D-SYS-4 Health from persisted traces only** — webhook queue, activity sync history, freshness of refreshed tables; no Quartz job store. *Why: no schema / infrastructure change, and these traces answer "is it working" well enough.*
- **D-SYS-5 The snapshot is a Domain read model and the wire format** (`SystemStatsReport`, like `CostAnalysisReport` / D-COST-9). *Why: a read-only report the server builds and the page shows as is — a 1:1 view-model copy of ~10 record types would add mapping without protecting anything.*
- **D-SYS-6 No KOM leaderboards** (maintainer). *Why: rankings already exist as their own pages; this page is about the app, not the riders' results.*

## Page (`/system/stats`, menu **System → Stats**)
1. **Headline tiles** — total + "+N in 30 days" where a created-at exists: users (+ active 24 h / 7 d / 30 d), segments, KOMs held now (Σ of each athlete's latest `koms_summary.koms`), KOM changes in 30 days (new / improved / lost / returned), takeovers between users in 30 days, clubs, activities (+ km), Strava bikes; Bike Tracker: bikes, components, installations, warehouses, services, expenses.
2. **People** — the 10 newest users (joined, last seen) and the 10 most recently active (last seen, last Strava login).
3. **Bike Tracker adoption** — per user with any BT data: bikes · components · installations · services · expenses · warehouses · last change; "N of M users use Bike Tracker".
4. **Last 30 days in Bike Tracker** — counts per type + a feed of the newest ~15 items (type, name, user, date).
5. **System health** — webhook queue (received 24 h / 7 d, unprocessed now, oldest unprocessed), activity syncs in 7 days by status + the last job run, athletes whose latest job sync was `NoValidToken`, freshness of KOM tracking / segments / athletes / stats / clubs (amber when older than expected).
6. **Growth chart** — users joined per month, cumulative (last 24 months).

## Checklist
- [x] Spec persisted.
- [x] **Domain**: `SystemStatsReport` read model.
- [x] **Application**: `ISystemStatsRepository`, `GetSystemStatsQuery` + handler.
- [x] **Infrastructure**: `EFSystemStatsRepository` (+ DI); `role` in the profile identity resource.
- [x] **API**: `AdminController` `GET system-stats`.
- [x] **WEB**: `RoleClaim`, System → Stats menu (admin only), `Pages/System/SystemStats`.
- [x] **Tests**: query handler.
- [x] Verified on local data + browser preview (desktop / mobile, menu hidden without the role).
- [x] **Docs**: `.ai/README.md`, `CHANGELOG.md`.

## Verification
- `dotnet build`, `dotnet test` green.
- The repository run against the local DB from a scratch console; a few numbers cross-checked with SQL.
- Browser preview with a fake admin user; menu hidden for a non-admin; non-admins get 403 from `admin/system-stats`.

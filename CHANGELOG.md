## UPCOMMING

### Features
- BikeTracker (Phase 2b-i — installations UX polish): installations now render as **one table** on both the bike and component detail pages (the current one highlighted green + a **Current** chip, historical rows below) — the bike page now also shows **historical** installs, so you can see everything that's passed through a frame. You can **edit** any installation record directly (bike / position / dates / Manual totals — fixing a wrong date no longer means remove-delete-re-add); install dates now include a **time** (defaults to midnight — set it to right-after-a-ride). Install/Add-manual buttons moved into the Installations panel. The **components list** gained *install-state* (installed / not installed) and *bike* filters, plus an **Install on bike** action straight from the row menu (no need to open the detail). On the component detail, the **Location** field now links to the bike, and Manual (dateless) entries sort to the **end** of the installations table.
- BikeTracker (Phase 2b-i — installations): you can now **install a component on a bike**. From a bike's detail page there's an **Installed components** panel (name → component, position, "since" date, with per-item **Move** / **Remove** actions) and an **Install component** button; from a component's detail page there's an **Install on bike** button, a **current-installation** banner, and an **installation history** table. Two install types: **Tracked** (dated window — `DateFrom`, open `DateTo` = currently installed; feeds ride-mileage attribution in a later phase) and **Manual** (dateless historical entry with static distance/time/elevation totals you type in, never computed). A component can have at most **one active Tracked installation** — re-installing it elsewhere is blocked (409) until you Move or Remove it. **Move** atomically closes the current window and opens a new one on the move date; **Remove** closes it (the component becomes unassigned); a historical/Manual record can be hard-**deleted** for corrections. Installing clears the component's warehouse, and its location now reads **on a bike ▸ warehouse ▸ unassigned** (shown as a chip on the components list and a "Location" field on the detail). Guards: deleting a **component with installation history** is blocked (archive instead); deleting a **bike** first removes its installations, freeing the components. New `bt.installation` table; everything per-user. Component-in-component, one component on several bikes at once, and computed mileage come later.
- BikeTracker (Phase 2a — components + warehouses): a new **Components** inventory (add / edit / list as cards or table / detail page / lifecycle Active→Archived→Sold / delete), each with a category picked from a **grouped** dropdown (headers per group, "Select component type" placeholder — no accidental default) with hand-drawn per-category **icons** and friendly names ("Bar Tape", not "BarTape"), optional brand/model/weight/notes/purchase info and initial seed metrics. The list has **filters** (by category group and by warehouse) on top of search. Components live in **Warehouses** — a new page (card or table view + search) to manage where non-installed parts sit (Home, Garage, a drawer…); deleting a warehouse keeps its components and clears their location. Everything is per-user. Installing components on bikes and computing their ride mileage come in later phases.
- BikeTracker (UX): selling or archiving a bike **or** a component now opens a dialog with an editable **Notes** field, so you can jot down why (e.g. "chain worn out", "sold with the old wheelset").
- BikeTracker (Phase 1e — single-activity refresh): each row on the **Strava activities** page now has a **Refresh** action that re-pulls that one activity from Strava (`GET /activities/{id}`) and updates it in place — handy right after you re-assign its bike or rename the ride on Strava, without waiting for the nightly sync. The refresh is a targeted upsert (no delete-detection, so other rows are untouched) and won't refresh someone else's activity. Under the hood the Strava client now models the full **DetailedActivity** payload, and the reusable `SyncActivityCommand` (keyed by athlete + activity id) is the same primitive a future webhook handler will use.
- BikeTracker (Phase 1e — Strava activities page + split sync): a new **Bike Tracker → Strava activities** page — a server-paged table (date → Strava, name/type, distance, moving time, avg speed, elevation, and the attributed **bike**), with a "Last updated" header that opens a **sync-history** dialog. Sync is now **two independent switches on Account → Strava**: *automatic activity sync* and *automatic bike sync*. Turning on activity sync the first time kicks a one-time **background full backfill** ("may take a while"); toggling it off/on again won't re-hammer Strava. A new daily **bike sync job (02:35)** keeps gear fresh for opted-in athletes; the Strava-bikes page keeps a manual "Sync bikes" button (independent of the auto flag) and shows a banner when auto sync is off.
- BikeTracker (Phase 1d — bike mileage): each bike now shows its **totals = initial seed + Σ of its Strava rides** (distance / moving time / elevation), matched via `gear_id ↔ bt.bike_link`. Computed on-the-fly from the synced activities (always correct after edits/re-syncs — no stored counters). Distance appears in the garage (cards + table); the bike detail page has a **Mileage** panel (distance/time/elevation + "incl. N Strava rides"). All attributed rides count for now (trainer/virtual included).
- BikeTracker (retired/historical bikes): Strava bike sync now also imports bikes that Strava omits from `GET /athlete` (retired gear) by hydrating the distinct bike `gear_id`s seen in your synced activities — so a retired bike used in older rides shows up after those rides have synced. The Strava-bikes page hints at this when "Show retired" is on.
- BikeTracker (UX polish): the garage shows a friendly empty-state placeholder (Add-bike CTA, "Show archived/sold" when filtered) instead of a bare alert; the **Sync from Strava** button now shows a spinner while syncing; and the Strava-bikes first-run placeholder no longer duplicates the header sync button (single clear CTA).
- BikeTracker (Phase 1c-ii — Strava private-rides access): read-only access to **private / "Only You" rides** (Strava `activity:read_all`) for accurate mileage — asked for, never forced. New Strava connections now request it up front (declining just falls back to public rides). Existing users can enable it on **Account → Strava**, which shows the granted scopes + activity-access level and an **Allow private rides** button (and a **Revoke private rides access** button to drop back to public-only); the **Strava bikes** page flags when it's off (mileage may be under-counted) and links there. The button runs a standalone Strava re-authorization (`approval_prompt=force`) via new `/account/upgrade` + `/account/connect-upgrade` identity endpoints that just re-store the athlete's token with the wider scope — no re-login. Login's required-scope check accepts `activity:read_all` in place of `activity:read` so these users can still sign in. In-UI reassurance: your activities are never shown to other users.
- BikeTracker (Phase 1c-i — Strava bikes + opt-in): a new **Bike Tracker → Strava bikes** page. One **Sync from Strava** click mirrors your Strava gear into `strava.bike` (1:1, incl. retired), turns on activity sync (`strava.athlete_sync.bikes_enabled` / `activities_enabled`) and backfills your activities. From the list you can **Create** a garage bike from a Strava one (dialog pre-filled: brand/model, frame_type→type, weight — mileage is left to accrue from activities, not seeded) or **Link** a Strava bike to an existing one — the coupling lives in a new `bt.bike_link` table. Linked state is shown on both sides (a "Strava" chip in the garage + on the bike detail; the Strava-bikes list badges the linked bike and deep-links to it) and can be **removed** (Unlink) from either side. Scope-escalation re-auth (for private/"Only You" rides) and mileage display come later.
- BikeTracker (Phase 1b — activity sync engine): new `strava` schema (`strava.activity` synced 1:1 from Strava, `strava.athlete_sync` per-athlete opt-in gate, `strava.activity_sync_history` per-run history); a two-tier `SyncActivitiesCommand` (full weekly / recent-window daily) that pulls each opted-in athlete's activities and bulk-upserts them, with window-scoped delete-detection; jobs (`SyncActivitiesFull/RecentJob`) + admin triggers (`/admin/sync-activities`, `/admin/athlete-sync`). (Clubs job moved to `:35`.) No UI/gear import/scope-escalation yet.
- BikeTracker (Phase 1a — Strava client foundation): extend the Strava API client with a full activity model (`ActivitySummaryModel`, incl. the hand-added `utc_offset` missing from Strava's schema), a paginated `IActivityApi.GetActivitiesAsync` (list athlete activities, `after`/`before` window, 429/rate-limit handling), and gear support (`GearSummaryModel`/`GearDetailedModel`, `IGearApi.GetGearAsync`, `bikes[]`/`shoes[]` on the athlete). No app wiring yet — foundation for the activity sync.
- BikeTracker (Phase 0): a "Bikes" garage — add / edit / list (card or table view) / detail page / lifecycle (Active → Archived / Sold) / delete, grouped under a new "Bike Tracker" nav section. Bikes persist in a new `bt` Postgres schema (`bt.bike`) with a strong FK to the athlete; every operation is scoped to the signed-in athlete.
- Introduce app-wide input validation: FluentValidation + a MediatR `ValidationBehavior` that fails with a `Result` (no exceptions), plus semantic error types (validation/not-found/forbidden/conflict) mapped to HTTP 422/404/403/409 as `application/problem+json`.

## 1.16.0 (2026-08-06)

### Features
- Update MudBlazor 8 → 9 (major upgrade). App code only needed two fixes: the app-bar avatar `MudMenu` activator (v9 no longer auto-opens — wired `MenuContext.ToggleAsync`) and a `MudTabs` param rename (`PanelClass` → `TabPanelsClass`) on the Account page. Build + all tests green.

## 1.15.0 (2026-07-29)

### Features
- New landing page: a proper scroll-through story of what KOM Tracker does (the notification blind spot, email reports, rankings, Battle Field, The Bar/The Burn & extended metrics, and the location/direction "hunt" combo) with punchy copy and light scroll animations — replaces the old 4-bullet login panel

## 1.14.0 (2026-07-25)

### Features
- KOM direction: show each segment's start→end compass bearing (arrow + degrees) on the koms list, koms-changes and the KOM-list modals; sortable by angle, and filterable by compass direction (koms list, koms-changes, KOM-list modals)
- Location filter: narrow koms to those starting near a point — pick it on a map and set a radius (a live circle shows the covered area); available on the koms list, koms-changes and the KOM-list modals
- Koms changes: filter by activity type
- Koms list/modals/changes: segment name links to the (fast) segment page again; the effort time now links to the effort on Strava (reverts the slow name→effort link from 1.13.0)
- Map: show a dot at each segment's start (hover for its name); clicking a segment highlights its start (green) and finish (dark) endpoints

## 1.13.0 (2026-07-17)

### Features
- KOM difficulty & effort ratings — "The Bar" (how hard a KOM is to take, estimated from the winning time + terrain) and "The Burn" (how hard the holder actually worked, from measured power), rated on Coggan's Cat 5 → World Class scale; shown on the koms list, the koms-changes list and the Battle Field details, with an FAQ explainer
- Ranking: click a count to see the KOMs behind it — a category (or total) count in Total, or a New/Lost count in Koms changes (respects the selected activity type)
- Koms list: segment names now link to the actual effort on Strava (not just the segment)

### Bug fixes
- Recover gracefully when the session token can't be renewed (e.g. after an API restart): silently redirect to re-login instead of crashing the page
- Fix breadcrumbs: update on the first navigation (no longer stale until a second menu click) and keep their links within the app base path

### Performance
- Serve the web app over HTTP/2 (request multiplexing) and compress static assets with Brotli/gzip, cutting first-load transfer significantly
- Brotli/gzip-compress API responses at the reverse proxy
- Remove unused Plotly library (~9.6 MB off every page load)
- Upgrade reverse-proxy nginx 1.21 → 1.31 and add the ngx_brotli module (security/currency + better compression)

## 1.12.0 (2026-07-11)

### Features
- Battle field: search and sort in the takeover details modal
- Battle field: FAQ explaining how it works

### Bug fixes
- Fix stale web app after deploy: content-hash all static assets (framework fingerprinted by the SDK; CSS/library assets hashed via a build-time script) and cache them immutably; only index.html revalidates
- Fix broken Plotly script references in index.html (point to the actual Plotly.Blazor 7.1.0 asset filenames)

## 1.11.0 (2026-07-07)

### Features
- Battle field: detect KOM takeovers between app users (backend)
- Battle field: page with head-to-head KOM takeover ranking
- Refresh athletes profile data daily (fixes stale names/avatars for inactive users)
- Log errors to a monthly rolling file

### Bug fixes
- Make KOM tracking resilient: isolate per-athlete failures so one error no longer aborts the whole run

## 1.10.0 (2026-07-01)

### Features
- Update to .NET 10.0
- Update MudBlazor to v8
- Update dependiencies
- Update postgres to 18

### Bug fixes
- Add safeguard to skip KOM tracking updates when Strava API returns empty/partial data

### Performance
- Speed up last KOMs changes query by ordering on koms_summary_id (PK index) instead of unindexed audit_cd

## 1.9.0 (2025-04-10)

### Features
- Update to .NET 9.0
- Update dependiencies

## 1.8.0 (2024-12-10)

### Features
- Interrupt jobs on 429 Too Many Requests (Rate Limit Exceeded)
- Update dependiencies
- Migrate from SendinBlue to Brevo (only rebrading).

## 1.7.0 (2024-01-03)

### Features
- Update to .NET 8.0
- Update dependiencies

## 1.6.0 (2023-10-11)

### Features
- Update to .NET 7.0
- Update dependiencies

## Bug fixes
- Add infinite recursion protection for `TrackKomsForAthleteAsync` (e.g. rejected token by user)

## 1.5.1 (2022-11-28)

### Features
- Ranking by activity type

## 1.5.0 (2022-11-25)

### Features
- Ranking

### Bug fixes
- Fix job for refreshing clubs data

## 1.4.0 (2022-11-13)

### Features
- Athlete clubs
- Filter last koms changes by club

## 1.3.0 (2022-11-08)

### Features
- Last koms changes

## 1.2.0 (2022-11-06)

### Features
- Extended categories
- Faq

## 1.1.0 (2022-06-10)

### Features
- KOMs map
- Returned koms (e.g. flag someone that stole  kom in vehicle)
- Bug fixing

## 1.0.0 (2022-04-13)

First deployment as MVP 😍

### Features
- KOMs tracking
- Mail notification about changes in koms
-- all lost (downhills, less popular etc.) 
-- new (also new for old activities e.g. someone has been crated a new segment)
-- improved
- KOMs list with filters
- Simple dashboard
- Simple profile
- Dark mode
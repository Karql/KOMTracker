## UPCOMMING

### Features
- **System → Stats** (admins only) — one page to see what's happening in the app: users, segments, KOMs, activities, clubs and Bike Tracker totals with what's new in the last 30 days, newest and recently active users, who uses Bike Tracker and what was just added, plus the health of the Strava syncs, webhooks and scheduled refreshes.

## 2.1.1 (2026-10-20)

### Features
- The app now also remembers when each user was **last active** (`last_seen_at`, updated at most hourly) — unlike the last sign-in, it moves even when a remembered session skips the Strava login for weeks.

## 2.1.0 (2026-10-20)

### Features
- **Bike Tracker → Costs** — what cycling really costs you: net cost, cost per km and per year, a drill-down donut and year-by-year bars (source → group → category → item), totals per bike, parts' cost per 1000 km and a full list of every cost. Filter by period, bike and source, net or gross of sales. Parts used on several bikes are split by the km they did on each. Also on the landing page.

## 2.0.1 (2026-10-20)

### Features
- The app now remembers when each user last signed in (`last_login_at` on the user), so it's easy to see who's been around lately.

## 2.0.0 (2026-10-19)

### Features
- **Bike Tracker** — a brand-new part of KOM Tracker: a garage and maintenance log for your bikes, fed by your Strava rides. Find it under **Bike Tracker** in the menu.
  - **Garage** — your bikes with their details — type, brand/model, weight, purchase and sale info, and Active / Archived / Sold status. Each bike shows its **mileage** — distance, moving time and elevation — counted from its Strava rides on top of any starting kilometres you enter.
  - **Strava sync** — one click brings your Strava bikes over (retired ones too) and links them to the garage. New and edited rides arrive within moments and land on the right bike; a **Strava activities** page lists them with a per-ride refresh. Private ("Only You") rides are counted only if you allow it on **Account → Strava**; your rides are never shown to anyone else.
  - **Components** — chains, cassettes, tyres, wheels, pedals… in 60+ categories with their own icons. Install a part on a bike, move it to another one, put a tyre **inside a wheel**, run a computer on several bikes at once — every component knows **how far it has really gone**, following it through the whole tyre → wheel → bike chain. Older parts can get a hand-entered history; parts on the shelf live in **Warehouses**.
  - **Services** — log a workshop visit or a DIY job once, even when it covered several things (fork + wheels + the whole bike), with date, place, cost and notes. Every bike/component page lists its services together with **its mileage on the day of each service**.
  - **Expenses** — everything else you buy for cycling (clothing, shoes, helmets, tubes, lube, tools, electronics, nutrition…) in its own list with categories, filters by year and category, and archive / sell like the rest.
  - **Handy everywhere** — card or table view (remembered), search and filters, suggestions for brands, models and shops you've used before, sale price and buyer for anything you sell, your **preferred currency** (Account → Profile details), dates you can type as `yyyy-MM-dd`, and both `,` and `.` accepted in numbers.
- **Landing page** — a new "New in 2.0" section introduces Bike Tracker.
- Under the hood: app-wide input validation with clear error messages, and Strava webhooks so ride changes are picked up near-instantly.

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

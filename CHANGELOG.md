## UPCOMMING

### Features
- KOM difficulty & effort ratings on the koms list — "The Bar" (how hard a KOM is to take, estimated from the winning time + terrain) and "The Burn" (how hard the holder actually worked, from measured power), rated on Coggan's Cat 5 → World Class scale (0–100); with an FAQ explainer

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
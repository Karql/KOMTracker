# Strava API — real behaviour notes (v3)

> **Source of truth over the OpenAPI spec** (`specs/openapi/strava-v3.yaml`) and the online docs — both diverge from what the API actually returns. Captured **2026-08-06** with our **approved (raised-limit)** app (shared `client_id` with KOMTracker). Use this when building the Phase-1 activity sync / Activity model.

## Rate limits (from response headers)
Strava exposes **two independent buckets**, each formatted `<15-minute>,<daily>`:

| bucket | header (limit) | header (usage) | observed |
|---|---|---|---|
| Overall | `x-ratelimit-limit` | `x-ratelimit-usage` | `3300,165000` — used `7,5402` |
| **Read-only** | `x-readratelimit-limit` | `x-readratelimit-usage` | `600,30000` — used `7,5402` |

- These are our **raised** limits (approved). Every read call counts toward **both** buckets, so for our read-heavy sync the **read bucket is the binding constraint**: **600 / 15 min** and **30 000 / day**.
- **Shared across the whole `client_id`** → KOMTracker + BikeTracker draw from the same buckets. Budget sync volume accordingly.
- `429 Too Many Requests` when exceeded → the client surfaces `TooManyRequests`; jobs should back off / stop for the window.

## Pagination — `GET /athlete/activities`
`GET /athlete/activities?page=&per_page=&before=&after=`
- `per_page`: docs say default **30**, but **max 200 works** (we already use 200 for KOMs). Use **200** to minimise calls (rate-limit friendly).
- **No total-count and no `Link` headers** — you can't know how many pages exist. **Iterate `page=1,2,…` until a page returns fewer than `per_page` items (or empty).**
- **`after` / `before`** (epoch seconds) bound the window. We use a **two-tier, cursor-less** strategy: a **recent-window** sync frequently (`?after=now-7d`, e.g. daily) for freshness, plus a **full** sync on a slow cadence (no `after`, e.g. weekly) to catch **edits/deletes of old activities** and any missed webhooks. Windows are derived from `now` → **no per-athlete cursor to persist**.
- Batch fetch returns **`SummaryActivity`** objects (fields below).

## Sample — one activity from `List Athlete Activities` (real, 2026-08-06; polyline truncated)
```json
{
  "resource_state": 2,
  "athlete": { "id": 2394302, "id_str": "2394302", "resource_state": 1 },
  "name": "Afternoon Ride",
  "distance": 3830.6,
  "moving_time": 629,
  "elapsed_time": 675,
  "total_elevation_gain": 11.0,
  "type": "Ride",
  "sport_type": "Ride",
  "workout_type": 10,
  "device_name": "Garmin fēnix 7x",
  "id": 19627877554,
  "id_str": "19627877554",
  "start_date": "2026-08-06T13:44:38Z",
  "start_date_local": "2026-08-06T15:44:38Z",
  "timezone": "(GMT+01:00) Europe/Warsaw",
  "utc_offset": 7200.0,
  "location_city": null, "location_state": null, "location_country": null,
  "achievement_count": 0, "kudos_count": 10, "comment_count": 0, "athlete_count": 1,
  "photo_count": 0,
  "map": { "id": "a19627877554", "summary_polyline": "cstpHsj`yBK]IyA?w@AgBKyAC_B?a@@Q…(truncated)…", "resource_state": 2 },
  "trainer": false,
  "commute": true,
  "manual": false,
  "private": false,
  "visibility": "everyone",
  "flagged": false,
  "gear_id": "b805524",
  "start_latlng": [50.08, 19.99],
  "end_latlng": [50.06, 20.02],
  "average_speed": 6.09,
  "max_speed": 10.34,
  "average_temp": 35,
  "average_watts": 100.4,
  "device_watts": false,
  "kilojoules": 63.1,
  "has_heartrate": true,
  "average_heartrate": 97.2,
  "max_heartrate": 127.0,
  "heartrate_opt_out": false,
  "display_hide_heartrate_option": true,
  "elev_high": 218.6,
  "elev_low": 196.8,
  "upload_id": 20755154279,
  "upload_id_str": "20755154279",
  "external_id": "garmin_ping_609506287894",
  "from_accepted_tag": false,
  "pr_count": 0,
  "total_photo_count": 0,
  "has_kudoed": false,
  "suffer_score": 1.0
}
```

## Field notes / gotchas (vs the OpenAPI model)
- **The real payload is a superset of the spec's `SummaryActivity`.** Fields present here but **missing from the spec model**: `utc_offset`, `visibility`, `average_temp`, `has_heartrate`, `heartrate_opt_out`, `display_hide_heartrate_option`, `suffer_score`, `from_accepted_tag`, `pr_count`, `location_city/state/country`, `id_str`/`upload_id_str`, `device_name`, `workout_type`. → **Don't codegen strictly from the spec**; map defensively (nullable) and keep what we need.
- `id` is a **long** but Strava also sends **`id_str`** (string) — big 64-bit ids lose precision in JS, hence the string twin. In C# use `long`; if any id ever round-trips through JS, prefer `id_str`. Same for `upload_id`/`upload_id_str`.
- `gear_id`: **string** (`"b805524"`), nullable / `"none"` when unset → links to `bt.bike_link.ExternalId`; no gear ⇒ unattributed.
- **Dates (D-15):** `start_date` = UTC ✅; `start_date_local` carries a **bogus `Z`** (it's local wall-clock, not UTC); `utc_offset` = `7200` (+2 h, DST-correct) → local = `start_date + utc_offset`; `timezone` = `"(GMT+01:00) Europe/Warsaw"` (offset label is *standard*, not the DST offset).
- **`map.summary_polyline` is already in the LIST response** → we get the route without a per-activity detail call.
- `sport_type` (preferred) vs `type` (deprecated); here both `"Ride"`, plus `workout_type` (int) and `commute: true`.
- `athlete` in the list is just `{ id, resource_state }` (MetaAthlete) — no name/avatar in this endpoint.
- `average_watts` present with `device_watts: false` = **estimated** power (no power meter); `kilojoules` present.

## Implications for the BikeTracker sync (Phase 1)
- Two-tier poll of `GET /athlete/activities?...&per_page=200`, loop pages until a short/empty page: **recent-window** (`after=now-7d`) frequently + **full** (no `after`) on a slow cadence — the full pass is what catches edited/deleted old rides. **Upsert into `strava.activity` 1:1** (all fields), **no gear/sport filter** at sync (D-10); filter only at attribution. Only athletes with `strava.athlete_sync.Enabled` are synced.
- Mind the **shared** rate buckets; the **read** bucket (600/15 min, 30 000/day) is the tighter one for our reads.

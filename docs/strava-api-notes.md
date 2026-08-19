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

## Get Activity — `GET /activities/{id}` → **DetailedActivity**
The single-activity endpoint returns a **superset of the `SummaryActivity`** from the list. On top of the summary fields it adds: `description`, `calories`, `perceived_exertion` / `prefer_perceived_exertion`, `device_name`, `embed_token`, `hide_from_home`, `leaderboard_opt_out` / `segment_leaderboard_opt_out`, `available_zones[]`, an embedded `gear` (SummaryGear), and the nested collections `segment_efforts[]`, `best_efforts[]`, `splits_metric[]`, `splits_standard[]`, `laps[]`, `photos`, `similar_activities`, `stats_visibility[]`.

- **Our client models the FULL payload** (`ActivityDetailedModel : ActivitySummaryModel`, reusing `SegmentEffortDetailedModel`/`GearSummaryModel`) — the Strava client is a universal connector, so it mirrors the API even where BikeTracker doesn't consume a field yet.
- **BikeTracker persists only the summary fields** (`ActivityDetailedModel` IS-A `ActivitySummaryModel`, so the existing summary→`ActivityEntity` mapping applies) — the single-activity refresh writes the same columns as the list sync, **no new columns**.
- Same date gotcha as the list (D-15): `start_date` is UTC, `start_date_local` carries a bogus `Z`; the nested `laps[]`/`segment_efforts[]` repeat this.

```jsonc
// GET /activities/{id} (real, polyline / long arrays truncated)
{
  "resource_state": 3,
  "athlete": { "id": 2394302, "resource_state": 1 },
  "name": "Afternoon Ride", "distance": 3830.6, "moving_time": 629, "elapsed_time": 675,
  "type": "Ride", "sport_type": "Ride", "id": 19598505831,
  "start_date": "2026-08-06T13:44:38Z", "start_date_local": "2026-08-06T15:44:38Z",
  "timezone": "(GMT+01:00) Europe/Warsaw", "utc_offset": 7200.0,
  "gear_id": "b10707658", "average_speed": 6.09, "max_speed": 10.34,
  "description": "nice one", "calories": 420.5, "perceived_exertion": 5,
  "device_name": "Garmin fēnix 7x", "embed_token": "…",
  "available_zones": ["heartrate","power"],
  "gear": { "id": "b10707658", "primary": false, "name": "Sensa", "nickname": "Sensa",
            "resource_state": 2, "retired": false, "distance": 29143765, "converted_distance": 29143.8 },
  "segment_efforts": [ { "id": 111, "name": "…", "elapsed_time": 60, "moving_time": 60,
                         "distance": 300, "segment": { "id": 555, "name": "…", "activity_type": "Ride" }, … } ],
  "best_efforts":   [ { "id": 222, "name": "1k", "distance": 1000, … } ],
  "splits_metric":  [ { "distance": 1000, "elapsed_time": 160, "moving_time": 160, "split": 1, "average_speed": 6.25, "pace_zone": 0 } ],
  "splits_standard":[ { "distance": 1609.34, … } ],
  "laps":           [ { "id": 333, "name": "Lap 1", "lap_index": 1, "distance": 3830.6, "moving_time": 629, … } ],
  "photos": { "count": 1, "primary": { "unique_id": "…", "urls": { "100": "…", "600": "…" }, "source": 1 } },
  "similar_activities": { "effort_count": 3, "average_speed": 6.0, "trend": { "speeds": [ … ], "direction": 0 } },
  "stats_visibility": [ { "type": "heart_rate", "visibility": "everyone" } ]
}
```

## Athlete & gear — real payloads (2026-08, our account)

### Token exchange athlete — `POST /oauth/token` (`athlete`)
Docs call this a `SummaryAthlete`, but the real payload is **fatter than the documented SummaryAthlete** (it carries `bio`, `weight`, `badge_type_id`, `username` — `weight` is a *DetailedAthlete* field per spec; `bio`/`badge_type_id`/`username`/`id_str`/`friend`/`follower` aren't in the spec at all). → modelled by `AthleteSummaryModel` (we keep those "extra" fields on Summary precisely because exchange returns them).
```json
"athlete": {
  "id": 2394302, "id_str": "2394302", "username": "karql", "resource_state": 2,
  "firstname": "Mateusz", "lastname": "Karkula", "bio": ":)",
  "city": "Kraków", "state": "Lesser Poland Voivodeship", "country": "Poland",
  "sex": "M", "premium": true, "summit": true,
  "created_at": "2013-06-21T16:39:29Z", "updated_at": "2026-08-06T16:09:40Z",
  "badge_type_id": 1, "weight": 80.0,
  "profile_medium": "…/medium.jpg", "profile": "…/large.jpg",
  "friend": null, "follower": null
}
```

### Get Authenticated Athlete — `GET /athlete` → **DetailedAthlete** (`resource_state: 3`)
Superset of the exchange athlete; adds `blocked`, `can_follow`, `follower_count`, `friend_count`, `mutual_friend_count`, `athlete_type`, `date_preference`, `measurement_preference`, `clubs[]`, `postable_clubs_count`, `ftp`, **`bikes[]`**, **`shoes[]`**. → modelled by `AthleteDetailedModel : AthleteSummaryModel` (we only add `bikes[]`/`shoes[]`; the rest we don't need, System.Text.Json ignores them). Gear shape (bikes[]/shoes[] items):
```json
{
  "id": "b805524", "primary": false, "name": "Bianka", "nickname": "Bianka",
  "resource_state": 2, "retired": false,
  "distance": 21207353, "converted_distance": 21207.4
}
```
- **Gear `distance` is metres and can be huge** (21 207 353 m ≈ 21 207 km) → **use `double`, not `float`** (float ~7 sig digits would drop the last digit). `converted_distance` = km.
- `nickname`, `retired`, `converted_distance` are **real but undocumented** (not in the spec's `SummaryGear`). `retired` is useful for gear import (Phase 1c).
- Gear import (1c): `bikes[]` here gives the summary; `GET /gear/{id}` adds `brand_name`/`model_name`/`frame_type` (int)/`description`/**`weight`** (→ `GearDetailedModel`). `id` is a **string**; `frame_type` is an **int**; distances are **metres** (float in spec, but treat as double).
- **`bikes[]` returns only ACTIVE gear — retired bikes are OMITTED.** `GET /athlete` (and the exchange athlete) list only non-retired bikes/shoes, so retired gear is invisible there. The **only** way to fetch a retired bike is `GET /gear/{id}` (returns it with `retired: true`). BikeTracker therefore discovers retired bike ids from synced activities (`gear_id` on old rides) and pulls each via `GET /gear/{id}` — see Phase 1c.
- **`frame_type` int → type map** (Strava's web combo): `1 = Mountain`, `2 = Cross (Cyclocross)`, `3 = Road`, `4 = Time Trial (TT)`, `5 = Gravel`, anything else → `Other`.

### Get Equipment — `GET /gear/{id}` → **DetailedGear** (`resource_state: 3`)
Adds `brand_name`, `model_name`, `frame_type` (int), `description`, and **`weight`** (kg — undocumented; can seed `Bike.WeightKg` in 1c) over the summary gear:
```json
{
  "id": "b10707658", "primary": false, "name": "Sensa", "nickname": "Sensa",
  "resource_state": 3, "retired": false,
  "distance": 29143765, "converted_distance": 29143.8,
  "brand_name": "Sensa", "model_name": "Giulia GF", "frame_type": 3,
  "description": "", "weight": 8.0
}
```

### Docs-vs-reality gotchas (athlete/gear)
- **Endpoint inconsistency:** `GET /athlete/clubs` exists, but there's **no** `/athlete/bikes` / `/athlete/shoes` / `/athlete/gears` — gear only comes embedded in `GET /athlete` or via `GET /gear/{id}`.
- **"SummaryAthlete" is often much leaner than documented** — `GET /clubs/{id}/admins` and `GET /activities/{id}/kudos` (both "SummaryAthlete" per docs) actually return only `{ resource_state, firstname, lastname }` (privacy/premium-driven trimming; docs not updated). We don't model those for BikeTracker.
- Same theme as `utc_offset`: **trust real responses over the spec**; add real-but-unspecced fields by hand.

### Clubs — one club, three inconsistent shapes (the "drama")
The **same club** (id 105951) returns different fields depending on the endpoint — and even the same field carries different values. All three below are the same club:

| field | `GET /athlete` (embedded) | `GET /athlete/clubs` | `GET /clubs/{id}` (DetailedClub) |
|---|---|---|---|
| `resource_state` | 2 | 2 | 3 |
| `member_count` | **0 (bogus here!)** | 1844 | 1844 |
| `membership` / `admin` / `owner` | ✅ present | ❌ absent | ✅ present |
| `description` / `club_type` / `following_count` / `website` | ❌ | ❌ | ✅ (detail-only) |

```jsonc
// GET /athlete → clubs[] item (resource_state 2): has membership/admin/owner, but member_count = 0
{ "id":105951, "resource_state":2, "name":"FFWD Wheels", …photos…,
  "activity_types":[…], "dimensions":[…], "sport_type":"cycling", "localized_sport_type":"Cycling",
  "city":"Zwolle","state":"Overijssel","country":"Netherlands",
  "private":false, "member_count":0, "featured":false, "verified":true, "url":"ffwdwheels",
  "membership":"member", "admin":false, "owner":false }

// GET /athlete/clubs → item (resource_state 2): NO membership/admin/owner, real member_count 1844
{ …same core…, "member_count":1844, "url":"ffwdwheels" }

// GET /clubs/{id} → DetailedClub (resource_state 3): superset + detail-only fields
{ …same core + membership/admin/owner…, "member_count":1844,
  "description":"Get Confident. Go Fast. …", "club_type":"company",
  "following_count":1, "website":"http://www.ffwdwheels.com" }
```

Rozkmina / gotchas:
- **`member_count` is 0 in the `GET /athlete` embedded club** — don't trust it there; use `/athlete/clubs` or `/clubs/{id}` for the real count.
- **`membership`/`admin`/`owner`** appear in the embedded `/athlete` club and in `/clubs/{id}`, but **NOT** in `/athlete/clubs` (even though that's literally "my clubs" — so the membership context is oddly dropped exactly where it's most expected).
- `resource_state` is 2 in both summary shapes and 3 in the detailed one — but the "summary" shapes carry different subsets, so resource_state isn't a reliable indicator of which fields you'll get.
- Detail-only (`/clubs/{id}`): `description`, `club_type`, `following_count`, `website` — all **simple scalars** (no collections), so there's no technical reason they couldn't be returned everywhere; Strava just doesn't.
- **Model impact for us: none.** `ClubSummaryModel` covers `/athlete` + `/athlete/clubs` (incl. membership/admin/owner). We don't call `GET /clubs/{id}`, so a `DetailedClub` model (with description/club_type/following_count/website) is intentionally **not** added until something needs it.

# Location (proximity) filter on the koms lists

**Status:** Done (live UI smoke pending on a running stack)
**Date:** 2026-07-22

## Goal

Next to the Direction filter, let the user narrow segments to those whose **start** lies within a radius of a point they pick on a map. Available on the same three surfaces as Direction: koms page, `KomsListDialog` modal, koms-changes page. Clicking the filter opens a map dialog; the user picks a point by clicking the map, sets a radius (1–25 km, default 10) with a slider that draws a live circle, and applies.

Kept deliberately simple: **no geocoding / no search field / no athlete-city default** (all would need an external API call). The map opens centered on Kraków (50.062390, 19.939133); selection is click-only.

## Decisions (with rationale)

- **Expose `StartLatitude`/`StartLongitude` on `SegmentViewModel`** instead of decoding `MapPolyline` client-side. The endpoints already exist on `SegmentEntity` + DB and AutoMapper maps by name (no `DtoProfile` change), so it's ~2 lines and avoids decoding a polyline for every segment on every filter re-eval. Payload grows by 2 floats/segment — negligible. (Only start coords are exposed; end coords aren't needed.)
- **Filter by the segment START** (per request) via a new `GeoHelper.GetDistance` (haversine, km) — lives beside `GetBearing` in `Application.Shared` so it's shared and unit-tested.
- **Client-side, in the existing `Filter=` predicate** — same as Direction: early `return false` when the start is farther than the radius. Instant, no re-fetch. AND-combines with search/direction/activity/club.
- **Trigger looks like the sibling filters** — a reusable `LocationFilterField` (`Shared/`) rendering an Outlined `MudField` (`Label="Location"`, pin icon); empty shows "Select on map…", set shows `{km} km · {lat}, {lng}` with a trailing ✕ (stops propagation) that clears. Placed **last**, after Direction, in each grid `MudItem xs=12 sm=4 md=3 lg=2`.
- **Live, subtle radius ring** — a **metric** `Circle` (radius in meters via `ICircleFactory`) drawn at the picked point and resized live as the slider moves; styled translucent (`Weight=1`, `FillOpacity=0.12`, primary color). A small `CircleMarker` marks the center. (`CircleMarker` radius is pixels — wrong for a metric area.) **`Circle.SetRadius` is unusable in FisSst 1.0.1** — it serializes the live Leaflet circle and throws "Converting circular structure to JSON", so the ring is **remove+recreated** (`DrawCircleAsync`) on each radius change instead.
- **Cursors** — the trigger is click-to-open, not a text input, so `.location-filter-field *` is forced to `cursor: pointer` (MudField's inner input defaults to a text caret); the picker map shows `cursor: crosshair` to signal click-to-pick.
- **Map created after the dialog opens** with its own `DivId="locationMapId"` + fixed-height CSS (`.map-location { height:420px }`), because the page `.map-wrapper` heights are `100vh`-based and FisSst 1.0.1 has no `InvalidateSize`. Init is deferred one render + `Task.Delay(300)` (mirrors `MapPage`) so tiles don't render gray inside the animating dialog.
- **Dialog returns a value** — first use of the `MudDialog.Close(DialogResult.Ok(result))` + `await dialog.Result` round-trip in this repo. Result is a `LocationFilter(double Lat, double Lng, double RadiusKm)` record. In the modal (`KomsListDialog`) the map dialog opens as a **nested** dialog — supported by `MudDialogProvider`.

## Changes

Backend:
- `API.Shared/ViewModels/Segment/SegmentViewModel.cs` — `StartLatitude` / `StartLongitude` (float, auto-mapped by name; no `DtoProfile` change).
- `Application.Shared/Helpers/GeoHelper.cs` — `GetDistance(lat1, lon1, lat2, lon2)` haversine, km (R=6371).
- `Application.Tests/Helpers/GeoHelperTests.cs` — `GetDistance` tests (same point = 0; 1° ≈ 111.19 km; Kraków→Warszawa ≈ 252 km).

Frontend:
- New `Shared/LocationFilter.cs` (record), `Shared/LocationFilterField.razor` (trigger), `Shared/LocationFilterDialog.razor` (map + `MudSlider<double>`).
- `wwwroot/css/app.css` — `.map-location` + `#locationMapId` (dialog-scoped, fixed height).
- `Pages/Koms.razor`(+`.cs`), `Shared/KomsListDialog.razor`, `Pages/KomsChanges.razor`(+`.cs`) — a Location `MudItem` after Direction; `_locationFilter` field; `OpenLocationDialogAsync` (inject `IDialogService`); early `return false` in `Search`/`SearchChanges` using `GeoHelper.GetDistance(... seg.StartLatitude, seg.StartLongitude)`.

## Verification

- `dotnet build src/KomTracker.sln` green; `dotnet test …GeoHelperTests` green (19 cases).
- UI smoke (running stack): `/koms` → Location opens map on Kraków → click a point (marker + subtle circle) → drag slider (circle resizes live) → Apply → list narrows to segments starting inside the radius; field shows `10 km · lat, lng`; ✕ clears. Combines with Direction/search. Ranking → koms count → modal: nested map dialog works. `/koms-changes`: Location (last) narrows client-side; Club still re-fetches.

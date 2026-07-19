# Map: segment start/end markers on click

**Status:** Done (live UI smoke pending on a running stack)
**Date:** 2026-07-21

## Goal

On the (beta) map, KOMs were drawn as orange polylines with a popup, but you couldn't tell where a segment starts or ends. Match Strava: clicking a segment marks its **start with a green dot** and its **end with a checkered finish flag**.

## Decisions (with rationale)

- **Click-to-reveal, not always-on** — the full line is already drawn for every KOM, so the missing piece is just start/end on the selected one. Showing markers only for the clicked segment (replacing the previous pair) matches Strava, avoids clutter, and keeps it light (2 markers at a time vs 2× hundreds via JS interop).
- **Start = `CircleMarker`, end = `Marker` with an SVG flag icon** — `CircleMarker` needs no asset for the green dot; the flag is a small `wwwroot/img/finish-flag.svg` set via `IIconFactory` + `Marker.SetIcon` (`MarkerOptions.Icon` is a low-level `IJSObjectReference`, so `SetIcon(Icon)` is the right API).

## Changes (`Pages/MapPage.razor.cs`)

- Inject `ICircleMarkerFactory`, `IMarkerFactory`, `IIconFactory` (FisSst.BlazorMaps, registered by `AddBlazorLeafletMaps()`).
- Build the finish-flag `Icon` once in `AddPolylinesAsync` (`IconUrl = img/finish-flag.svg`, `IconSize (28,28)`, `IconAnchor (5,26)` so the pole base sits on the point).
- After creating each polyline, subscribe `polyline.OnClick(_ => HighlightSegmentAsync(start, end))` (start/end = decoded polyline first/last point). Popup still opens via the existing `BindPopup`.
- `HighlightSegmentAsync` `Remove()`s the previous `_startMarker`/`_endMarker`, then adds a green `CircleMarker` (radius 6, white stroke, green fill) at start and a flag `Marker` at end (`SetIcon(_finishIcon)`).
- New asset `wwwroot/img/finish-flag.svg`.

## Post-implementation notes

- **Custom marker icon dropped — FisSst.BlazorMaps 1.0.1 can't render one.** Two runtime failures killed the flag `Marker`: with empty options the icon was null (`Cannot read properties of null (reading 'createIcon')`), and setting it via `SetIcon` before `AddTo` produced a non-Leaflet icon (`t.icon.createIcon is not a function`), which also broke `Remove` (`_leaflet_events` undefined). `Icon.JsReference` isn't public, so `MarkerOptions.Icon` isn't usable either. **Resolution: both endpoints use `CircleMarker`** — green (#2e7d32) for start, dark (#212121) for finish. No icon, no flag; reliable and clearly distinguishes start vs end. The `finish-flag.svg` asset was removed.
- **Always-on start dots added** (per the follow-up ask): a small `CircleMarker` at each segment start with a hover tooltip (name) + the same popup + click-to-highlight, so starts are visible without clicking. Zoom-based permanent name labels (Strava's overview) remain a deferred nice-to-have.
- A real checkered flag would need either a maps-library upgrade or raw Leaflet `L.divIcon` via JS interop (this wrapper exposes no DivIcon) — deferred.
- **Highlight is sticky + re-entrancy-guarded + clears on empty-map click.** Clicking the same segment again keeps the highlight (no toggle-off — re-clicking a start should not un-highlight it); only an empty-map click clears. A `_highlighting` flag stops the polyline's and start-dot's click handlers (they overlap at the start) from racing and leaking a stray marker pair; markers are nulled right after `Remove()`. A map `OnClick` clears the highlight when clicking away. Only the **highlighted** segment's line is thickened via `Polyline.SetStyle` (`BaseWeight 3` → `HighlightWeight 6`), restored on clear — the other lines keep their default weight.
- **`SetStyle` must repeat `Color`.** `PathOptions` without `Color` makes Leaflet reset the stroke to its default blue (`#3388ff`), so both the highlight and the restore pass `Color = SegmentColor` (the primary orange) alongside `Weight`.
- **No `_suppressNextMapClear`.** An earlier version assumed a polyline click also bubbles to the map `OnClick` and used a suppress flag to skip it — but in this wrapper a polyline click does **not** fire the map click, so the flag was set and never consumed, which meant clearing needed two empty-map clicks (first only reset the flag). Removed it: the map `OnClick` fires only on genuine empty-map clicks and clears in one.

## Verification

- `dotnet build` green (API signatures confirmed by compile).
- UI smoke (running stack, `/map`): click a segment → green dot at start, checkered flag at finish; clicking another moves the pair; popup still opens; only two markers exist at a time.

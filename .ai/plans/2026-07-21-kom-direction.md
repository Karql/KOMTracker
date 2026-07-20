# KOM geographic direction (compass bearing)

**Status:** Done (live UI smoke pending on a running stack)
**Date:** 2026-07-21

## Goal

Show each segment's geographic direction — the start→end compass bearing (0–360°) + an 8-point cardinal bucket (N…NW) — on the effort grids: a rotated arrow with the angle, sortable by degrees, filterable by cardinal.

## Decisions (with rationale)

- **Compute server-side from the stored endpoint coordinates, not the polyline** — `SegmentEntity` already stores `StartLatitude/StartLongitude/EndLatitude/EndLongitude`, so the bearing is a trivial start→end calc; no polyline decode. Computed once in mapping and exposed on `SegmentViewModel`, it's uniformly available for display + sort + filter across every grid, with no per-page geo math (the map stays client-side, unrelated).
- **Only `Bearing` (double) crosses the wire; `Direction`/`DirectionText` are computed properties** on the shared `SegmentViewModel` (API.Shared is shared with WEB), so they compute identically client- and server-side. Raw coordinates stay unexposed.
- **8-point compass** (matches the maintainer's N/NE/E/SE… examples); `((int)Math.Floor(bearing/45 + 0.5)) % 8`.

## Changes

Backend:
- `Application.Shared/Models/Segment/CompassDirection.cs` (enum N..NW).
- `Application.Shared/Helpers/GeoHelper.cs` — `GetBearing` (atan2 initial bearing, normalized 0–360), `GetCompassDirection`, `GetCompassDirectionText`.
- `SegmentViewModel` — `Bearing` (double) + computed `Direction`/`DirectionText`.
- `API/Mapings/DtoProfile.cs` — map `Bearing` from the entity endpoints.

Frontend:
- New `Shared/DirectionArrow.razor` — SVG arrow rotated by `Bearing` (0 = north/up; CSS clockwise = compass-positive), degrees below, cardinal in tooltip. Integer degrees for the `rotate()` value (locale-safe).
- New **"Dir"** column, second-to-last (before Type), in `Pages/Koms.razor`, `Pages/KomsChanges.razor`, and `Shared/KomsListDialog.razor` (the unified modal behind Battle Field + both Ranking modals). Sort by `Segment.Bearing`.
- **Cardinal direction filter (done, 2026-07-22)** — a `MudSelect<CompassDirection?>` (`Clearable`) above the grid on the koms list, koms-changes and the KOM-list modals, plus an activity-type filter added to koms-changes.

## Direction filter (2026-07-22)

### Decisions (with rationale)
- **Client-side via the table's existing `Filter=` predicate** — every one of these surfaces already holds its efforts in memory and `SegmentViewModel.Direction` is a computed client-side property, so the filter just folds into the `Search(...)` predicate (fail-fast on a direction mismatch before the name match). No API/backend change; the list narrows instantly on select. The koms-changes **Club** filter stays server-side (it re-fetches a different athlete set), but its new **activity-type** filter is also client-side (same in-memory rows), so only the predicate changed there.
- **Combo above the grid, not in the toolbar** — an earlier version put a filter in `<ToolBarContent>` next to the compact search `MudTextField`, which misaligned heights against an `Outlined` `MudSelect`. Moved to the `<MudGrid Class="mb-2"><MudItem xs="12" sm="3">` pattern used by Ranking/KomsChanges/BattleField, leaving the search box alone.
- **`DirectionArrow` gained an optional `ShowDegrees` (default true)** so the dropdown options can show just the rotated arrow (`Bearing = (int)dir * 45`) + cardinal text, reading the same as the "Dir" column, without a degree label. Default keeps all existing call sites unchanged.
- **Direction filter is always last** in each filter row (per the maintainer); koms-changes order is Club, Activity type, Direction.

## Verification

- Unit tests `GeoHelperTests` (15 cases): cardinal bearings N/E/S/W ≈ 0/90/180/270; compass buckets for the 8 cardinals + off-axis + 337.5→N.
- `dotnet build` + `dotnet test` green (147 total).
- UI smoke (running stack): Dir column shows on koms / koms-changes / modals; arrow points correctly (due-east ≈ 90° points right); sort orders by angle; the direction filter narrows the koms list.

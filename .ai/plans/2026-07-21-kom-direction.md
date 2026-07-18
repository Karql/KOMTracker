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
- Cardinal direction filter on the koms page: **deferred** (the enum + `SegmentViewModel.Direction` are ready to wire a `MudSelect<CompassDirection?>` when wanted).

## Verification

- Unit tests `GeoHelperTests` (15 cases): cardinal bearings N/E/S/W ≈ 0/90/180/270; compass buckets for the 8 cardinals + off-axis + 337.5→N.
- `dotnet build` + `dotnet test` green (147 total).
- UI smoke (running stack): Dir column shows on koms / koms-changes / modals; arrow points correctly (due-east ≈ 90° points right); sort orders by angle; the direction filter narrows the koms list.

# BikeTracker — Phase 2b-i: Installations (component → bike)

## Context
Phase 2a gave Components + Warehouses (CRUD), but a component's only "location" is a warehouse — no way to say *this component is mounted on this bike*. Phase 2b adds **Installations** (assign a Component to a parent over a time window with a position). Sliced: **2b-i = component → BIKE** (install / move / remove, positions, date windows, plus **Manual** historical entries), surfaced on bike- and component-detail. **2b-ii** (component-in-component/meta, one component on several bikes at once, full D-7 invariant, D-18 cascade) and **Phase 3** (chain mileage) are out of scope.

Mirrors the Phase-2a vertical (Clean Architecture, `bt.*`, enum-as-string, FluentValidation + `ValidationBehavior`, `ToActionResult`, explicit VM mappings, MudBlazor). Bike-detail has a "Components — coming soon" placeholder; component-detail has an Initial-metrics "later phase" note — the hook points. One EF migration (`bt.installation`).

## Decisions (rationale)
- **D-2b1-1 One `Installation` entity; 2b-i parent = Bike only, `BikeId` nullable FK** (forward-compatible for 2b-ii `ParentComponentId`). Code requires `BikeId` in 2b-i.
- **D-2b1-2 Types `Tracked` (DateFrom + nullable DateTo) and `Manual` (dateless, static `ManualDistanceKm/Hours/ElevationM`, always historical).** No mileage computed in 2b-i — Manual values stored only (aggregation = Phase 3).
- **D-2b1-3 Invariant (D-7 subset): at most ONE active Tracked installation per component (`DateTo == null`).** Install/move while active → `ConflictError`. No multi-bike / component-in-component.
- **D-2b1-4 Actions: Install · Move (atomic close-current + open-new) · Remove (set DateTo) · Delete (hard-delete row).**
- **D-2b1-5 Install clears `Component.WarehouseId`; location = Installed-on-bike ▸ Warehouse ▸ Unassigned.** Remove leaves it unassigned.
- **D-2b1-6 Delete guards (D-18 subset): Component delete blocked when installation history exists (`ConflictError`, prefer Archive); Bike delete removes its installations first** (mirrors `ClearWarehouseAsync`).

## Checklist
- **Domain**: `ComponentInstallationType`, `InstallationPosition` enums; `InstallationEntity`; extend `ComponentEntity` with `[NotMapped]` InstalledOnBikeId/Name/Position.
- **Infra**: `InstallationEntityTypeConfiguration` (bt.installation, referenceless FKs user=Cascade / component,bike=Restrict, enums, indexes); KOMDBContext DbSet+config; `IInstallationRepository`+`EFInstallationRepository` (GetByBike/GetByComponent/GetActiveTrackedByComponent/Get/AnyByComponent/Add/Update/Delete/DeleteByBike); DI; migration `AddInstallationTable`.
- **Application**: Install/Move/Remove/Delete commands (+validators); GetBikeInstallations / GetComponentInstallations queries; extend GetComponents/GetComponent to resolve current install; DeleteComponent guard + DeleteBike cleanup.
- **API**: `InstallationsController` (POST install, PUT {id}/move, PUT {id}/remove, DELETE {id}, GET ?bikeId= / ?componentId=); Installation VMs + mappings; extend ComponentViewModel.
- **WEB**: BikeDetails installed-components panel + Install button; ComponentDetails current-install + history panels + Add-manual-usage; Components list install chip; Install/Move/Remove dialogs.
- **Tests + Docs**: Installation handler+parity tests; CHANGELOG; .ai/README.

## Verification
- `dotnet build src/KomTracker.sln`; `dotnet ef migrations add AddInstallationTable …` + `database update`; `dotnet test`. Live browser check on `https://localhost:5501`.
- Manual: install on bike (Tracked) → shows on both details + list chip; second install blocked (Conflict) until Move/Remove; Move → history closed+new; Remove → unassigned; Manual usage → history; component delete w/ history blocked; bike delete frees components; ownership isolation.

## Decisions — polish (post-review)
- **D-2b1-7 Direct edit of an installation record (`UpdateInstallationCommand`, `PUT /installations/{id}`).** Every field except `Type` is editable — Tracked rows edit bike/position/`DateFrom`/`DateTo`, Manual rows edit bike/position/static totals. *Why: the original "correct a mistake = Remove → Delete → re-add" loop was painful for the common case of a wrong date; a single edit is far better. Type stays immutable (a Tracked↔Manual switch has no coherent data mapping).* The D-2b1-3 invariant is re-checked on edit: reopening a row (`DateTo → null`) while another active Tracked exists → `ConflictError`.
- **D-2b1-8 Install/move/remove dates carry a time-of-day (default midnight).** *Why: "changed the tyre right after today's ride" needs sub-day ordering; a date-only picker couldn't place the install after the ride. Stored as UTC via the existing `EnsureUtc` relabel (no tz conversion — consistent with the rest of BikeTracker).* 
- **D-2b1-9 Installations render as one table (current row highlighted + "Current" chip), on both bike and component pages; the bike page shows historical installs too.** *Why: the standalone green "installed on" banner was weak and the split current/history layout was noisy; a single highlighted table reads better and the bike-side history answers "what's passed through this frame". Row actions: Edit (any) · Move/Remove (current only) · Delete (any).*
- **D-2b1-10 Components list gains install-state (installed / not-installed) + bike filters, and an "Install on bike" row action.** *Why: quick triage of what's mounted vs. spare, and installing without a detour through the detail page.*

## Out of scope
- 2b-ii (component→component, multi-bike, full D-7, D-18 cascade); Phase 3 (chain mileage); service/cost (4); alerts (5); webhooks (6).

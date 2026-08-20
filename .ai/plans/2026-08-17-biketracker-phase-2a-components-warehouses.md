# BikeTracker — Phase 2a: Components + Warehouses (CRUD, no installations)

## Context
Phase 1 is done (garage + Strava sync/mileage for bikes). The next concept phase is **Phase 2 — Components + Installations + Warehouses**. That's large, so it's sliced: **2a = Components CRUD + Warehouses** now; **2b = Installations** (parent = bike/component, positions, date windows, move, invariants D-7, lifecycle cascades D-18) later; **Phase 3 = component mileage via chain attribution**. Confirmed: **no installations and no computed component mileage in 2a**.

A **Component** mirrors the existing `Bike` entity closely (Name/Brand/Model/Weight/Notes/Price/Purchase/Initial metrics/Lifecycle+Sale), so 2a is largely "clone the Bike vertical for Component" plus a tiny **Warehouse** entity (where a not-yet-installed component sits). Everything follows established Bike patterns (Clean Architecture, `bt.*` schema, enum-as-string, FluentValidation + `ValidationBehavior`, `ToActionResult` semantic errors, explicit VM mappings, MudBlazor list/detail/dialog).

Backend + API + WEB. **One EF migration** (`bt.component`, `bt.warehouse`); `bt` schema already exists.

## Decisions (rationale)
- **D-2a-1 Component is a separate entity that mirrors Bike (no shared base).** *Why: concept OQ-3 — shared fields but diverging concerns; cloning the proven Bike vertical is lowest-risk.*
- **D-2a-2 `ComponentCategory` = grouped code-side enum (D-13), NOT a DB lookup table; `ComponentCategoryMetadata` maps category → `ComponentCategoryGroup` only.** *Why: D-13 keeps types code-side; a table only pays off with user-defined categories (out of scope). Registry's sole job is the grouped picker.* **No installable flag on the category** — a future per-component "cost-only" flag (visibility only) is deferred.
- **D-2a-3 Warehouse is a minimal user-owned entity (just `Name`); `Component.WarehouseId` is a nullable current-location FK, `onDelete: SetNull`.** *Why: components need a place pre-install; deleting a warehouse must not cascade-delete its components.*
- **D-2a-4 No installations / no computed mileage in 2a.** Component detail shows Initial metrics only. *Why: installs are 2b, attribution is Phase 3.*
- **D-2a-5 `ComponentLifecycle {Active,Archived,Sold}` parallel enum; D-18 cascade + hard-delete history-guard deferred to 2b.** *Why: matches per-entity enum convention; cascade needs installations.*

## Checklist
See the working plan for the full per-file checklist. Layers: Domain (WarehouseEntity, ComponentEntity, ComponentLifecycle/ComponentCategory/ComponentCategoryGroup enums, ComponentCategoryMetadata) → Infrastructure (EF configs, KOMDBContext DbSets, migration `AddComponentAndWarehouseTables`, IComponentRepository/IWarehouseRepository + EF impls) → Application (Save/ChangeLifecycle/Delete Component commands + validators + ComponentDateHelper, GetComponents/GetComponent queries, Warehouse Save/Delete commands + GetWarehouses query) → API (ComponentsController, WarehousesController, VMs + mappings) → WEB (Components list+detail, Warehouses page, AddEditComponentDialog/SellComponentDialog/AddEditWarehouseDialog, NavMenu) → Tests + Docs.

## Verification
- `dotnet build src/KomTracker.sln`; `dotnet ef migrations add AddComponentAndWarehouseTables --project src/KomTracker/KomTracker.Infrastructure --startup-project src/KomTracker/KomTracker.API` + `database update`; `dotnet test src/KomTracker.sln`.
- Manual: Warehouses add; Components empty-state → add (grouped category picker + warehouse) → card/table → details → edit/archive/sell/delete; delete a warehouse holding a component (component survives, location cleared); ownership isolation.

## Out of scope
- 2b installations (D-7/D-18); Phase 3 chain attribution; per-component cost-only flag; service/cost (4), alerts (5), webhooks (6).

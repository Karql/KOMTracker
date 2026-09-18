# BikeTracker — Phase 2b-ii: Meta-components, multi-bike, D-7, D-18

## Context
Phase 2b-i shipped installations for the simple case: **one component → one bike**, at most one active Tracked window (the D-2b1-3 subset). That's not the real world — a tyre lives *in a wheel* which is *on a bike*, and some parts (a bike computer, pedals) rotate across **several bikes at once**. Phase 3 (component mileage) must attribute rides *through* this chain, so the structural model has to land **before** mileage, or the attribution logic would be built once and reworked. This phase completes installations: component-in-component, multi-bike concurrency, the full **D-7** linkage invariant, and **D-18** lifecycle cascade/detach. Mileage stays out (Phase 3).

One EF migration adds `installation.parent_component_id`.

## Decisions (rationale)
- **D-2b2-1 One installation, parent = `BikeId` XOR `ParentComponentId`.** Add nullable `ParentComponentId` (FK → `bt.component`, Restrict); exactly one of `BikeId`/`ParentComponentId` set per row. *Why: concept's single install mechanism targets a bike **or** a component (§4); 2b-i reserved `BikeId` nullable for this — no reshaping, just the second parent column.*
- **D-2b2-2 Full D-7 invariant (replaces D-2b1-3).** A component's **active Tracked** installations are homogeneous: **either into exactly one parent component, or onto one-or-more (distinct) bikes** — never mixed, never two parent components. A component **inside a parent component has no bike placement**, and one **on bikes has no component parent**. Manual rows are historical and exempt. *Why: unambiguous chain (one logical parent path) so Phase-3 attribution can't double-count.*
- **D-2b2-3 No cycle + at most one component-in-component level.** A component can't be installed into itself or any ancestor (walk-up guard); the target parent component must not itself be inside another component, nor may a component already inside a component accept component-children. *Why: maintainer wants "one level" — permits canonical **tyre → wheel → bike** (one comp-in-comp edge), forbids deeper towers (wheel→wheel→tyre).*
- **D-2b2-4 Multi-bike concurrency (distinct bikes only).** A component may hold **several active Tracked installations, but only onto different bikes** — allowed while on *other* bikes, blocked if already on *that same* bike (no duplicate) or currently *inside a component*. Installing **into a component** requires **no current active installs** (comp-in-comp is exclusive/single). *Why: a bike computer lives on road + gravel; a part sits in only one wheel.*
- **D-2b2-5 D-18 lifecycle cascade/detach.**
  - **Sold** → cascade **Sold** to still-installed children (components whose active parent is this one), closing their windows at the sale date; also close the component's **own** active windows.
  - **Archived** → **detach** open children (close their windows → *unassigned*; history kept; child lifecycle untouched); also close the component's own active windows.
  - **Hard-delete** only when the component has **no installation history** as self *or* as a parent (else Conflict → Archive).
  *Why: history is never rewritten; physical semantics match (sold wheelset takes its tyres; a shelved wheel frees them).*
- **D-2b2-6 One-level display.** Detail pages show the **immediate** parent (bike or component) + **immediate** children, each a link; deeper nesting via navigation. *Why: maintainer preference; a full recursive tree is heavy and rarely needed.*
- **D-2b2-7 `IsMetaComponent` flag gates install targets.** A per-component boolean; only meta components are offered/accepted as a parent (UI parent-picker filters to meta; server rejects installing into a non-meta component). It can't be switched off while the component has installation history as a parent. *Why: without it the "In a component" picker lists the entire inventory, which is unusable once there are many single parts; the flag makes containers (wheels, etc.) explicit and searchable, plus a list filter. The edit guard prevents orphaning nested parts.*
- **D-2b2-8 The "Contains" panel shows current + historical children.** `GetComponentQuery` populates `Component.Children` from `GetByParentComponentAsync` (all rows, current-first), rendered as a table mirroring the installations table (current highlighted + chip, past below). *Why: symmetry — a child shows its full installation history, so the parent should show everything that has passed through it, not just what's mounted now.*

## Checklist

### Domain
- `Entities/Component/InstallationEntity.cs` — add `int? ParentComponentId`; `[NotMapped] string? ParentComponentName`. Parent = `BikeId` XOR `ParentComponentId`.
- `Entities/Component/ComponentEntity.cs` — generalized current-placement read model: `[NotMapped]` `int? ParentComponentId`, `string? ParentComponentName`, `IReadOnlyList<InstallationEntity> CurrentPlacements`, `int InstalledBikeCount`; keep `InstalledOnBikeId/Name/Position` (first bike, back-compat).

### Infrastructure
- `Configurations/Component/InstallationEntityTypeConfiguration.cs` — map `parent_component_id`; referenceless FK → `ComponentEntity` (Restrict); index.
- Migration `AddInstallationParentComponent` + `database update`.
- `IInstallationRepository`/`EFInstallationRepository` — plural `GetActiveTrackedByComponentAsync`; `GetByParentComponentAsync`; batch `GetActiveChildrenByParentComponentsAsync`.

### Application
- `Commands/Installation/InstallComponentCommand.cs` — add `ParentComponentId`; validator XOR target; handler owns target + enforces D-7 via new `InstallationInvariant`; clears `WarehouseId`.
- `MoveInstallationCommand.cs` + `UpdateInstallationCommand.cs` — accept component target; re-run invariant.
- **New** `Commands/Installation/InstallationInvariant.cs` — homogeneity, no-cycle (walk-up), one-level, no dup-on-same-bike, comp-in-comp exclusivity → `ConflictError`.
- `Commands/Component/ChangeComponentLifecycleCommand.cs` — D-18 Sold-cascade / Archived-detach + close own windows.
- `Commands/Component/DeleteComponentCommand.cs` — block delete when self OR parent of any installation.
- Queries `GetComponentQuery`/`GetComponentsQuery` — resolve CurrentPlacements + children; `GetComponentInstallationsQuery` — `ParentComponentName`; `GetBikeInstallationsQuery` unchanged.

### API
- VMs: `InstallationViewModel` +`ParentComponentId/Name`; `InstallComponentViewModel`/`UpdateInstallationViewModel` +`ParentComponentId`; `MoveInstallationViewModel` +`NewParentComponentId`; `ComponentViewModel` +`ParentComponentId/Name`, `InstalledBikeCount`, `CurrentPlacements` + children. Mappings + parity tests.
- `InstallationsController` passes new fields (routes unchanged).

### WEB
- `Shared/InstallComponentDialog.razor` — target toggle *On a bike / In a component* (component autocomplete filtered: exclude self/descendants/one-level violations); multi-bike allowed. Same toggle in Move/Edit dialogs.
- `Pages/ComponentDetails.razor` — Location = immediate parent(s) linked; new **Contains (children)** panel; installations table Parent column.
- `Pages/Components.razor` — chip/filter: *On {bike}* / *On N bikes* / *In {component}* / warehouse / unassigned.
- Sell/Archive dialogs note child cascade/detach.

### Tests + Docs
- Handler tests: install into component; D-7 conflicts (mix, two parents, self, cycle, >1 level, dup same bike); multi-bike ok; move/update to component; D-18 (Sold cascade + close own, Archived detach, delete blocked as parent). Parity; `GetComponentQuery` parent/children.
- `CHANGELOG.md`; `.ai/README.md`.

## Verification
- `dotnet build src/KomTracker.sln`; `dotnet ef migrations add AddInstallationParentComponent …` + `database update`; `dotnet test` green.
- Manual: wheel on bike → tyre into wheel (chain one level each way); bike computer on two bikes; blocked cases (mix, two parents, cycle, >1 level) → Conflict; Sell wheel → tyre cascaded Sold; Archive wheel → tyre detached; delete parent-with-history blocked; ownership isolation.

## Out of scope (later)
- Phase 3 (chain mileage); full recursive tree; service/cost (4); alerts (5); webhooks (6).

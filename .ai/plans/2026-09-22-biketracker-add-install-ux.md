# BikeTracker — faster add/install UX

## Context
Entering components — especially a backlog of historical parts from a spreadsheet — is too many hops today: add a component, find it on the list, click Install, then open the component detail just to close its window (the only place with the uninstall action). This phase streamlines the happy path: chain add→install, let one install step also set an end date (historical closed window), rename the confusing "Remove", and add component-creation straight from a bike (incl. an OEM preset that copies the bike's purchase info). Pure UX/flow work — no new persistence.

## Decisions
- **D-UX-1 Chain add → install (no wizard).** After a component is created, immediately open the existing `InstallComponentDialog` pre-filled; Cancel just skips (the component is already saved). *Why: matches "okienko po save", reuses both dialogs, minimal code vs a rebuilt stepper.*
- **D-UX-2 Install can set an optional uninstall date.** `InstallComponentDialog` (Tracked) gains an optional removal date → one step records an already-closed historical window. `InstallComponentCommand` accepts `DateTo`; the D-7 active-exclusivity invariant and the warehouse-clear run **only when `DateTo == null`** (active), mirroring `UpdateInstallationCommand`'s existing "check only when it stays active" rule. *Why: kills the add→install→open-detail→remove sequence for historical parts.*
- **D-UX-3 Rename "Remove" → "Uninstall" (UI only).** Tooltips + dialog titles/buttons; the endpoint `/installations/{id}/remove` and `RemoveInstallationCommand` keep their names. *Why: "Remove" read like a hard delete next to "Delete record"/"Delete".*
- **D-UX-4 Add component from the bike view, incl. OEM preset.** A split button **"Add component ▾"** on the bike's Installed-components panel with two items, both chaining to install on that bike (preselected):
  - **Add component** — normal create.
  - **Add OEM component** — create pre-filled with the bike's **PurchaseDate**, **PurchasePlace**, **Price = 0**; the chained install pre-fills the install date with the bike's **PurchaseDate**. Prefills only — nothing new is stored. *Why: bulk-entering parts that came with a bike.*

## Implementation

### Backend (allow install with a removal date — D-UX-2)
- `API.Shared/ViewModels/Installation/InstallComponentViewModel.cs` — add `DateTime? DateTo`.
- `API/Controllers/InstallationsController.cs` `Install` — pass `DateTo = model.DateTo`.
- `Application/Commands/Installation/InstallComponentCommand.cs` — add `DateTime? DateTo`; set `DateTo = tracked ? InstallationDateHelper.EnsureUtc(request.DateTo) : null`; run `InstallationInvariant.CheckAsync` **only when `tracked && request.DateTo is null`**; clear `component.WarehouseId` **only when `tracked && request.DateTo is null`**. Validator: when Tracked, `DateTo` (if set) must be after `DateFrom`.

### WEB — dialogs
- `Shared/InstallComponentDialog.razor` — (a) new params `int? PreselectBikeId` (preselect the bike in component-context mode) and `DateTime? DefaultDateFrom` (prefill install date); (b) Tracked branch gets an optional **Uninstall date** (`MudDatePicker` Clearable + time), sent as `DateTo`. Caption: "Set it to record a historical, already-removed installation in one step."
- `Shared/AddEditComponentDialog.razor` — new create-only prefill params `DateTime? DefaultPurchaseDate`, `string? DefaultPurchasePlace`, `decimal? DefaultPrice`; applied in `OnInitializedAsync` when `Component is null`; auto-expand the "Additional details" panel when a prefill is present so the copied purchase info is visible.
- `Shared/RemoveInstallationDialog.razor` — wording → **Uninstall** (title text, body, primary button).

### WEB — flows
- `Pages/BikeDetails.razor` — replace the single add area: keep **Install component**, add a `MudMenu` split button **"Add component ▾"** → items `Add component` / `Add OEM component`. Rename the row **Remove** tooltip → **Uninstall**.
- `Pages/BikeDetails.razor.cs` — `AddComponentAsync` / `AddOemComponentAsync` → shared `AddThenInstallAsync(bool oem)`: open `AddEditComponentDialog` (OEM ⇒ pass the three prefills) → on saved `ComponentViewModel`, open `InstallComponentDialog` with `ComponentId`+`PreselectBikeId = _bike.Id` (OEM ⇒ `DefaultDateFrom = _bike.PurchaseDate`) → `LoadAsync`. Rename `RemoveAsync` dialog title → "Uninstall component".
- `Pages/Components.razor.cs` — `AddAsync`: after the dialog returns the saved component, chain to `InstallComponentDialog` (component context, no bike preselect) → reload.
- `Pages/ComponentDetails.razor` + `.razor.cs` — rename the **Remove** tooltip and `RemoveAsync` dialog title → **Uninstall**.

### Tests
- `Application.Tests/Commands/Installation/InstallationCommandHandlersTests.cs` — add: install with `DateTo` set creates a **closed** window (entity `DateTo` set) and does **not** clear the warehouse / does not hit the active-invariant; existing active-install tests stay green (they pass `DateTo == null`).

## Out of scope
- No new persisted fields (OEM is a prefill preset only). No bulk/CSV import. No changes to Move/Manual semantics.

## Verification
- `dotnet build src/KomTracker.sln`; `dotnet test src/KomTracker.sln` green.
- Manual (WEB): (1) Components list **Add** → after save the Install dialog pops with the new component preselected; Cancel leaves it created. (2) In Install, set an **Uninstall date** → the row shows a closed `from → to` period immediately, component not "current". (3) Bike view **Add component ▾ → Add component** → save → install dialog with the bike preselected. (4) **Add OEM component** → form pre-filled (price 0, bike's place/date), install date defaults to the bike's purchase date. (5) The former **Remove** action now reads **Uninstall** everywhere; **Delete record** unchanged.

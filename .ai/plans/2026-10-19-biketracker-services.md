# BikeTracker — Services (`bt.service`, revises D-17)

## Context
Concept §12 post-v1 item "Services — not designed yet". The maintainer logs maintenance (bike check-up, shock service, wheel rebuild…) with **Name, Date, Place, Cost, Notes** and no lifecycle. Unlike the concept (D-17: one service → exactly one bike/component), **one service links to N bikes and/or components** — one workshop visit ("check-up + shock service") shouldn't be split into several records. Still **no propagation** along the install chain. Bike and component detail pages get a **Services** panel listing each service with **the mileage of that bike/component at the service date**, kept as a stored projection. A Services list page + detail page like Expenses. The Excel "Serwisowe" rows are imported later by one-off SQL outside the repo.

Code name **`ServiceRecord`** (UI "Services", tables `bt.service` / `bt.service_target`) — avoids clashing with `Application/Services/*` (ComponentMileageService…) and DI "services".

## Decisions
- **D-SRV-1 One service → N targets** via `bt.service_target` (`service_id` cascade, `bike_id` XOR `component_id`, unique per service + target); at least 1 target; every target must belong to the user. *Why: one workshop visit covers several things; splitting it into N records duplicates date / place / cost. Revises D-17's "exactly one".*
- **D-SRV-2 No propagation** (D-17 kept): a target is exactly what was serviced — servicing a wheel doesn't touch its tyre. *Why: avoids "what was mounted then" resolution; log the tyre as a second target if it was serviced too.*
- **D-SRV-3 Fields:** Name (required, 200), Date (required; date + time-of-day defaulting to 00:00, like installations — D-2b1-8), Place (optional, 200, shared place autocomplete), Cost (optional, ≥ 0; null = unknown, like component Price), Notes (optional, 2000). No lifecycle. *Why: the maintainer's field list; time-of-day lets the odometer cut exactly before/after a same-day ride, same convention as install dates; a service is an event, so Active/Archived/Sold makes no sense.*
- **D-SRV-4 Mileage at service = stored projection on the target row** (`distance_km`, `moving_hours`, `elevation_m`, `computed_at` on `bt.service_target`), recomputed from source (D-14). Bike: seed + Σ its Strava gears' rides with `StartDate <` service date. Component: seed + all Manual baselines (dateless = historical) + Tracked windows (incl. the in-parent overlap) **clipped at the service date**. *Why: a component with many windows across bikes over ~8k activities would be slow to compute on every detail view (maintainer's call); the projection makes reads trivial, and recompute-from-source means no drift.*
- **D-SRV-5 Recompute is event-driven through the existing reactor.** `ComponentMileageProjectionUpdater` already resolves events → affected component ids (expanded to children) and bikes; it additionally dispatches `RecalculateServiceMileageCommand { ServiceIds, ComponentIds, BikeIds }`. Triggers: a service saved (`ServiceRecordChangedNotification`), `ComponentChanged`, `InstallationChanged`, `AthleteActivitiesSynced`, `ActivitySynced`, and a new **`BikeChangedNotification`** published by `SaveBikeCommand` (initial metrics) and Strava link / unlink (they published nothing before — the same event now also refreshes that bike's component mileage, closing an existing staleness gap). Best-effort + an admin rebuild endpoint, like component mileage. *Why: one targeting place, no handler-calls-handler; an edited ride before a service date or a moved install window corrects the stored value.*
- **D-SRV-6 Deleting a bike/component that a service covers is blocked** (409 with the reason), FKs `Restrict`; archive/sell are unaffected; deleting a service deletes its targets. *Why: the maintainer's choice — service history must not silently lose what it covered.*
- **D-SRV-7 Cost is per service**, not split per target. *Why: the invoice is one amount; how to split it across targets is a Phase 4 cost-rollup rule (OQ-16).*
- **D-SRV-8 UI:** Services page in the Bike Tracker menu (search name / place / target, year + target filters, card/table toggle, newest first); detail page (Details / Covers table with per-target mileage / Notes; Edit, Delete); add/edit dialog with a **multi-target picker** (autocomplete over bikes + components → removable chips — the app's first multi-select); a **Services panel** on bike and component details (Name link, Date, Place, Cost, Mileage at service), **shown only when the bike/component has services**, plus an **Add service** button in the page header that pre-selects it. *Why: the maintainer wants both the page and the panels ("a separate Services panel, if there are any"); the header button keeps adding possible when the panel is hidden.*
- **D-SRV-9 Excel "Serwisowe" import = SQL outside the repo** (components-migration pattern), after the feature. *Why: same as D-EXP-9 — a one-off migration of one user's data.*

## Checklist
- [x] Spec persisted.
- [x] **Domain** `Entities/ServiceRecord/`: `ServiceRecordEntity`, `ServiceTargetEntity`.
- [x] **Infrastructure**: `bt.service` / `bt.service_target` configurations, DbSets, `IServiceRecordRepository` / `EFServiceRecordRepository`, DI, migration `AddServiceTables`.
- [x] **Mileage at date**: `ComponentMileageService` cutoff + `ComputeTotalsAtAsync` / `ComputeBikeTotalsAtAsync`.
- [x] **Application**: `SaveServiceRecordCommand` (+ validator), `DeleteServiceRecordCommand`, `RecalculateServiceMileageCommand`, queries (list, single, by target); `BikeChangedNotification` + `ServiceRecordChangedNotification` wired into the updater; delete blocks in `DeleteBikeCommand` / `DeleteComponentCommand`; service places in suggestions.
- [x] **API / API.Shared**: `ServicesController`, admin recalculation endpoint, view models + mappings.
- [x] **WEB**: `Services` page, `ServiceDetails` page, `AddEditServiceDialog` (multi-target picker), `ServicesPanel` on bike + component details, nav entry, delete-block messages.
- [x] **Tests**: handlers, validators, contract parity, delete blocks, mileage-at-date, updater targeting, suggestions.
- [x] **Docs**: CONCEPT (Service, D-17, §12), `.ai/README.md`, `CHANGELOG.md`.
- [ ] Manual verification.

## Out of scope
- Cost split / rollups (Phase 4). Alerts "reset on service" (Phase 5 — services are the natural reset markers later). Service types/categories, labour vs parts (OQ-8). Attachments (OQ-18).

## Verification
- `dotnet build src/KOMTracker.sln`; migration `AddServiceTables` + `database update`; `dotnet test src/KOMTracker.sln` green.
- Manual (local): add a service covering a bike + 2 components (one inside a wheel) → the detail page shows per-target mileage matching the odometer at that date; bike/component pages show the Services panel and Add service pre-selects the target; editing the date recomputes; refreshing/editing a ride before the date updates the value after sync; deleting a covered bike/component → 409 with the reason; list search / year / target filters; place suggestions include service places.

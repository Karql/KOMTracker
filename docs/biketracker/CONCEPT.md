# BikeTracker — Concept & Domain

**Status:** Draft — living document. We brainstorm here; phase-by-phase implementation specs are later distilled into `.ai/plans/`.
**Source:** organized from the maintainer's raw notes in `notes/BikeTracker.md`.
**Language:** English (repo convention); design discussion happens in Polish.

## 1. Vision & scope
A bike **maintenance journal** for a handful of friends: track bikes, their components, mileage / hours / elevation (mostly auto from Strava), service & purchase costs, resale, and (later) maintenance alerts. Built as a **feature module inside the existing KOMTracker app** (shared Strava login, tokens, DB and front-end).

**Why:** the real driver is wanting **my own tool, laid out exactly the way I want it** — free and self-hosted, no paywall. Distinctive vs the existing (paid) tools: correct Strava mileage attribution through the install chain, meta-components (group swap), and one component tracked across multiple bikes at once. (OQ-1 resolved.)

**In v1:** Strava integration (import gears + activity sync via webhooks + polling), bikes, components, installations (incl. meta-components/groups, multi-bike, positions, dateless historical), warehouses, mileage/hours/elevation projections, lifecycle (archive/sell).
**Out of v1** (model fields still added now): alerts, cost/resale analysis. (OQ-14 resolved.)

## 2. Actors
- **Rider / Owner** — a Strava-connected athlete managing their **own** garage. Sees only their own data.
- **Admin** — reuse KOM admin if/when needed.

## 3. Ubiquitous language
- **Bike** — a meta bike owned by an athlete; optionally linked to external-service bikes (Strava "gear") for auto data.
- **Component** — a wearable/replaceable part (chain, tyre, wheel, bar tape…) with a category. Non-installable cost items (tools, lube, clothing) are just components in non-installable categories.
- **Installation** — assignment of a component to a **Bike or to another Component** over a time window and position; the single mechanism for both "component on bike" and "component in component" (grouping).
- **Activity** — a ride imported from an external service (Strava), attributed to a bike; source of mileage/time/elevation.
- **Warehouse / Store** — a place holding non-installed components (Home, Garage, drawer…).
- **Service** — a maintenance/repair record with cost.
- **Projection** — precomputed totals (mileage/hours/elevation) per bike/component, maintained by jobs.
- **Lifecycle status** — Active / Archived / Sold (see §4).

## 4. Domain model (draft)
All top-level entities are owned by an **Athlete** via a strong FK (D-1).

### Bike
Meta entity. Fields: Name, Brand, Description, **Type** (our enum, seeded from Strava's `frame_type`: Road / Mountain / TT / Cross / Gravel — extend with e.g. Enduro / DH / City), **Weight** (kg, optional — **not in the API**, user-entered), Notes, Price, PurchasePlace, PurchaseDate, Initial mileage / hours / elevation, **Lifecycle status + SaleDate/SalePrice** (§ Lifecycle).
- **BikeExternalLink** `{ ExternalService, ExternalId }` (1 bike → N links) — decouples from Strava; enables future Garmin etc. `ExternalId` is a **string** (Strava gear id, e.g. `b1234567`). (D-2)

### Component
Fields: Name, Category (see seed list), **Weight** (kg, optional), Notes, Price, PurchasePlace, PurchaseDate, Initial mileage / hours / elevation, current Warehouse (when not installed), **Lifecycle status + SaleDate/SalePrice**.
- Shares many fields with Bike but kept as a **separate entity** (no forced shared base). *(OQ-3: revisit only if duplication hurts.)*
- **Seed categories** (editable; mirrors Strava's list + extras) — *installable:* Chain, Front/Rear Wheel, Front/Rear Tyre, Cassette, Chainrings, Crankset, Bottom Bracket, Front/Rear Derailleur, Front/Rear Brake, Front/Rear Brake Pads, Front/Rear Brake Lever, Handlebar, Bar Tape, Stem, Fork, Pedals, Saddle, Seatpost; *non-installable* (D-4): Tool, Lube/Consumable, Apparel. *(OQ-9 resolved.)*

### Installation
Target = **Bike OR Component** (polymorphic parent). Fields: Component, ParentBikeId? / ParentComponentId?, **Type (Tracked | Manual)**, DateFrom, DateTo (null = currently installed), Position (front/rear/left/right/top/bottom/…), **manual Mileage/Hours/Elevation** (Manual type only).
- **Grouping via installation** (parent = Bike or Component) → structurally recursive, shallow in practice; keeps the position binding; moving a component = close one installation, open another. Example: tyre installed *into* a wheel (A); wheel installed *onto* a bike (B); next week move the tyre (close A, open A'). (D-3)
- **Type = Tracked** — has a date window; mileage is computed from activities via the chain.
- **Type = Manual (historical)** — no dates, static Mileage/Hours/Elevation entered by hand (legacy wear: "this tyre did ~X km on that bike"), added as a fixed amount, never recomputed. A Manual installation is **always historical → never "currently installed"** (you may have many across bikes). Because it's static, its numbers live on the installation row — no extra projection table needed for this case.
- **Concurrency is constrained by the linkage invariant** (D-7): a component's active installations are either into **one** parent component, **or** onto **one-or-more bikes** (e.g. a bike computer on road + gravel) — never mixed, never two parent components.

### Activity
A ride imported from Strava. Fields (Strava names in brackets; **units: metres / seconds**):
- ExternalService, ExternalId (Strava activity id), **Bike** (resolved via `gear_id` → BikeExternalLink; `gear_id` is a **string and nullable/clearable** ("none") — **activities without a `gear_id` are unattributed** and skipped for bike/component mileage), Date (`start_date` = **UTC** — use for installation-window overlap; `start_date_local` + `timezone` for display), **SportType** (`sport_type`, e.g. MountainBikeRide / GravelRide / Ride / EBikeRide / VirtualRide; `type` is deprecated), Distance, MovingTime (`moving_time`), ElapsedTime (`elapsed_time`), Elevation (`total_elevation_gain`).
- Flags: `trainer`, `commute`, `manual`.
- Optional / later: power (`average_watts` / `weighted_average_watts` / `device_watts`), **`kilojoules`** (work done — candidate wear metric), HR, speed, calories, elev_high/low.
- MVP tracks **distance / moving time / elevation** (D-6). *(OQ-17: which rides count toward wear — `trainer` / virtual / `manual` / `flagged`?)*

### Warehouse / Store
Fields: Name. Holds non-installed components. (Retirement is a lifecycle status, not a warehouse — see below.)

### Service
Fields: Bike or Component, Date, Cost, Place (autocomplete). *(OQ-8: description/type, labour vs parts?)*

### Projection
Precomputed totals per bike/component (mileage/hours/elevation), maintained by jobs so the UI never groups+sums on the fly. *(OQ-10: columns on base entities vs separate projection tables.)*

### Lifecycle & sale (Bike + Component)
- **Status: Active → Archived → Sold.** *Archive* is a user action that hides the item from the main view but you still own it (like archiving an email; the warehouse acts as a label). *Sold* means you no longer own it and records **SaleDate + SalePrice**, which feeds cost analysis (e.g. wheels ridden 1 year, sold for 2k). Both Archived and Sold drop out of the default "active garage" view. (D-5)
- **Selling a bike cascades "Sold" to the components still installed on it** — to keep a component, detach it before selling. (OQ-15 resolved; part of D-5.)

## 5. Use cases (grouped)
- **Garage:** add/edit/archive/sell a bike; view active garage; view a bike's components & totals.
- **Components:** add a component; set category; assign to a warehouse; archive/sell; record a dateless historical usage.
- **Installations:** install a component on a bike or into a parent component (with position + date); move it (close + open); install one component on several bikes at once.
- **Strava:** import gears as bikes; auto-sync activities; attribute mileage to bikes → components.
- **Service & cost:** log a service with cost; see cost analysis per bike/component (incl. resale).
- **Alerts (later):** set a threshold (km/hours/period) on a component/bike; get notified when due.

## 6. Key flows
### Strava import
- Import **gears** (bikes) from `GET /athlete` (`bikes[]`) + `GET /gear/{id}` → create/link a Bike via BikeExternalLink. Map `brand_name` / `model_name` → Bike (Brand); `frame_type` is an **integer** → small int→`Bike.Type` map (Phase-1 detail); seed Initial mileage from the gear's `distance` (**float, metres**); `primary` marks the main bike; gear `id` is a **string**. (Strava gears cover bikes + shoes — we import bikes only.)
- **Activity sync:** webhooks (push) **+** periodic polling fallback (missed webhook). Attribute each activity to its Strava gear → Bike. *(OQ-11: webhook callback hosting + subscription; polling cadence.)*

### Mileage attribution (the crux — next brainstorm round)
- **Bike total** = initial + Σ its activities.
- **Component total** = initial + Σ Manual-installation baselines + Σ activities reaching it through the **installation chain** (Tracked windows only) during overlapping windows (a tyre gets the km its wheel got while that wheel was on a bike, during the tyre-in-wheel window); summed across concurrent bike installations. Tracked for **distance / moving time / elevation** (D-6).
- The linkage invariant (D-7) keeps the chain unambiguous (no mixed parents) → no double counting.
- Only activities **with a `gear_id`** are attributed; ones without are skipped. Window overlap is computed on the activity's **UTC `start_date`**.
- Computed (Tracked) totals recomputed by a job on activity sync / installation change; storage TBD (OQ-10). Manual totals are static on the installation.

## 7. Rules & alerts (later phase)
Thresholds per component/bike on **mileage / hours / elapsed period** → alert when due. *(OQ-13: channel — reuse existing email? in-app? both.)*

## 8. Cost analysis (later phase)
Per component/bike and per athlete: purchase + services − resale = **net cost**; cost/km; cost/period. Bike-level rollups may include attached components. *(OQ-16: rollup rules.)*

## 9. Constraints / NFR
- Feature module **inside KOMTracker**: `BikeTracker/` folders/namespaces in existing `KomTracker.*` projects; entities in `KOMDBContext` with strong FK to `Athlete`; reuse the existing Blazor front (new "Bike" nav); shared identity + Strava tokens + one `client_id`; weak VPS; few users.

## 10. Decisions (with rationale)
- **D-1 In-place modular monolith** — folders in existing projects, one `KOMDBContext` with strong refs to `Athlete`, reuse front, shared identity/tokens/DB/`client_id`. Separate projects tangled refs; strong refs need one context; hobby scale favours one app.
- **D-2 Bike = meta + `BikeExternalLink`** — decouple from Strava, enable future services.
- **D-3 Grouping via Installation (parent = Bike or Component)** — one mechanism for both; recursive-capable but shallow in practice; keeps the position binding and handles moving a component between parents cleanly.
- **D-4 Accessories/tools/consumables = component categories** (never installed) — avoids a parallel entity; still cost-tracked.
- **D-5 Lifecycle Active/Archived/Sold + resale** — archive hides from main view (still owned); sold records SaleDate/SalePrice for cost analysis; applies to Bike and Component. **Selling a bike cascades Sold to its still-installed components** (detach first to keep them).
- **D-6 Track three wear metrics** — distance, moving time, elevation totals per bike/component; alerts pick whichever fits (chain→km, suspension→hours).
- **D-7 Component-linkage invariant** — a component's *active* installations are homogeneous: either into **exactly one** parent component, **or** onto **one-or-more bikes** — never mixed, never two parent components at once. `Installation.Type = Manual` (dateless, static totals) is always historical, never active.
- **D-8 Strava's public API (v3) exposes only bikes + activities** — no components at all, and even bike `weight` is web-only (`DetailedGear` = brand/model/frame_type/description; the `weight` in the spec is the *athlete's*). Strava's newer **web** UI added a *basic* components feature, but it's **per-bike, non-movable, and not in the API** — so it validates our differentiators (moving components between bikes, meta-components) rather than threatening them. Import stays **bikes + activities**; the component / installation / mileage-attribution layer is fully ours. We do **not** replicate Strava's auto-generated "Frame" component — our Bike is that top level.

## 11. Open questions
OQ-3 shared base for Bike/Component (defer) · OQ-8 service fields (labour/parts, post-v1) · OQ-10 computed-projection storage (columns vs tables) · OQ-11 webhook hosting + polling cadence · OQ-13 alert channel (with alerts, post-v1) · OQ-16 cost rollup rules (post-v1) · OQ-17 which rides count toward wear (trainer / virtual / manual / flagged) · OQ-18 attachments/documents (receipts/photos) · OQ-19 bike health/condition score.
*(Resolved: OQ-1 vision, OQ-2 D-2, OQ-4/OQ-12 D-3, OQ-5 Manual installation, OQ-6 D-6, OQ-7/OQ-9 D-4/D-5, OQ-14 scope, OQ-15 D-5 cascade.)*

## 12. Phasing (rough → becomes `.ai/plans/`)
**v1:**
- **Phase 0** — Domain skeleton + Bikes CRUD (manual) + garage view + lifecycle (archive/sell).
- **Phase 1** — Strava integration: import gears, activity sync (webhooks + polling), bike mileage.
- **Phase 2** — Components + installations (parent = bike/component incl. meta-components, multi-bike, positions, dateless historical) + warehouses.
- **Phase 3** — Projections/jobs for component mileage/hours/elevation (chain attribution).

**Post-v1:**
- **Phase 4** — Service, purchase & resale cost analysis.
- **Phase 5** — Alerts (channel TBD — OQ-13).

## 13. Parked ideas (competitor scan — post-v1 candidates)
Noted from a scan of the existing tools; not committed, just captured so we don't forget:
- **Attachments / documents** — receipts, manuals, warranties, photos on a bike/component/service. *(OQ-18)*
- **Bike health / condition score** — an aggregate wear/maintenance indicator per bike. *(OQ-19)*
- **Richer alert rules** (Phase 5) — thresholds by distance / hours / calendar time / lifespan; one-off vs recurring; **reset on service** ("usage since last service"); dedicated **chain-wax cadence**.
- **Notification channels** — email (reuse) and/or PWA push, in-app. *(OQ-13)*
- **Import UX** — paste-a-list / one-by-one (AI-assisted import intentionally out of scope).
- **Sharing / public build showcase** — out of scope (private, few friends); noted only.

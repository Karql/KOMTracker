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
- **Bike** — a meta bike owned by the user (app account); optionally linked (via **BikeLink**) to external-service bikes (Strava "gear") for auto data.
- **Component** — a wearable/replaceable part (chain, tyre, wheel, bar tape…) with a category. Non-installable cost items (tools, lube, clothing) are just components in non-installable categories.
- **Installation** — assignment of a component to a **Bike or to another Component** over a time window and position; the single mechanism for both "component on bike" and "component in component" (grouping).
- **Activity** — a ride imported from an external service (Strava), attributed to a bike; source of mileage/time/elevation.
- **Warehouse / Store** — a place holding non-installed components (Home, Garage, drawer…).
- **Service** — a maintenance/repair record with cost.
- **Projection** — precomputed totals (mileage/hours/elevation) per bike/component, maintained by jobs.
- **Lifecycle status** — Active / Archived / Sold (see §4).

## 4. Domain model (draft)
**Schemas:** BikeTracker entities live in **`bt.*`**; data synced from Strava lives in **`strava.*`** as first-class Strava records; KOM tables may migrate to **`kt.*`** someday. One `KOMDBContext` maps all schemas (D-9).
**Ownership:** every `bt.*` entity (Bike, Component, Installation, Warehouse, Service) has a strong FK to the **User** (app account) — for visibility + edit-security (D-11). The `bt` ↔ `strava` boundary is a **soft link** (via the bike link's external id, resolved in app/job logic), **not** an EF navigation (D-10).

### Bike (`bt.bike`, owned by User)
Meta entity. Fields: Name, Brand, **Model**, **Type** (code-side enum — D-13; full list in Appendix §14), **Weight** (kg, optional — **not in the API**, user-entered), **Notes** (single free-text — Description + Notes merged into one), Price, PurchasePlace, PurchaseDate, Initial mileage / hours / elevation, **Lifecycle status + SaleDate/SalePrice** (§ Lifecycle).
- **BikeLink** (`bt.bike_link`) `{ ExternalService, ExternalId }` (1 bike → N links) — the **only** place the external-service abstraction lives; `ExternalId` is a **string** (Strava gear id, e.g. `b1234567`). Bridges to `strava.*` in app/job logic, not via an EF relationship. (D-2/D-10)

### Component
Fields: Name, **Brand**, **Model**, **Category** (code-side enum — D-13; grouped, full list in Appendix §14), **Weight** (kg, optional), **Notes** (single free-text), Price, PurchasePlace, PurchaseDate, Initial mileage / hours / elevation, current Warehouse (when not installed), **Lifecycle status + SaleDate/SalePrice**.
- Shares many fields with Bike but kept as a **separate entity** (no forced shared base). *(OQ-3: revisit only if duplication hurts.)*
- **Categories** = a **grouped** code-side enum (full seed set in Appendix §14). **No front/rear/left/right in the category** — that's `Installation.Position`; the only kept split is **Front/Rear Derailleur** (mechanically distinct). Each category carries a UI `group` + an `installable` flag in the code registry (non-installable = cost-only, e.g. Toolset/Apparel). *(OQ-9 resolved.)*

### Installation
Target = **Bike OR Component** (polymorphic parent). Fields: Component, ParentBikeId? / ParentComponentId?, **Type (Tracked | Manual)**, DateFrom, DateTo (null = currently installed), Position (front/rear/left/right/top/bottom/…), **manual Mileage/Hours/Elevation** (Manual type only).
- **Grouping via installation** (parent = Bike or Component) → structurally recursive, shallow in practice; keeps the position binding; moving a component = close one installation, open another. Example: tyre installed *into* a wheel (A); wheel installed *onto* a bike (B); next week move the tyre (close A, open A'). (D-3)
- **Type = Tracked** — has a date window; mileage is computed from activities via the chain.
- **Type = Manual (historical)** — no dates, static Mileage/Hours/Elevation entered by hand (legacy wear: "this tyre did ~X km on that bike"), added as a fixed amount, never recomputed. A Manual installation is **always historical → never "currently installed"** (you may have many across bikes). Because it's static, its numbers live on the installation row — no extra projection table needed for this case.
- **Concurrency is constrained by the linkage invariant** (D-7): a component's active installations are either into **one** parent component, **or** onto **one-or-more bikes** (e.g. a bike computer on road + gravel) — never mixed, never two parent components.

### Activity (`strava.activity`)
A **first-class Strava record** in `strava.*`, synced raw — *not* a generic entity with `external_service`/`external_id`. It simply **is** a Strava activity (and may feed other features later); the abstraction lives on the bike link, not here. Keyed by the Strava activity id; carries Strava's own `athlete_id` + `gear_id`. **Bridged to BikeTracker by matching `gear_id` ↔ `bt.bike_link.ExternalId` in app/job logic** (no EF nav across the boundary). Fields (units: metres/seconds):
- Id (Strava activity id), `athlete_id`, **`gear_id`** (string, nullable/clearable "none" — **no gear ⇒ unattributed**, skipped), Date (`start_date` = **UTC** for window overlap; `start_date_local` + `timezone` for display), **SportType** (`sport_type`, e.g. MountainBikeRide / GravelRide / Ride / EBikeRide / VirtualRide; `type` deprecated), Distance, MovingTime (`moving_time`), ElapsedTime (`elapsed_time`), Elevation (`total_elevation_gain`).
- Flags: `trainer`, `commute`, `manual`, `private`, `flagged`.
- **Persisted 1:1 with the API** (D-10) — the list above is illustrative, not exhaustive; we store *all* `DetailedActivity`/`SummaryActivity` fields too (power `average_watts`/`weighted_average_watts`/`max_watts`/`device_watts`, `kilojoules`, HR, speed, calories, elev_high/low, map/polyline, device_name, …). **All activities are synced**, including those without a `gear_id`.
- Attribution/MVP *uses* **distance / moving time / elevation** (D-6); everything else is stored for future features. *(OQ-17: which rides count toward wear — `trainer` / virtual / `manual` / `flagged`?)*

### Webhook event (`strava.webhook_event`) — inbox
Raw Strava webhook notification, persisted on receipt for async processing (§6). Fields: raw payload, `object_type` (activity|athlete), `object_id`, `aspect_type` (create|update|delete), `owner_id`, `subscription_id`, `event_time`, `received_at`, `processed`, `attempts`, `error`.

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
- Import **gears** (bikes) from `GET /athlete` (`bikes[]`) + `GET /gear/{id}` → create a `bt.bike` + a `bt.bike_link` (ExternalId = gear id). Map `brand_name` / `model_name` → Bike (Brand); `frame_type` is an **integer** → small int→`Bike.Type` map (Phase-1 detail); seed Initial mileage from the gear's `distance` (**float, metres**); `primary` marks the main bike; gear `id` is a **string**. (Strava gears cover bikes + shoes — we import bikes only.)
- **Activity sync:** baseline is **periodic polling + recalculation** (a job re-fetches each athlete's recent activities on a schedule). **Sync all activities** into `strava.activity` (1:1, regardless of `gear_id`/sport); attribution to a bike happens later via `gear_id` ↔ `bt.bike_link`. Webhooks (below) are a **later enhancement**, not part of the first working app.

### Strava webhooks (inbox + async processing) — later enhancement
> **When:** deferred. We build the app first on **periodic recalculation** (polling); webhooks come later. They **don't change the logic** — same commands, same attribution — they just make data recalculate **sooner** (lower latency). Design captured now so Phase 1's command path is webhook-ready.

- **One subscription per app** (shared `client_id`), **registered once by hand** (curl/Postman) — *not* built into the app. `POST /push_subscriptions` (`client_id`, `client_secret`, `callback_url` ≤255, `verify_token`); view/delete the same way, ad-hoc. The single public HTTPS callback then receives events for **all** authorized athletes (KOM + Bike).
- **The app implements only the callback** (+ worker + polling): the one-time **validation handshake** — Strava GETs the callback with `hub.mode=subscribe` + `hub.challenge` + `hub.verify_token` → reply `200 {"hub.challenge": <value>}` within **2 s** (check `verify_token` from config) — and the event **receipt** (below).
- **Receive = validate + persist + ACK fast, NEVER process inline.** The callback must return **200 within 2 s** (Strava retries up to 3× if not, and one athlete "save" can emit several events). So the endpoint only: checks `subscription_id`/`verify_token`, writes the raw event to the **`strava.webhook_event`** inbox (`processed=false`), returns 200.
- **`strava.webhook_event`** (inbox row): raw payload + `object_type` (activity|athlete), `object_id`, `aspect_type` (create|update|delete), `owner_id` (athlete), `subscription_id`, `event_time` (unix), `received_at`, `processed`, `attempts`, `error`. Gives durability, retries, dedup, and survival across restarts.
- **Async worker** drains unprocessed events → each becomes a **command/task**:
  - activity `create`/`update` → fetch + upsert `strava.activity` (1:1), then recompute affected bike/component projections;
  - activity `delete` → remove/flag the `strava.activity` + recompute;
  - athlete `updates.authorized = "false"` → **deauthorization**: revoke tokens / mark the athlete disconnected.
  Idempotent: dedup by object and **re-fetch current state from the API** rather than trusting event order (events can arrive out of order / duplicated). On success mark `processed`; on failure bump `attempts` + store `error`.
- **Trigger:** the worker runs on a schedule **and** may be kicked immediately after a webhook write (fire-and-forget) for low latency — but the DB inbox stays the source of truth.
- **Polling fallback:** a periodic job re-fetches each athlete's recent activities (catches missed/failed webhooks); same command path, deduped by activity id.
- Scopes: `activity:read` (public) / `activity:read_all` (private activities). *(OQ-11a: job runner — Hangfire (persistent queue + dashboard + retries; new dep) vs the existing **Quartz** draining the inbox. OQ-11b: polling cadence + webhook callback hosting/public URL.)*

### Mileage attribution (the crux — next brainstorm round)
- **Bike total** = initial + Σ its activities.
- **Component total** = initial + Σ Manual-installation baselines + Σ activities reaching it through the **installation chain** (Tracked windows only) during overlapping windows (a tyre gets the km its wheel got while that wheel was on a bike, during the tyre-in-wheel window); summed across concurrent bike installations. Tracked for **distance / moving time / elevation** (D-6).
- The linkage invariant (D-7) keeps the chain unambiguous (no mixed parents) → no double counting.
- Only activities **with a `gear_id`** are attributed; ones without are skipped **at attribution time** (they're still synced + stored). Window overlap is computed on the activity's **UTC `start_date`**.
- Computed (Tracked) totals **recomputed from source** (activities × installation windows), never adjusted incrementally — recompute runs on activity sync / installation change. So **editing or correcting an installation** (e.g. a wrong year) always yields correct totals; no drift. (D-14; storage TBD OQ-10.) Manual totals are static on the installation.

## 7. Rules & alerts (later phase)
Thresholds per component/bike on **mileage / hours / elapsed period** → alert when due. *(OQ-13: channel — reuse existing email? in-app? both.)*

## 8. Cost analysis (later phase)
Per component/bike and per athlete: purchase + services − resale = **net cost**; cost/km; cost/period. Bike-level rollups may include attached components. *(OQ-16: rollup rules.)*

## 9. Constraints / NFR
- Feature module **inside KOMTracker**: `BikeTracker/` folders/namespaces in existing `KomTracker.*` projects; entities in one `KOMDBContext` mapping schemas **`bt.*`** (BikeTracker), **`strava.*`** (synced), future **`kt.*`**; `bt.*` owned by **User**; strong refs **within** a schema, soft `bt`↔`strava` bridge via external id + logic; reuse the existing Blazor front (new "Bike" nav); shared identity + Strava tokens + one `client_id`; weak VPS; few users.

## 10. Decisions (with rationale)
- **D-1 In-place modular monolith** — folders in existing projects, one `KOMDBContext` (EF multi-schema), reuse front, shared identity/tokens/DB/`client_id`. Separate projects tangled refs; strong refs need one context; hobby scale favours one app.
- **D-2 Bike = meta + `BikeLink`** (`bt.bike_link` `{ ExternalService, ExternalId }`) — the **single** external-service abstraction point; decouples from Strava, enables future services (Garmin).
- **D-9 Schema separation in one context** — `bt.*` (BikeTracker), `strava.*` (raw Strava-synced data), future `kt.*` (KOMTracker migration). EF maps multiple schemas in the one `KOMDBContext`; no second DbContext.
- **D-10 Strava-synced data as first-class `strava.*` entities** (e.g. `strava.activity`) — not generic "entity + external_service/external_id" rows. The external abstraction sits only on `bt.bike_link`; the `bt`↔`strava` boundary is a **soft link** resolved in app/job logic (match `gear_id` ↔ link `ExternalId`), never an EF navigation. Strong refs are used only **within** a schema/domain.
  - **Mirror the Strava API models 1:1** — persist **all** fields the API returns (not a trimmed subset), and **sync everything** (e.g. *all* activities, not only those with a `gear_id`). Rationale: a future feature must never force a re-sync just because we skipped a field or a row. Filtering (by `gear_id`, ride `sport_type`, etc.) happens at **attribution/query** time, not at sync.
- **D-14 Projections recompute from source; history is fully editable** — totals are always derived from the source of truth (activities × installation windows + manual baselines), recomputed idempotently, **never** mutated by increments. Installations and history (dates, position, parent, lifecycle) are **freely editable/correctable**, and any edit re-derives affected totals + the bike logbook — so a fixed date is actually fixed (no stale/forward drift). *(Deliberate contrast with a competitor where editing a wrong install date left the total wrong.)*
- **D-13 Types as code-side enums, not lookup tables** — `Bike.Type` and `Component.Category` are C# enums persisted **as strings** (EF `HasConversion<string>`); no `bike_type`/`component_type` tables. Per-type metadata (display name, UI **group**, `installable`?, default wear metric) lives in a code registry/helper. Consistent with the existing `ActivityTypeConsts` / `ExtendedCategoryEnum` pattern; simplest at hobby scale; readable DB values; `Other` is the escape hatch. Front/rear/left/right is **not** a category — it's `Installation.Position`. Revisit a lookup table only if user-defined categories are ever needed.
- **D-12 Webhooks: inbox + async, never inline — and deferred.** Baseline sync is periodic **polling + recalculation**; webhooks are a **later** latency optimization (same commands/attribution, just sooner). When built: the callback only validates + writes a `strava.webhook_event` (`processed=false`) and returns 200 within 2 s (Strava retries ≤3×, multiple events per save). A background worker drains the inbox into idempotent commands (sync/upsert/delete activity; handle deauthorization), re-fetching current state from the API rather than trusting event order. One subscription per app, **registered once by hand** (curl/Postman) — the app builds only the callback + worker, not subscription management. A polling job backstops missed events. Runner (Hangfire vs existing Quartz) is OQ-11a.
- **D-11 `bt.*` owned by the User (app account)** for visibility + edit-security. *Known debt (deferred):* today `UserEntity` carries a Strava `athlete_id` (the relationship is backwards); the app account should be the root with linked external identities (Strava now, Garmin later). Not fixing now — but new `bt.*` ownership targets the User so it's future-proof.
- **D-3 Grouping via Installation (parent = Bike or Component)** — one mechanism for both; recursive-capable but shallow in practice; keeps the position binding and handles moving a component between parents cleanly.
- **D-4 Accessories/tools/consumables = component categories** (never installed) — avoids a parallel entity; still cost-tracked.
- **D-5 Lifecycle Active/Archived/Sold + resale** — archive hides from main view (still owned); sold records SaleDate/SalePrice for cost analysis; applies to Bike and Component. **Selling a bike cascades Sold to its still-installed components** (detach first to keep them).
- **D-6 Track three wear metrics** — distance, moving time, elevation totals per bike/component; alerts pick whichever fits (chain→km, suspension→hours).
- **D-7 Component-linkage invariant** — a component's *active* installations are homogeneous: either into **exactly one** parent component, **or** onto **one-or-more bikes** — never mixed, never two parent components at once. `Installation.Type = Manual` (dateless, static totals) is always historical, never active.
- **D-8 Strava's public API (v3) exposes only bikes + activities** — no components at all, and even bike `weight` is web-only (`DetailedGear` = brand/model/frame_type/description; the `weight` in the spec is the *athlete's*). Strava's newer **web** UI added a *basic* components feature, but it's **per-bike, non-movable, and not in the API** — so it validates our differentiators (moving components between bikes, meta-components) rather than threatening them. Import stays **bikes + activities**; the component / installation / mileage-attribution layer is fully ours. We do **not** replicate Strava's auto-generated "Frame" component — our Bike is that top level.

## 11. Open questions
OQ-3 shared base for Bike/Component (defer) · OQ-8 service fields (labour/parts, post-v1) · OQ-10 computed-projection storage (columns vs tables) · OQ-11a webhook job runner (Hangfire vs existing Quartz) · OQ-11b polling cadence + public webhook callback hosting · OQ-13 alert channel (with alerts, post-v1) · OQ-16 cost rollup rules (post-v1) · OQ-17 which rides count toward wear (trainer / virtual / manual / flagged) · OQ-18 attachments/documents (receipts/photos) · OQ-19 bike health/condition score.
*(Resolved: OQ-1 vision, OQ-2 D-2, OQ-4/OQ-12 D-3, OQ-5 Manual installation, OQ-6 D-6, OQ-7/OQ-9 D-4/D-5, OQ-14 scope, OQ-15 D-5 cascade.)*

## 12. Phasing (rough → becomes `.ai/plans/`)
**v1:**
- **Phase 0** — Domain skeleton + Bikes CRUD (manual) + garage view + lifecycle (archive/sell).
- **Phase 1** — Strava integration: import gears, activity sync via **periodic polling + recalculation**, bike mileage. (Webhooks explicitly out — see below.)
- **Phase 2** — Components + installations (parent = bike/component incl. meta-components, multi-bike, positions, dateless historical) + warehouses.
- **Phase 3** — Projections/jobs for component mileage/hours/elevation (chain attribution).

**Post-v1:**
- **Phase 4** — Service, purchase & resale cost analysis.
- **Phase 5** — Alerts (channel TBD — OQ-13).
- **Phase 6 (optimization)** — Strava webhooks: push + inbox + async worker for lower-latency recalcs. No new logic — same command path as Phase 1's polling; only freshness improves.

## 13. Parked ideas (competitor scan — post-v1 candidates)
Noted from a scan of the existing tools; not committed, just captured so we don't forget:
- **Bike logbook / timeline** — a per-bike chronological history (installs, swaps, removals, services, frame changes, sale) **derived** from the data (D-14), fully editable, always consistent after corrections. (The competitor has a logbook but it drifts on edits — ours must not.)
- **Activities view + manual re-sync** — a UI list of the athlete's synced Strava activities with a button to **manually trigger a re-fetch/re-sync** of one (or all). A safety valve for when a webhook/poll was missed (seen on the competitor: webhook didn't fire → clicked → it refreshed). Reuses the same sync command path.
- **Attachments / documents** — receipts, manuals, warranties, photos on a bike/component/service. *(OQ-18)*
- **Bike health / condition score** — an aggregate wear/maintenance indicator per bike. *(OQ-19)*
- **Richer alert rules** (Phase 5) — thresholds by distance / hours / calendar time / lifespan; one-off vs recurring; **reset on service** ("usage since last service"); dedicated **chain-wax cadence**.
- **Notification channels** — email (reuse) and/or PWA push, in-app. *(OQ-13)*
- **Import UX** — paste-a-list / one-by-one (AI-assisted import intentionally out of scope).
- **Sharing / public build showcase** — out of scope (private, few friends); noted only.

## 14. Appendix — seed enums (code-side, D-13; persisted as strings)

### Bike types
Road · Mountain · Gravel · Urban · Triathlon · Cyclocross · Hybrid · Indoor · Commuter · E-Bike · Time Trial · Touring · BMX · **Other**

### Component categories (grouped for UI; front/rear collapsed — except derailleurs)
- **Brakes:** Brake · Brake Caliper · Brake Lever · Brake Pads · Brake Rotor
- **Drivetrain:** Chain · Cassette · Chainring · Crankset · Front Derailleur · Rear Derailleur · Shifter · Pulley · Chainguide · Sprocket
- **Cockpit:** Handlebar · Bar Tape · Grips · Stem
- **Wheels:** Wheel · Tire · Tire Insert · Hub · Spokes · Rim Tape · Inner Tube · Tubeless Sealant · Thru Axle
- **Structure:** Frame · Fork · Headset · Bottom Bracket · Bearing · Bolts · Pedals · Saddle · Seatpost
- **Suspension:** Suspension Fork · Rear Shock · Dropper Seatpost · Suspension Seatpost
- **Cables:** Cable · Hydraulic Lines
- **Electric:** Battery · Motor
- **Indoor:** Trainer · Indoor Bike · Fan · Mat · Riser
- **Accessories:** Computer · Lights · Lock · Pump · Rack · Bottle · Bell/Horn · Fenders · Kickstand · Toolset · Apparel · Accessories · **Other**

Notes: **Frame** is an **optional** category — the **Bike** is the durable *named* identity, and a Frame component lets you track **frame swaps** under it (real case: a cracked / warranty frame replaced while keeping the same named bike). Define it if you want that history; not required. Non-installable categories (Toolset, Apparel, Accessories, Other, consumables) are cost-only via the registry's `installable=false`. Lists are code-side, editable. **Both `Bike.Type` and `Component.Category` always include a mandatory `Other` fallback** (used whenever nothing fits — e.g. an unusual bike or a frame/part not in the list), so nothing is ever un-categorizable.

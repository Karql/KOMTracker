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
- Id (Strava activity id), `athlete_id`, **`gear_id`** (string, nullable/clearable "none" — **no gear ⇒ unattributed**, skipped), **dates** — persist `start_date` as the canonical **UTC** instant + **`utc_offset`** (seconds, DST-correct) + `timezone` string; local = `start_date + utc_offset` (**do NOT trust `start_date_local`** — Strava tags it with a bogus `Z`, D-15), **SportType** (`sport_type`, e.g. MountainBikeRide / GravelRide / Ride / EBikeRide / VirtualRide; `type` deprecated), Distance, MovingTime (`moving_time`), ElapsedTime (`elapsed_time`), Elevation (`total_elevation_gain`).
- Flags: `trainer`, `commute`, `manual`, `private`, `flagged`.
- **Persisted 1:1 with the API** (D-10) — the list above is illustrative, not exhaustive; we store *all* `DetailedActivity`/`SummaryActivity` fields too (power `average_watts`/`weighted_average_watts`/`max_watts`/`device_watts`, `kilojoules`, HR, speed, calories, elev_high/low, map/polyline, device_name, …). **All activities are synced**, including those without a `gear_id`.
- Attribution/MVP *uses* **distance / moving time / elevation** (D-6); everything else is stored for future features. *(OQ-17: which rides count toward wear — `trainer` / virtual / `manual` / `flagged`?)*

### Webhook event (`strava.webhook_event`) — inbox
Raw Strava webhook notification, persisted on receipt for async processing (§6). Fields: raw payload, `object_type` (activity|athlete), `object_id`, `aspect_type` (create|update|delete), `owner_id`, `subscription_id`, `event_time`, `received_at`, `processed`, `attempts`, `error`.

### Strava sync state (`strava.athlete_sync`)
Whether an athlete's Strava **activities are being synced** — a **generic `strava.*` capability** (not Bike-specific: synced activities may feed other features later), which **BikeTracker only *toggles*** (activation, D-16). **1 row per athlete with sync on.** Fields: `AthleteId` (key; joins to `token` + `strava.activity.athlete_id`), `Enabled` (the sync-job gate), `ActivatedAt`; optional `LastSyncAt`/`LastStatus` for telemetry only. **No incremental cursor** — sync is window-based (§6), so nothing needs persisting between runs.

### Warehouse / Store
Fields: Name. Holds non-installed components. (Retirement is a lifecycle status, not a warehouse — see below.)

### Service
Fields: Bike or Component, Date, Cost, Place (autocomplete). **Targets exactly one bike/component; NOT propagated along the install chain** (servicing a wheel doesn't touch the tyre that was in it — log a separate service on the child if needed) → no temporal "what was mounted then" resolution. (D-17) *(OQ-8: description/type, labour vs parts?)*

### Projection
Precomputed totals per bike/component (mileage/hours/elevation), maintained by jobs so the UI never groups+sums on the fly. *(OQ-10: columns on base entities vs separate projection tables.)*

### Lifecycle & sale (Bike + Component)
- **Status: Active → Archived → Sold.** *Archive* is a user action that hides the item from the main view but you still own it (like archiving an email; the warehouse acts as a label). *Sold* means you no longer own it and records **SaleDate + SalePrice**, which feeds cost analysis (e.g. wheels ridden 1 year, sold for 2k). Both Archived and Sold drop out of the default "active garage" view. (D-5)
- **Selling a bike cascades "Sold" to the components still installed on it** — to keep a component, detach it before selling. (OQ-15 resolved; part of D-5.)
- **Grouped components — lifecycle & delete (D-18):**
  - **Sold** parent → cascades Sold to still-installed children (whole thing gone; detach first to keep).
  - **Archive/Retire** parent → **detaches** its still-open children (closes their install windows → warehouse); does **not** cascade — children keep their own lifecycle and their **past installation records stay** (history preserved). Optional prompt: move detached children onto the bike the parent was on.
  - **Delete:** prefer Archive. **Hard-delete only when the component has no installation history**; with history → block (or hard-warn it erases that history). We **never** auto-rewrite children's past installations onto the bike (the wheel-on-two-bikes → split case is a trap). History survives via archive, not delete.

## 5. Use cases (grouped)
- **Garage:** add/edit/archive/sell a bike; view active garage; view a bike's components & totals.
- **Components:** add a component; set category; assign to a warehouse; archive/sell; record a dateless historical usage.
- **Installations:** install a component on a bike or into a parent component (with position + date); move it (close + open); install one component on several bikes at once.
- **Strava:** import gears as bikes; auto-sync activities; attribute mileage to bikes → components.
- **Service & cost:** log a service with cost; see cost analysis per bike/component (incl. resale).
- **Alerts (later):** set a threshold (km/hours/period) on a component/bike; get notified when due.

## 6. Key flows
### Strava import
> Real API payloads, pagination (`per_page` 200, loop-until-empty, `after` for incremental) and rate-limit headers are documented in **`docs/strava-api-notes.md`** (global reality reference — Strava notes apply to KOM too; trusted over the spec).

- Import **gears** (bikes) from `GET /athlete` (`bikes[]`) + `GET /gear/{id}` → create a `bt.bike` + a `bt.bike_link` (ExternalId = gear id). Map `brand_name` / `model_name` → Bike (Brand); `frame_type` is an **integer** → small int→`Bike.Type` map (Phase-1 detail); seed Initial mileage from the gear's `distance` (**float, metres**); `primary` marks the main bike; gear `id` is a **string**. (Strava gears cover bikes + shoes — we import bikes only.)
- **Activity sync:** baseline is **periodic polling + recalculation**, **two-tier, no stored cursor** — a **full sync** on a slow cadence (e.g. weekly) that pages *all* activities to catch **edits/deletes of old rides** (and any missed events), plus a **recent-window sync** frequently (e.g. daily, `?after=now-7d`) for freshness. Windows are derived from `now`, so nothing is persisted between runs. **All activities** go into `strava.activity` (1:1, regardless of `gear_id`/sport, D-10); attribution to a bike happens later via `gear_id` ↔ `bt.bike_link`. Webhooks (below) are a **later** latency enhancement, not the correctness path.

### Strava consent & opt-in activation
- **Manual-first.** Bikes / components / installations work **without Strava**; Strava is only the **auto-mileage** layer. Adding a bike by hand needs no Strava at all.
- **Scopes.** Login baseline (KOM) = `read, activity:read, profile:read_all` (minimal). Accurate mileage needs **`activity:read_all`** — over `activity:read` it adds private + "Only You" activities **and privacy-zone data** (full routes). So the BikeTracker-sync set is `read, activity:read_all, profile:read_all` (we do **not** need `read_all`, which only covers private *segments/routes*). Strava lets the user **uncheck any requested scope** and returns the *actually granted* scope (in the redirect `scope` param + the token-exchange response) → we store it in **`token.Scope`** and **always validate that** (never assume the grant matched the request).
- **Activation = an explicit action** ("**Sync bikes from Strava**" on a BikeTracker page) that doubles as the **per-athlete opt-in** (so we never auto-import for everyone):
  1. Check `token.Scope` for `activity:read_all`.
  2. **Present** → import gears (`bt.bike` + `bt.bike_link`), **upsert `strava.athlete_sync` (`Enabled=true`)**, run the initial **full backfill** (all activities, paged), enable polling.
  3. **Missing** → prompt to *reconnect for broader access*; re-run Strava authorize with the **union** `read,activity:read_all,profile:read_all` + `approval_prompt=force` (reuse the `LoginEndpoint → /account/connect → ConnectCommand` bridge, parameterized by scope); Strava overwrites `token.Scope` on the new exchange → then do step 2. **Partial grant:** if the user reconnects but still unchecks `activity:read_all`, sync stays **off** (we just re-offer). Request the **union** so KOM's scopes are kept (edge: unchecking a KOM scope here would reduce KOM access).
- **Job gating.** The polling/sync job processes **only athletes with Strava-sync = on** — the concrete realization of the earlier deferred "enrollment / don't mass-import". Manual-only Bike users are never polled.
- **Shared-token caveat.** One Strava app + one token per athlete → upgrading to `activity:read_all` also upgrades KOM's token (harmless; `read_all ⊇ read`). We ask for more **only** on Bike opt-in → minimal-by-default, escalate-on-demand.
- Optional later: a "Disconnect sync" (stop polling, keep data).

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
- Scopes: activity webhooks require **≥ `activity:read`** (satisfied by our Bike-sync `activity:read_all`). *(OQ-11a: job runner — Hangfire (persistent queue + dashboard + retries; new dep) vs the existing **Quartz** draining the inbox. OQ-11b: polling cadence + webhook callback hosting/public URL.)*

### Mileage attribution (the crux — next brainstorm round)
- **Bike total** = initial + Σ its activities.
- **Component total** = initial + Σ Manual-installation baselines + Σ activities reaching it through the **installation chain** (Tracked windows only) during overlapping windows (a tyre gets the km its wheel got while that wheel was on a bike, during the tyre-in-wheel window); summed across concurrent bike installations. Tracked for **distance / moving time / elevation** (D-6).
- The linkage invariant (D-7) keeps the chain unambiguous (no mixed parents) → no double counting.
- Only activities **with a `gear_id`** are attributed; ones without are skipped **at attribution time** (they're still synced + stored). Window overlap is computed on the activity's **UTC `start_date`**.
- Computed (Tracked) totals **recomputed from source** (activities × installation windows), never adjusted incrementally — recompute runs on activity sync / installation change. So **editing or correcting an installation** (e.g. a wrong year) always yields correct totals; no drift. (D-14; storage TBD OQ-10.) Manual totals are static on the installation.

## 7. Rules & alerts (later phase)
An alert **rule** attaches to **one bike or component** (no propagation — D-17). Threshold on **distance / moving-time hours / calendar time / lifespan**; **due = usage-or-time since the last *reset* ≥ threshold**. A **reset** is logging the maintenance was done (a Service, or a lightweight "mark done") → the interval **restarts** (recurring rules auto-restart on each reset; one-off rules fire once). Crucially, a reset is a **timestamp marker** and "since reset" is **derived from source** — no stored counter, so it stays correct after history edits (D-14). E.g. wheel: *"tubeless sealant every 6 months"* → refill = reset → counts from zero again; replacing a tyre is a separate swap, and if you refreshed sealant then you reset the wheel's rule. (D-19) *(OQ-13: notification channel — email / PWA / in-app.)*

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
- **D-16 Strava sync is explicit opt-in + scope-gated.** Bikes are manual-first; the "Sync from Strava" action (a) ensures the token has **`activity:read_all`** — reconnecting for it (union re-auth, `approval_prompt=force`) if `token.Scope` lacks it — and (b) upserts **`strava.athlete_sync`** (`Enabled=true`) — the gate for the sync job. The sync state lives in **`strava.*`** (generic Strava capability); Bike only toggles it. Keeps default scopes minimal (escalate only on opt-in) and prevents mass activity import for existing users. Shared token means the upgrade also benefits KOM (harmless, `read_all ⊇ read`). This is the concrete form of the earlier deferred enrollment/job-scoping.
- **D-15 UTC everywhere; Strava date quirks handled explicitly.** Store all timestamps as **`timestamp with time zone` (timestamptz)** with `DateTime.Kind=Utc` / `DateTime.UtcNow` (Docker = UTC; Npgsql modern mode — verified: prod uses `UtcNow`, no `EnableLegacyTimestampBehavior`). For Strava activities persist the canonical **`start_date` (UTC)** + **`utc_offset` (seconds, DST-correct)** + `timezone`; compute local as `start_date + utc_offset`. **Never treat `start_date_local` as an instant** — Strava appends a bogus `Z` to a local wall-clock (would be off by the offset). Time-ordering + installation-window overlap use the UTC instant; **per-day bucketing / matching a local install date uses the *local* date** (`start_date + utc_offset`) so a late-evening ride lands on the right day. New `bt.*`/`strava.*` datetime columns are timestamptz (legacy `token.expires_at` is `timestamp without time zone` — left as-is, KOM only). *(Phase 1: add a full Activity model to `Strava.API.Client` — only `ActivityMetaModel` exists today — incl. start_date/start_date_local/timezone/utc_offset/gear_id/…. **Gotcha: `utc_offset` appears only in the spec's `example:` blocks, not in the `SummaryActivity`/`DetailedActivity` schema** — so don't trust codegen from the OpenAPI models; add `utc_offset` (and any other real-but-unspecced fields) by hand from actual responses.)*
- **D-14 Projections recompute from source; history is fully editable** — totals are always derived from the source of truth (activities × installation windows + manual baselines), recomputed idempotently, **never** mutated by increments. Installations and history (dates, position, parent, lifecycle) are **freely editable/correctable**, and any edit re-derives affected totals + the bike logbook — so a fixed date is actually fixed (no stale/forward drift). *(Deliberate contrast with a competitor where editing a wrong install date left the total wrong.)*
- **D-13 Types as code-side enums, not lookup tables** — `Bike.Type` and `Component.Category` are C# enums persisted **as strings** (EF `HasConversion<string>`); no `bike_type`/`component_type` tables. Per-type metadata (display name, UI **group**, `installable`?, default wear metric) lives in a code registry/helper. Consistent with the existing `ActivityTypeConsts` / `ExtendedCategoryEnum` pattern; simplest at hobby scale; readable DB values; `Other` is the escape hatch. Front/rear/left/right is **not** a category — it's `Installation.Position`. Revisit a lookup table only if user-defined categories are ever needed.
- **D-12 Webhooks: inbox + async, never inline — and deferred.** Baseline sync is periodic **polling + recalculation**; webhooks are a **later** latency optimization (same commands/attribution, just sooner). When built: the callback only validates + writes a `strava.webhook_event` (`processed=false`) and returns 200 within 2 s (Strava retries ≤3×, multiple events per save). A background worker drains the inbox into idempotent commands (sync/upsert/delete activity; handle deauthorization), re-fetching current state from the API rather than trusting event order. One subscription per app, **registered once by hand** (curl/Postman) — the app builds only the callback + worker, not subscription management. A polling job backstops missed events. Runner (Hangfire vs existing Quartz) is OQ-11a.
- **D-11 `bt.*` owned by the User (app account)** for visibility + edit-security. *Known debt (deferred):* today `UserEntity` carries a Strava `athlete_id` (the relationship is backwards); the app account should be the root with linked external identities (Strava now, Garmin later). Not fixing now — but new `bt.*` ownership targets the User so it's future-proof.
- **D-3 Grouping via Installation (parent = Bike or Component)** — one mechanism for both; recursive-capable but shallow in practice; keeps the position binding and handles moving a component between parents cleanly. Unlike the rigid "the group binds all its children, can't pull a child out without dismounting the group" approach, our **independent installations** let you move a child (tyre) between parents (wheels) freely, and a parent off the bike auto-stops its children accruing (chain traversal). Consistency comes from recompute-from-source (D-14) + the linkage invariant (D-7), not from locking the user.
- **D-4 Accessories/tools/consumables = component categories** (never installed) — avoids a parallel entity; still cost-tracked.
- **D-5 Lifecycle Active/Archived/Sold + resale** — archive hides from main view (still owned); sold records SaleDate/SalePrice for cost analysis; applies to Bike and Component. **Selling a bike cascades Sold to its still-installed components** (detach first to keep them).
- **D-6 Track three wear metrics** — distance, moving time, elevation totals per bike/component; alerts pick whichever fits (chain→km, suspension→hours).
- **D-7 Component-linkage invariant** — a component's *active* installations are homogeneous: either into **exactly one** parent component, **or** onto **one-or-more bikes** — never mixed, never two parent components at once, and **no self/cyclic installation** (a component can't end up its own ancestor). `Installation.Type = Manual` (dateless, static totals) is always historical, never active.
- **D-17 Events/services never propagate along the install chain** — a service (and any component event/cost) attaches to exactly one bike/component. No auto-linking to whatever was mounted at the time → avoids temporal cross-resolution; log a separate entry on the child if you serviced it too.
- **D-19 Alerts: per-entity, reset-based, derived** (Phase 5) — a rule lives on one bike/component (no propagation, D-17); "due" = distance / hours / calendar time / lifespan **since the last reset** ≥ threshold; a **reset** is a timestamp marker (logged when maintenance is done) and "since reset" is **derived from source** (no stored counters, D-14); recurring rules auto-restart on reset (e.g. tubeless sealant every 6 months). Channel is OQ-13.
- **D-18 Parent lifecycle & delete are non-destructive to child history** — **Sold** parent cascades Sold to still-installed children (D-5); **Archive/Retire** parent **detaches** open children (closes their windows → warehouse) and keeps their past installation records; **hard-delete only when a component has no installation history** (else archive) — we never auto-rewrite children's past installations onto a bike (the wheel-on-two-bikes → split case is a trap). Tames the grouped-component edge cases by simplifying, not by heavy validation.
- **D-8 Strava's public API (v3) exposes only bikes + activities** — no components at all, and even bike `weight` is web-only (`DetailedGear` = brand/model/frame_type/description; the `weight` in the spec is the *athlete's*). Strava's newer **web** UI added a *basic* components feature, but it's **per-bike, non-movable, and not in the API** — so it validates our differentiators (moving components between bikes, meta-components) rather than threatening them. Import stays **bikes + activities**; the component / installation / mileage-attribution layer is fully ours. We do **not** replicate Strava's auto-generated "Frame" component — our Bike is that top level.

## 11. Open questions
OQ-3 shared base for Bike/Component (defer) · OQ-8 service fields (labour/parts, post-v1) · OQ-10 computed-projection storage (columns vs tables) · OQ-11a webhook job runner (Hangfire vs existing Quartz) · OQ-11b polling cadence + public webhook callback hosting · OQ-13 alert channel (with alerts, post-v1) · OQ-16 cost rollup rules (post-v1) · OQ-17 which rides count toward wear (trainer / virtual / manual / flagged) · OQ-18 attachments/documents (receipts/photos) · OQ-19 bike health/condition score.
*(Resolved: OQ-1 vision, OQ-2 D-2, OQ-4/OQ-12 D-3, OQ-5 Manual installation, OQ-6 D-6, OQ-7/OQ-9 D-4/D-5, OQ-14 scope, OQ-15 D-5 cascade.)*

## 12. Phasing (rough → becomes `.ai/plans/`)
**v1:**
- **Phase 0** — Domain skeleton + Bikes CRUD (manual) + garage view + lifecycle (archive/sell).
- **Phase 1** — Strava integration: **opt-in activation** ("Sync from Strava" + `activity:read_all` scope check/reconnect, D-16), import gears, activity sync via **periodic polling + recalculation** (gated on Strava-sync = on), bike mileage. (Webhooks explicitly out — see below.)
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

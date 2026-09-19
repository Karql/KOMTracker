# BikeTracker — Phase 3: Component mileage (stored projection + recompute)

## Context
Installations (2b-i/2b-ii) place a component on a bike, in a wheel-on-a-bike, or across several bikes — but the detail still says *"Ride mileage from installed windows — coming in a later phase."* This phase computes each component's **distance / moving-time / elevation** from the Strava rides that reached it through its installation chain. Bikes compute mileage on-the-fly (one indexed `GROUP BY`, `BikeTotalsCalculator`), but component mileage must intersect **installation date-windows** with activities — per-component on the list over ~8000 activities is far too slow. So component totals are a **stored projection** (`bt.component_mileage`, the repo's first) recomputed by a command that fires on every relevant change and after each activity sync. Per-installation window mileage stays cheap and is computed live on detail pages.

## Decisions (rationale)
- **D-3-1 Stored projection `bt.component_mileage` (1:1 with component).** Columns: `component_id` (PK == FK, `ValueGeneratedNever`, **ON DELETE CASCADE**), the four totals, `computed_at` + audit. **No `user_id`** (row is 1:1 with the component; reads start from the user's components and batch-load by id). List/detail **read** it — no activity scan on read. *Why: on-the-fly over 8000 activities × N components kills the list; store the expensive aggregate (CONCEPT §4 Projection). First stored projection in the repo; bikes stay on-the-fly.*
- **D-3-2 Attribution + recompute-from-source.** `total = Initial seed + Σ Manual baselines + Σ windowed activity aggregates over the component's effective Tracked windows`. Effective window = `(bike gear-ids, [from, to))`: on a bike → `[DateFrom, DateTo)`; in a parent component → overlap of `[DateFrom, DateTo)` with each of the parent's Tracked on-bike windows. One-level ⇒ ≤2 hops. Half-open on UTC `StartDate`; `DateTo == null` ⇒ open. Always recomputed from source (no drift on date edits). *Why: CONCEPT §6 — a tyre gets the km its wheel got while on a bike, during the tyre-in-wheel window; summed across concurrent bikes.*
- **D-3-3 Recompute triggers — inline, narrowly targeted, deletion-robust.** Recompute reads ALL a component's Tracked windows (`GetByComponentAsync`, current + historical) → historical counts, self-corrects on add/remove. Component mutation (`SaveComponent`, all Installation commands, `ChangeComponentLifecycle`) → affected component + active children. Activity sync → components on the affected athlete's bikes (current + historical installs): `SyncActivitiesCommand` athlete→user (`GetUserIdByAthleteIdAsync`)→`GetBikesAsync(includeInactive)`→`GetByBikeAsync`→component ids; `SyncActivityCommand` gear→`GetByExternalIdAsync`→bike→`GetByBikeAsync`. All end at one `RecalculateComponentsMileageCommand { ComponentIds }` (handler expands active children). *Why: edit-narrow but sync-broad was inconsistent; targeting the athlete's bikes (not gear still seen) means a deleted last ride still recomputes and drops stale km.* Note: daily recent sync delete-detects only its 7-day window; older deletions land on the weekly full sync (same as bikes).
- **D-3-4 Count all rides** (incl. trainer/virtual), like bikes; OQ-17 deferred.
- **D-3-5 Per-installation window mileage computed live (not stored).** Detail installation tables + Contains show each Tracked row's window contribution (few windows → few aggregates); the list uses only the stored total. *Why: cheap per page; storing per-installation multiplies rows + recompute cost.*
- **D-3-6 Reuse.** `GetGearTotalsInWindowAsync(gearIds, from, to?)` = `GetGearTotalsAsync` + `StartDate` bounds. `ComponentMileageService` builds effective windows (pure overlap) + sums; totals shape mirrors `BikeTotals`. Projection upsert clones the 1:1 FlexLabs `Upsert().WhenMatched().RunAsync()` used for `athlete_sync`.
- **D-3-7 Recompute triggered by domain events, not a command sent from handlers (revision of D-3-3's wiring).** The mutation/sync handlers `Publish` MediatR `INotification`s — `ComponentChanged`/`InstallationChanged` (`Notifications/Component`) and `AthleteActivitiesSynced`/`ActivitySynced` (`Notifications/Strava`) — and one reactor, `ComponentMileageProjectionUpdater`, resolves the affected component ids (athlete→bikes→components, gear→bike→components) and dispatches the canonical `RecalculateComponentsMileageCommand`, which owns child-expansion + recompute + upsert and is shared with the admin backfill endpoint. *Why: the original wiring had every **command** handler `_mediator.Send` the recompute command — command-calls-command, a recognized MediatR smell that hides the dependency and misuses request-semantics for a side effect. Moving the triggers to events (the repo's existing pattern, `TrackKomsCompletedNotification`) fixes it AND reduces the commands' responsibility: they announce their own domain fact with zero mileage knowledge. A **notification handler → command** is the blessed side-effect shape (cf. `TrackKoms` → `RefreshStats`/`DetectKomTakeovers`, each also admin-recoverable), so the updater dispatches the command rather than calling a service — keeping one canonical recompute op behind both the event trigger and the admin endpoint. Phase 5 wear-alerts can subscribe to the same facts without touching any command. Best-effort (log + swallow) like the sibling notification handlers — the projection is always rebuildable via the admin endpoint.*
- **D-3-8 `SyncBikesCommand` batch drives a service, not a sub-command.** The per-athlete bike sync was extracted from `SyncStravaBikesCommandHandler` into `IStravaBikeSyncService.SyncAthleteBikesAsync(athleteId) → Result`; the command handler is now a thin delegate (still the single-athlete entry point for controllers), and the `SyncBikesCommand` batch loop calls the service directly. *Why: the same command-calls-command smell as the mileage triggers, but it needs the sub-op's `Result` to abort the run on a rate-limit — so a notification (`Publish`, returns void) can't model it; the clean fix is a shared service both the item-command and the batch depend on. Kept the command for its independent callers (controllers, `ActivateStravaSyncCommand`).*

## Checklist
### Domain
- New `Entities/Component/ComponentMileageEntity.cs : BaseEntity` — `ComponentId` PK, four totals, `ComputedAt`.
- `ComponentEntity` — `[NotMapped]` `TotalDistanceKm/TotalMovingHours/TotalElevationM/AttributedActivityCount`.
- `InstallationEntity` — `[NotMapped]` `WindowDistanceKm/WindowMovingHours/WindowElevationM/WindowActivityCount`.

### Infrastructure
- New `ComponentMileageEntityTypeConfiguration` (`ToTable("component_mileage","bt")`, `PrepareBaseColumns`, `HasKey(ComponentId)`+`ValueGeneratedNever`, FK→component **Cascade**).
- `KOMDBContext` DbSet + ApplyConfiguration; migration `AddComponentMileageTable` + update.
- New `IComponentMileageRepository`/`EFComponentMileageRepository` (`UpsertAsync` FlexLabs, `GetByComponentIdsAsync`, `GetAsync`) + DI.
- `IActivityRepository`/`EFActivityRepository` + `GetGearTotalsInWindowAsync(gearIds, from, to?)`.

### Application
- New `Services/ComponentMileageService` — effective `AttributionWindow`s (bike gear-ids via `IBikeLinkRepository` Strava filter; parent on-bike windows via `GetByComponentAsync`; pure overlap) + sums via `GetGearTotalsInWindowAsync`; component total + per-window totals; conversions from `BikeTotalsCalculator`.
- New `Commands/Component/RecalculateComponentsMileageCommand { ComponentIds }` — expand active children, dedup, compute + upsert per component; skip missing/other-user.
- Trigger sends after `SaveChangesAsync` in `SaveComponent`, `InstallComponent`, `Move/Remove/Update/DeleteInstallation`, `ChangeComponentLifecycle`; `SyncActivitiesCommand` (athlete→user→bikes→components), `SyncActivityCommand` (gear→bike→components).
- New `IUserService.GetUserIdByAthleteIdAsync(int)` + `UserService` impl (expose existing `AthleteId→Id`).
- Reads: `GetComponentQuery`/`GetComponentsQuery` batch-load projections → `Total*`; `GetComponentInstallationsQuery`/`GetBikeInstallationsQuery`/`GetComponentQuery.Children` set `Window*` via the service.

### API
- `ComponentViewModel` (+mapping) `Total*`; `InstallationViewModel` (+mapping) `Window*`.
- `AdminController` `PUT admin/recalculate-component-mileage` — body = list of component ids → `RecalculateComponentsMileageCommand`.

### WEB
- `ComponentDetails.razor` Mileage panel (mirror BikeDetails) + per-window cell in installations + Contains tables.
- `Components.razor` distance on card + table.
- `BikeDetails.razor` per-window cell in installed table.

### Tests + Docs
- `ComponentMileageServiceTests` (pure): direct window, outside-window excluded, open window, parent overlap, multi-bike sum, seed+manual.
- `RecalculateComponentsMileageCommand` handler test (upsert totals + children expansion); a trigger test; `GetComponentsQuery` reads projection.
- CHANGELOG; `.ai/README.md`.

## Verification
- Build sln; migration add + update; `dotnet test` green.
- Backfill test components: `PUT admin/recalculate-component-mileage` with ids in body.
- Manual: install chain over past date → total = bike rides since install; edit date → recompute; tyre-in-wheel-on-bike → overlap; computer on 2 bikes → sum; Manual adds; historical window still counts; delete component → projection gone; delete a ride → total drops after full sync.

## Out of scope
- Alerts (5); cost (4); webhooks (6); OQ-17 ride filtering; storing per-installation window mileage.

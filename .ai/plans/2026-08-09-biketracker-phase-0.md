# BikeTracker — Phase 0: Bikes CRUD + Garage + Lifecycle

**Status:** Done — build + all tests green (165), migration applied to dev DB. Includes review-driven revisions (owner = User; no AutoMapper; single flat `SaveBikeCommand`; semantic errors → 422/404/403/409 problem+json; string enums on the wire; per-page card/table view saved to localStorage). Only live UI smoke pending (needs the stack running).
**Date:** 2026-08-09
**Concept:** `docs/biketracker/CONCEPT.md` (Decisions D-1..D-19)

## Context
BikeTracker is a bike-maintenance journal added to the KOMTracker app as **folders inside the existing projects** (no new solution/projects, no new front-end), persisted in the existing `KOMDBContext` under a **new `bt` Postgres schema**, with **strong FKs** into the existing model (`athlete`).

Phase 0 is the foundation vertical: a user manages their **bikes** — add / edit / list (card view) / detail page / change lifecycle (Active → Archived / Sold) / delete. **No Strava, components, installations, or mileage** yet (Phase 1/2). Deliberately the tightest slice that establishes Domain → EF/migration → MediatR CQRS → API → Blazor, so later phases extend it.

## Checklist

### 1. Domain — `KomTracker.Domain/Entities/Bike/`
- [ ] `BikeType.cs` — enum (concept appendix): Road, Mountain, Gravel, Urban, Triathlon, Cyclocross, Hybrid, Indoor, Commuter, EBike, TimeTrial, Touring, BMX, Other. Stored by name (string).
- [ ] `BikeLifecycle.cs` — Active, Archived, Sold. Stored by name (string).
- [ ] `BikeEntity.cs : BaseEntity` — `int Id` (DB-generated), `string UserId` (FK→`AspNetUsers.Id`), `string Name`, `string? Brand`, `string? Model`, `BikeType Type`, `decimal? WeightKg`, `string? Notes`, `decimal? Price`, `string? PurchasePlace`, `DateTime? PurchaseDate`, `decimal InitialDistanceKm`, `decimal? InitialMovingHours`, `decimal? InitialElevationM`, `BikeLifecycle Lifecycle` (default Active), `DateTime? SaleDate`, `decimal? SalePrice`.

### 2. Infrastructure — persistence
- [ ] `Persistence/Configurations/Bike/BikeEntityTypeConfiguration.cs` — `ToTable("bike","bt")`, `PrepareBaseColumns()`, `HasKey(Id)` (generated, no `ValueGeneratedNever`), `HasOne<AthleteEntity>().WithMany().HasForeignKey(AthleteId)`, snake_case `HasColumnName`, `HasMaxLength` (name/brand/model/purchase_place 200, notes 2000), enum props `.HasConversion<string>().HasMaxLength(50)`, `HasIndex(AthleteId)`.
- [ ] `Persistence/KOMDBContext.cs` — `DbSet<BikeEntity> Bike` + `// BikeTracker` `ApplyConfiguration`.
- [ ] `Persistence/Repositories/EFBikeRepository.cs : EFBaseRepository, IBikeRepository` — `GetBikesAsync(athleteId, includeInactive)` (AsNoTracking), `GetBikeAsync(id)` (tracked), `AddBike`/`UpdateBike`/`DeleteBike`.
- [ ] `Persistence/PersistenceDependencyInjection.cs` — `AddScoped<IBikeRepository, EFBikeRepository>()`.
- [ ] Migration `AddBikeTable` (`--project KomTracker.Infrastructure --startup-project KomTracker.API`); verify `EnsureSchema("bt")` + `bt.bike` + all date cols `timestamptz`.

### 3. Validation + error infra (NEW, app-wide) — FluentValidation, Result-based, semantic errors
- [ ] Packages in `KomTracker.Application`: `FluentValidation` + `FluentValidation.DependencyInjectionExtensions`.
- [ ] `Application/Errors/` — `AppError : FluentResults.Error` (abstract); `ValidationError`(→422, carries field→messages), `NotFoundError`(→404), `ForbiddenError`(→403), `ConflictError`(→409, future). Existing `XxxError` untouched → default 400.
- [ ] `Application/Behaviors/ValidationBehavior.cs : IPipelineBehavior<TRequest,TResponse> where TResponse : ResultBase` — run validators; on failure return failed `TResponse` with a `ValidationError` (no throw); helper builds `Result`/`Result<T>` (reflection for closed `T`). Constraint skips queries.
- [ ] `Application/DependencyInjection.cs` — `cfg.AddOpenBehavior(typeof(ValidationBehavior<,>))` at the `// TODO: Behaviors` slot + `AddValidatorsFromAssembly`.
- [ ] Validators: `AddBikeCommandValidator`, `UpdateBikeCommandValidator`, `ChangeBikeLifecycleCommandValidator`.

### 4. Application — CQRS
- [ ] `Interfaces/Persistence/Repositories/IBikeRepository.cs`
- [ ] `Models/Bike/BikeModel.cs` (flat read model; Entity→Model mapped manually in handlers)
- [ ] `Queries/Bike/GetBikesQuery.cs` (`IEnumerable<BikeModel>`, `{AthleteId, IncludeInactive}`)
- [ ] `Queries/Bike/GetBikeQuery.cs` (`BikeModel?`, `{Id, AthleteId}`)
- [ ] `Commands/Bike/AddBikeCommand.cs` (`Result<BikeModel>`)
- [ ] `Commands/Bike/UpdateBikeCommand.cs` (`Result<BikeModel>`)
- [ ] `Commands/Bike/ChangeBikeLifecycleCommand.cs` (`Result`; guard Sold⇒SalePrice/SaleDate)
- [ ] `Commands/Bike/DeleteBikeCommand.cs` (`Result`)
- [ ] Handlers keep only existence/authz guards → `NotFoundError`/`ForbiddenError`; then `SaveChangesAsync`.

### 5. API
- [ ] `API.Shared/ViewModels/Bike/` — `BikeViewModel`, `SaveBikeViewModel`, `ChangeBikeLifecycleViewModel` (shared with WEB).
- [ ] `API/Controllers/BikesController.cs : BaseApiController<BikesController>` `[Route("bikes")] [BearerAuthorize]`, scoped by `GetCurrentUser().AthleteId`: GET list (`?include_inactive=`), GET `{id}`, POST, PUT `{id}`, PUT `{id}/lifecycle`, DELETE `{id}`.
- [ ] Result→IActionResult mapping helper: switch on error type → `ValidationError`=422 (`ValidationProblemDetails`, `application/problem+json`), `NotFoundError`=404, `ForbiddenError`=403, `ConflictError`=409, else 400; success → `Ok`/`NoContent`.
- [ ] `API/Mapings/DtoProfile.cs` — `CreateMap<BikeModel, BikeViewModel>()`.

### 6. WEB (`KomTracker.WEB`, MudBlazor 9.8.0)
- [ ] `Pages/Bikes.razor(.cs)` — `@page "/bikes"`, Card view default (`MudGrid`/`MudCard`) + Card⇄Table toggle (Table = compact `MudTable`), Add button + show-archived/sold filter, loading gate, breadcrumbs.
- [ ] `Pages/BikeDetails.razor(.cs)` — `@page "/bikes/{Id:int}"`, all fields grouped + actions Edit/Archive/Sell/Delete + "Components" placeholder.
- [ ] `Shared/AddEditBikeDialog.razor` — `MudDialog`+`MudForm`, progressive disclosure (core + collapsible "Additional details"), no lifecycle/sale fields.
- [ ] `Shared/SellBikeDialog.razor` — SaleDate + SalePrice → `PUT /bikes/{id}/lifecycle` Sold; Archive/reactivate direct; Delete confirm.
- [ ] API calls via `HttpClient` (JSON extensions); 422 → parse `ValidationProblemDetails`; errors via `ISnackbar`.
- [ ] `Shared/NavMenu.razor` — `MudNavGroup Title="Bike Tracker"` wrapping `Bikes` link.

### 7. Tests
- [ ] Validators (`TestValidate`): lifecycle Sold-without-price invalid; add empty-name/negative-weight invalid; happy paths valid.
- [ ] Handlers: missing bike ⇒ `NotFoundError`; other athlete's bike ⇒ `ForbiddenError`; happy paths.

### 8. Docs
- [ ] `CHANGELOG.md` `## UPCOMMING` entry.
- [ ] `.ai/README.md` BikeTracker section.

## Verification
1. `dotnet build src/KomTracker.sln` clean.
2. Migration inspected (`EnsureSchema("bt")`, `bt.bike`, `timestamptz` dates) + `dotnet ef database update`.
3. `dotnet test src/KomTracker.sln` green.
4. App: `/bikes` add (progressive form) → card → toggle table → detail → edit → archive → sell → delete; per-user scoping holds.
5. Invalid POST → 422 `application/problem+json`; unknown id → 404; existing KOM endpoints unaffected.

## Decisions
- **D-P0-1 Bike-only this phase.** *Why:* component/installation tables would be dead until Phase 2; a focused vertical de-risks the new `bt` schema + CQRS + UI plumbing first.
- **D-P0-2 Full field set on `bt.bike` now.** *Why:* the fields are cheap columns; adding them later means extra migrations and form churn.
- **D-P0-3 FluentValidation introduced app-wide, Result-based (no exceptions).** *Why:* the garage is the app's first real user input; the codebase already standardizes on FluentResults, so returning `Result.Fail(ValidationError)` keeps one error channel ("errors as values") instead of adding an exception path. Idiomatic Clean-Architecture; there was a reserved `// TODO: Behaviors` slot.
- **D-P0-4 Semantic error bases (`AppError`/Validation/NotFound/Forbidden/Conflict).** *Why:* the existing `XxxError`+const pattern is fine but has no HTTP-category signal; a thin additive layer lets the API map `Result`→status via one type switch and turns the ownership check into a `ForbiddenError` (vs a manual controller 403). Non-breaking (unknown types → 400).
- **D-P0-5 HTTP: validation→422, business→400, not-found→404, forbidden→403.** *Why:* 422 = well-formed but semantically invalid (correct for field validation); 400 stays for malformed/binding (ASP.NET default). ProblemDetails / `application/problem+json` (RFC 7807) so WEB gets structured field errors.
- **D-P0-6 `bt` schema (new convention).** *Why:* concept groups BikeTracker under `bt.*` (parallel `strava.*`/`kt.*`); keeps the new domain visibly separate in the shared DB. `ToTable("bike","bt")` + auto `EnsureSchema`.
- **D-P0-7 Enums in Domain, stored as string (`HasConversion<string>`).** *Why:* Domain can't depend on Application (so enums live in Domain); string storage is self-documenting in the DB, robust to member reordering, and removes the "pin Other" ordinal hack — storage overhead negligible at this scale. Contract: stored value = member name; rename ⇒ data migration.
- **D-P0-8 All dates `DateTime` UTC → `timestamptz`.** *Why:* consistency with every other date in the app, and full instants avoid boundary ambiguity when Phase-1 attribution compares sale/purchase against activity `start_date` (UTC). UI shows date only; normalize `MudDatePicker` values with `SpecifyKind(Utc)`.
- **D-P0-9 Owner = platform User, scoped by `UserId` from JWT `sub`.** *Why:* bikes belong to the **User** (identity, `AspNetUsers.Id`), not the Strava athlete — so ownership survives future integrations (Garmin, etc.); Strava's athlete-centric link is the historical exception. `bt.bike.user_id` → `AspNetUsers.Id` (string FK). The current user id comes from the JWT `sub` (→ `ClaimTypes.NameIdentifier`), exposed via `UserModel.UserId`; every query/command is scoped to it (no route param).
- **D-P0-12 API.Shared → Domain reference** so ViewModels are enum-typed and WEB gets a typed `MudSelect<BikeType>` (Domain is a dependency-free entity/enum lib).
- **D-P0-13 No AutoMapper for BikeTracker** — explicit `ToViewModel()` extension (compile-time safe; AutoMapper is reflection-based, now paid, and drifts silently on renames). Existing AutoMapper usage elsewhere is untouched.
- **D-P0-14 Single `SaveBikeCommand` (nullable `Id`)** for create+update; queries/commands return `BikeEntity` (no `BikeModel`), controller maps to `BikeViewModel`.
- **D-P0-16 Flat command + a request-body DTO in `API.Shared`.** `SaveBikeViewModel` (API.Shared) = the request body + WEB form (editable fields only; no server-owned fields → no over-posting). `SaveBikeCommand` (Application) carries the **same fields flat** + `Id?`/`UserId`; the controller maps `SaveBikeViewModel → SaveBikeCommand`. *Why not one shared type / command == body?* The WEB (Blazor WASM) references only the `*.Shared` projects, **not** `Application`, so it can't reference the command to post it; making the command the wire type would force `WEB → Application` (MediatR/FluentResults into the browser bundle). So body and command must be separate types. The field duplication + one explicit map is the cost; a **reflection parity test** (`BikeContractParityTests`) fails if `SaveBikeViewModel` and `SaveBikeCommand` fields drift, killing the silent-drift risk.
- **D-P0-17 All command validation runs in the MediatR pipeline.** `SaveBikeCommand` (flat) and `ChangeBikeLifecycleCommand` are validated by `ValidationBehavior` → validation is **guaranteed from any caller** (controller, job, another handler), not just the API, and error keys are the flat field names camelCased (`name`, `saleDate`) matching the posted JSON. (Rejected: validating at the API boundary — not guaranteed for non-API callers, and inconsistent across commands.) Both surface as `ValidationError` → 422 `application/problem+json`. Content-type is set automatically by the framework (ObjectResult value derives from ProblemDetails); we only set the status code + register 422 in `ClientErrorMapping` (for `type`). Enums are `[JsonConverter(typeof(JsonStringEnumConverter))]` → strings on the wire.
- **D-P0-15 ProblemDetails via the framework** — `AddProblemDetails()` + `ControllerBase.Problem()/ValidationProblem()` (no hand-set content-type).
- **D-P0-10 Hard delete allowed in Phase 0.** *Why:* no child history exists yet; D-18's archive-not-delete/guard-on-history rule kicks in from Phase 2 when installations exist.
- **D-P0-11 WEB naming "Bikes" + Card view default + basic detail page.** *Why:* "Bikes"/"Components" parallel naming reads clearer than "Garage"; few bikes suit cards; the detail page is the natural home for the full field set (cards stay lean) and the future components list. Grouped in a `MudNavGroup "Bike Tracker"`.

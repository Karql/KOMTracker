# Strava webhooks — step 1: inbox endpoint + persistence (D-12)

## Context
Today Strava data is pulled by polling jobs. Strava webhooks (https://developers.strava.com/docs/webhooks/) push activity/athlete create/update/delete events, enabling near-real-time recalcs. This is the **inbox** half of the already-documented **D-12** (CONCEPT): expose a public callback that **validates + persists each event raw** to `strava.webhook_event`, returning 200 fast — so we can watch what actually arrives before building any processing. **Event handling / a draining worker is explicitly a later step** (Phase 6). The subscription itself is created by hand (Postman); we build only the callback.

## Decisions
- **D-12-1 Inbox only, store raw.** The callback persists every event to `strava.webhook_event` and returns 200 immediately; no processing yet (a later worker drains it — the `processed` flag readies that).
- **D-12-2 Fields typed + `updates` as JSON.** Columns mirror the Strava payload — `object_type`, `object_id` (long), `aspect_type`, `owner_id` (long), `subscription_id` (int), `event_time` (long epoch) — plus `updates` stored as **jsonb** (the hash serialized verbatim; null for non-update events). `AuditCD` doubles as the received-at time. *Why: the 7 fields ARE the whole documented payload; a separate raw blob is redundant.*
- **D-12-3 No athlete FK.** `webhook_event` gets **no** `HasOne<AthleteEntity>` FK — `owner_id`/`object_id` are raw Strava ids that may not map to a tracked athlete.
- **D-12-4 Security = secret in the path + verify_token handshake.** The callback route carries a private `{secret}` (a GUID) checked on every GET/POST; the GET validation echoes `hub.challenge` only when `hub.verify_token` matches config. Both live under the existing **`StravaApiClientConfiguration`** section (`WebhookVerifyToken`, `WebhookSecret`) — no new config class. A third guard — matching `subscription_id` — is left as a code comment for the future (id known only after the subscription is created).
- **D-12-5 Unauthenticated controller.** The callback omits `[BearerAuthorize]` (auth is opt-in per controller; no global fallback policy); the `{secret}` is the gate.

## Implementation
### Domain / Infrastructure (persistence)
- **New** `Domain/Entities/Strava/WebhookEventEntity.cs : BaseEntity` — `Id` (int, generated), `ObjectType`, `ObjectId` (long), `AspectType`, `Updates` (string?), `OwnerId` (long), `SubscriptionId` (int), `EventTime` (long), `Processed` (bool).
- **New** `Infrastructure/Persistence/Configurations/Strava/WebhookEventEntityTypeConfiguration.cs` — `ToTable("webhook_event","strava")`, `PrepareBaseColumns()`, `HasKey(Id)` identity, snake_case columns, `Updates` `.HasColumnType("jsonb")` nullable, `ObjectType`/`AspectType` `HasMaxLength(50).IsRequired()`, `Processed` default `false`; **no** athlete FK; index on `Processed` + `EventTime`.
- `Persistence/KOMDBContext.cs` — `DbSet<WebhookEventEntity> WebhookEvent` + `ApplyConfiguration(...)` in the two `// Strava` groups.
- Migration `AddStravaWebhookEvent` + `database update`.
- **New** `IWebhookEventRepository` + `EFWebhookEventRepository` — `void Add(WebhookEventEntity)`; register `AddScoped` in `PersistenceDependencyInjection`.

### Application
- **New** `Commands/Strava/StoreStravaWebhookEventCommand.cs` `{ ObjectType, ObjectId, AspectType, Updates (string?), OwnerId, SubscriptionId, EventTime }` → handler maps to entity (`AuditCD = UtcNow`, `Processed = false`), `repo.Add`, `SaveChangesAsync`, `Result.Ok`. Permissive (store whatever arrives).

### API
- Extend `StravaApiClientConfiguration` (`src/Strava/Strava.API.Client/Configurations/StravaApiClientConfiguration.cs`) with `WebhookVerifyToken` + `WebhookSecret` — already bound + DI-registered via `AddStrava`, no new class/registration.
- **New** `ViewModels/Strava/StravaWebhookEventViewModel.cs` — `[JsonPropertyName("object_type")]` etc.; `updates` typed `JsonElement?`.
- **New** `Controllers/StravaWebhookController.cs : BaseApiController<...>`, `[Route("strava/callback/{secret}")] [ApiController]`, **no** `[BearerAuthorize]`:
  - `GET Validate` — secret mismatch ⇒ `NotFound()`; verify_token mismatch ⇒ `Forbid()`; else `Ok(new Dictionary<string,string>{ ["hub.challenge"] = challenge })`. Bind handshake with `[FromQuery(Name="hub.challenge")]` etc.
  - `POST Receive([FromRoute] secret, [FromBody] StravaWebhookEventViewModel)` — secret mismatch ⇒ `NotFound()`; serialize `updates` (`GetRawText()`, null when absent) → `StoreStravaWebhookEventCommand` → `Ok()`. `// TODO(D-12-4): also verify subscription_id once known.`
  - Config via `HttpContext.RequestServices.GetRequiredService<StravaApiClientConfiguration>()`.
- `appsettings.json` — add `WebhookVerifyToken` + `WebhookSecret` placeholders to the existing `StravaApiClientConfiguration`; real values in `appsettings.local.json` + prod.

### Docs
- `docs/biketracker/CONCEPT.md` — refine D-12 / §12: inbox implemented now; worker/processing + subscription-id guard + polling backstop deferred.
- `CHANGELOG.md`, `.ai/README.md`.

## Out of scope (later)
- Processing / draining worker / triggering `SyncActivityCommand`; subscription management (Postman); `subscription_id` validation (comment only); polling backstop.

## Verification
- `dotnet build src/KomTracker.sln`; migration add + update; `dotnet test` green (`StoreStravaWebhookEventCommand` handler test asserts the row is added).
- Manual (Postman): GET `…/strava/callback/{secret}?hub.mode=subscribe&hub.challenge=abc&hub.verify_token=<token>` → `200 {"hub.challenge":"abc"}`; wrong secret → 404; wrong token → 403. POST a sample event → `200` + row in `strava.webhook_event`. Register the real subscription via Postman (`callback_url` = deployed `…/kom-tracker-api/strava/callback/{secret}`, `verify_token` = config).

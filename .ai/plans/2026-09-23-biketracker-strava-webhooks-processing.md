# Strava webhooks — step 2: process the inbox (D-12 worker)

## Context
Step 1 collects raw Strava webhook events into `strava.webhook_event` (unprocessed). This step **acts on them**: activity create/update → re-sync the activity by id; delete → remove the local activity — recomputing mileage either way. Drained by an hourly Quartz job also triggered immediately on receipt. `updates` is ignored (unreliable). Athlete/other object types are no-op for now.

## Decisions
- **D-12-6 Drain via Quartz, triggered on receipt, coalesced.** Durable `ProcessStravaWebhookEventsJob` (`[DisallowConcurrentExecution]`, fixed `JobKey`), hourly cron `0 0 * * * ?` gated by `ApplicationConfiguration.ProcessWebhookEventsJobEnabled`, plus **unconditional** `ISchedulerFactory.TriggerJob(Key)` from the webhook POST (best-effort). Drains **all** unprocessed each run. No "skip if running" guard: with DisallowConcurrentExecution a trigger fired mid-run is BLOCKED, not dropped, and runs right after — so a mid-run event is handled seconds later, not at the next hourly tick. Hourly = backstop.
- **D-12-7 Only `object_type=activity`.** create/update → sync by `object_id`; delete → remove locally. Athlete events / unknown aspect → mark processed, no-op.
- **D-12-8 Gate on owner's activity sync.** `owner_id` == athlete id; skip (mark processed, no history) unless `IAthleteSyncRepository.GetAsync(owner)?.ActivitiesEnabled == true`.
- **D-12-9 Terminal vs transient.** Success / `NotFoundError` / skip / unknown → `processed=true`. Transient (no token, rate-limit, other error, exception) → leave unprocessed to retry next run (logged). (`attempts`/`error` deferred.)
- **D-12-10 Reuse single-activity sync via a service (no command→command).** Extract `IStravaActivitySyncService` from `SyncActivityCommand` (now a thin delegate, still used by Refresh); the drain calls the service. Mirrors `StravaBikeSyncService`.
- **D-12-11 History gets a type.** `strava.activity_sync_history` + `type` (`ActivitySyncType` Job|Webhook, string, default `Job`) + nullable `activity_id`; UI shows a **Type** column.
- **D-12-12 Filtered index** on `webhook_event.processed` `WHERE processed = false`.

## Implementation
### Persistence
- `Domain/Entities/Strava/ActivitySyncType.cs` (new enum `{ Job, Webhook }`, JsonStringEnumConverter).
- `ActivitySyncHistoryEntity` — add `ActivitySyncType Type`, `long? ActivityId`.
- `ActivitySyncHistoryEntityTypeConfiguration` — `type` (`HasConversion<string>().HasMaxLength(50).IsRequired().HasDefaultValue(nameof(ActivitySyncType.Job))`), `activity_id` nullable.
- `WebhookEventEntityTypeConfiguration` — `HasIndex(x => x.Processed).HasFilter("processed = false")`.
- `IWebhookEventRepository`/`EF` — `GetUnprocessedAsync()` (AsNoTracking, order event_time, id asc) + `MarkProcessedAsync(int id)` (ExecuteUpdate processed=true, audit_md=UtcNow).
- `IActivityRepository`/`EF` — `GetAsync(int athleteId, long activityId)` + `DeleteAsync(int athleteId, long activityId)` (ExecuteDeleteAsync).
- Migration `AddWebhookProcessingSupport` + update.

### Application
- `Services/IStravaActivitySyncService` + impl (register in Application DI): `SyncAthleteActivityAsync(athleteId, activityId)` (current SyncActivityCommand body) + `DeleteAthleteActivityAsync(athleteId, activityId)` (read gear → delete → publish `ActivitySyncedNotification`, delete-before-publish).
- `SyncActivityCommand` — delegate to `SyncAthleteActivityAsync`.
- `ProcessStravaWebhookEventsCommand` (new) — drain loop per D-12-7..9 (per-event try/catch; mark or leave; write Webhook history on terminal sync outcomes).
- `SyncActivitiesCommand.RecordHistoryAsync` — set `Type = Job`.
- `ApplicationConfiguration.ProcessWebhookEventsJobEnabled = true`.

### API
- `Infrastructure/Jobs/ProcessStravaWebhookEventsJob.cs` (`[DisallowConcurrentExecution]`, static `JobKey Key`) → sends the drain command.
- `Startup.cs` — AddTransient + `ScheduleJob<...>(t => hourly cron, j => j.WithIdentity(Key))` gated by the flag.
- `StravaWebhookController.Receive` — after persist, best-effort `TriggerJob(Key)` via `ISchedulerFactory` (try/catch; no skip-if-running guard).
- `ActivitySyncHistoryViewModel` + `StravaBikeMappings` — add `Type`, `ActivityId`.

### WEB
- `StravaSyncHistoryDialog.razor` — Window → **Type** column: `Job - Full` / `Job - From yyyy-MM-dd` / `Webhook - {ActivityId}`.

### Tests + Docs
- `ProcessStravaWebhookEventsCommandTests`, `StravaActivitySyncServiceTests`; update `SyncActivityCommandHandlerTests` (delegation) + `SyncActivitiesCommandHandlerTests` if needed.
- CONCEPT D-12/§12 (worker done), CHANGELOG, README.

## Out of scope
- Athlete events; `attempts`/`error` retry columns; acting on `updates`.

## Verification
- Build; migration; `dotnet test` green. Manual (Postman): create/update event (enabled athlete) → activity row + Webhook history + mileage; delete → row gone + mileage drops; disabled athlete → processed no-op; job flag off → hourly backstop still drains; history Type column renders.

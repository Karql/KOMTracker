# Graceful re-login when the session token can't be renewed

**Status:** Done
**Date:** 2026-07-12

## Goal

After the API restarts (e.g. a new deploy) a logged-in user who refreshes the page saw a broken page: token refresh returns `400 invalid_grant`, the API call then 401s, and the unhandled `HttpRequestException` crashed the component render. Make the app recover instead of crashing.

## Root cause (recorded for the future)

IdentityServer4 is registered with in-memory stores only (`AddInMemoryClients/ApiScopes/IdentityResources`, no `AddOperationalStore`) in `KomTracker.Infrastructure/Identity/IdentityDependencyInjection.cs`, so the **persisted-grant store holding refresh tokens is in memory and is wiped on every restart**. The login cookie survives (DataProtection keys persist to the mounted `/keys`) and so does the signing key file, so the IdP session is still valid — only the refresh token is gone.

**Durable alternative (not done):** persist grants with `AddOperationalStore(...)` (IdentityServer4.EntityFramework.Storage + Npgsql) + an EF migration for `PersistedGrants`/`DeviceCodes` (+ token cleanup). Optional hardening: persist the IS4 signing credential to `/keys` (today `AddDeveloperSigningCredential` regenerates on a fresh container). Deferred — the frontend recovery below already removes the crash and, because the IdP cookie survives, the re-login is effectively seamless.

## Decisions

- **Fix on the frontend (no DB change)** — the IdP cookie survives the restart, so re-triggering login is silent (no Strava prompt) and lands the user back logged in. Chosen over the operational-store change for its small footprint; the guard is needed regardless (any 401 shouldn't crash the SPA).
- **Global `DelegatingHandler`, not per-call try/catch** — one handler covers every API call.
- **Registered as the outermost handler** — `AuthorizationMessageHandler` throws `AccessTokenNotAvailableException` before delegating, so only a wrapping handler can catch it; it also observes the bubbled-up 401.
- **`ErrorBoundary` in `MainLayout`** as a safety net for any transient render exception (recovered on navigation).

## Changes

- New `KomTracker.WEB/Infrastructure/ReauthenticateOnFailureHandler.cs` — on `AccessTokenNotAvailableException` or a `401` response → `NavigationManager.NavigateToLogin("authentication/login")`.
- `KomTracker.WEB/DependencyInjection.cs` — `AddScoped<ReauthenticateOnFailureHandler>()` + chained as the first (outer) message handler on the API `HttpClient`, before `AuthorizationMessageHandler`.
- `KomTracker.WEB/Shared/MainLayout.razor` (+ `.razor.cs`) — wrap the authorized `@Body` in `<ErrorBoundary Context="ex">` with a MudAlert fallback; `Recover()` on `LocationChanged` (implements `IDisposable`).

## Verification

- `dotnet build` green (132 tests unaffected).
- Manual (running stack): log in → restart API (wipes the in-memory refresh token) → refresh page → app redirects through `authentication/login` and returns logged in (no Strava prompt), data loads; no unhandled exception / crash.

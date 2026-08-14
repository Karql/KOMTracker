# BikeTracker — Phase 1c-ii: Strava scope escalation (activity:read_all, opt-in)

**Status:** Done — backend/WEB compile (Infrastructure + WEB built; API project not rebuilt while the app was running — file lock only, no code errors), tests green (Application 137, Infrastructure 35). No migration. Pending: full API rebuild + live smoke once the running app is stopped.
**Date:** 2026-08-14
**Concept:** `docs/biketracker/CONCEPT.md` §6 (consent & opt-in), D-16. Reality: `docs/strava-api-notes.md`. Builds on 1c-i (`.ai/plans/2026-08-13-biketracker-phase-1c-i-strava-bikes.md`).

## Context
Tokens carry the login scopes `read, activity:read, profile:read_all`. Sync works with `activity:read` but **misses private / "Only You" rides and gets coarser start/finish points** → bike mileage can be under-counted. `activity:read_all` fixes it. Let users **opt in** to the wider scope without forcing it on login, **see their granted scopes**, understand the accuracy trade-off, and be reassured activities are never shown to others.

**Approach:** a standalone "upgrade" side-flow isolated from IdentityServer login. *Grant full access* → new identity endpoint → Strava authorize (wider scope + `approval_prompt=force`) → new callback exchanges the code and **overwrites the athlete's stored token** (widened `Scope`) → redirects back. No new IdentityServer session, no app-token reissue, no `ConnectCommand`. Athlete comes from Strava's exchange response. No Strava-dashboard change (same callback domain, new path).

**Placement:** action on the **Account page** ("Strava" section: scopes + *Grant full access*); the **Strava bikes page** keeps an info notice that links to it. **After upgrade: just refresh status** (private rides pulled by nightly full sync / manual Sync — no auto re-sync).

## Decisions (rationale)
- **D-1cii-1 Standalone upgrade side-flow, not a forced OIDC re-login.** Reuses the Strava authorize-URL build + `TokenService.ExchangeAsync` + token upsert, skips sign-in; never affects normal login's scope.
- **D-1cii-2 Always store the freshly-exchanged token; report `read_all` presence.** Avoids a stale/invalidated old token; if the user declines `read_all`, store the valid token anyway and report "still basic" → UI re-offers.
- **D-1cii-3 One authorize scope set for login + escalation: `Constants.Strava.AuthorizeScopes` = `read, activity:read, activity:read_all, profile:read_all`.** New connections request `activity:read_all` up front (login `approval_prompt=auto`); the escalation re-requests it with `approval_prompt=force`. Requesting `activity:read` too means declining the private part still leaves basic access (Strava lets the user uncheck the private scope). This supersedes the earlier "minimal-by-default" login (still opt-in in effect — the user can decline).
- **D-1cii-4 Relax `ConnectCommand.VerifyRequiredScope`: activity requirement = `activity:read` OR `activity:read_all`.** Once a user has `read_all`, a later normal login can lack literal `activity:read`; the old all-literals check would reject a valid login. Login still *requests* minimal `RequiredScopes`.
- **D-1cii-5 Action on Account, notice on Strava bikes; no auto re-sync** (user choice).
- **D-1cii-6 Open-redirect guard** on the upgrade endpoint's `returnUrl`: it must start with a configured `IdentityConfiguration.RedirectUris` prefix (same trust model as `StartWithRedirectUriValidator`; those are prefixes like `https://localhost`, not exact URIs — origin equality was wrong and rejected valid dev URLs).
- **D-1cii-7 UI wording avoids "full access"** (no write scopes exist): "Allow private rides" / activity-access "All rides (incl. private)" vs "Public rides only".
- **D-1cii-8 Revoke = re-auth with a narrower scope set** (`/account/upgrade?mode=basic` → `BasicScopes`, no read_all, `approval_prompt=force`) — replaces the token with a public-only one. NOT Strava `deauthorize` (that would disconnect the whole app incl. KOM). The callback reports only `ok|error`; the web derives the message from an `intent=allow|revoke` param it puts in returnUrl + the refreshed access level.

## Checklist

### Infrastructure — identity endpoints (the bridge)
- [ ] `Infrastructure.Shared/Identity/Constants.cs` — `EndpointNames.Upgrade` + `.ConnectUpgrade`; `ProtocolRoutePaths.Upgrade = "/account/upgrade"` + `.ConnectUpgrade = "/account/connect-upgrade"`.
- [ ] Extract Strava authorize-URL builder from `LoginEndpoint` → `StravaAuthorizeUrl.Build(clientId, scopes, approvalPrompt, redirectUri, returnUrl)`; refactor `LoginEndpoint` to use it (`RequiredScopes`, `auto`, `Connect`).
- [ ] `Identity/Endpoints/UpgradeEndpoint.cs : IEndpointHandler` — read+validate `returnUrl` origin (D-1cii-6), build URL with `SyncScopes` + `approval_prompt=force` + redirect `ConnectUpgrade`, `RedirectResult`.
- [ ] `Identity/Endpoints/ConnectUpgradeEndpoint.cs : IEndpointHandler` — read `code`/`scope`/`state`; `_mediator.Send(UpgradeScopeCommand)`; decode `state`→returnUrl; redirect with `?strava_upgrade=granted|denied|error`.
- [ ] Register both via `.AddEndpoint<…>` in `IdentityDependencyInjection`.

### Application
- [ ] `Constants.Strava.SyncScopes` = `{ read, activity:read_all, profile:read_all }`; shared const for `activity:read_all`.
- [ ] `Commands/Account/UpgradeScopeCommand.cs { Code, Scope }` → `Result<UpgradeScopeResult>(bool HasActivityReadAll)`: `TokenService.ExchangeAsync` → fail ⇒ Fail; else `AddOrUpdateTokenAsync` (+ optional athlete) + `SaveChangesAsync`; `HasActivityReadAll = scope contains activity:read_all`.
- [ ] Relax `ConnectCommand.VerifyRequiredScope` (D-1cii-4).
- [ ] Extend `StravaSyncStatusModel` + `GetStravaSyncStatusQuery` with `Scopes` (string[]).

### API
- [ ] `StravaSyncStatusViewModel` += `Scopes`; map in `StravaBikeMappings`. (No new controller endpoint; Account reads `GET /bike-tracker/strava/sync-status`.)

### WEB
- [ ] **Account** — "Strava" `MudTabPanel`: fetch sync-status; access level (Full green / Basic warning) + scope chips; when basic, benefits + "activities never shown to others" + **Grant full access** → `NavigateTo($"{authority}/account/upgrade?returnUrl=…", forceLoad:true)` (authority from `Configuration["IdentityConfiguration:Authority"]`); on init read `?tab=strava` + `?strava_upgrade=…` (Snackbar + reload).
- [ ] **Strava bikes** — make the `!HasActivityReadAll` alert actionable → **Manage Strava access** → `NavigateTo("account?tab=strava")`.

### Tests
- [ ] `UpgradeScopeCommandHandlerTests` (read_all present/absent/exchange-fail); `ConnectCommand` relaxed-scope test; `StravaAuthorizeUrl.Build` test.

### Docs
- [ ] `CHANGELOG.md` `## UPCOMMING`; `.ai/README.md`.

## Verification
- `dotnet build src/KomTracker.sln` + targeted `dotnet test` green. No migration.
- Manual: Basic user → Account/Strava shows Basic + chips + notice; Strava bikes notice links there. Grant full access → Strava forced consent → approve → back `?strava_upgrade=granted`, now Full; DB `token.Scope` updated. Normal logout→login still works. Decline read_all → `denied`, still Basic, token valid. Manual Sync now pulls private rides.

## Out of scope
- Auto re-sync post-upgrade; full disconnect / Strava `deauthorize` UI (revoke of private-rides access IS done); **1d** mileage display; webhooks (Phase 6); components/installations (Phase 2/3).

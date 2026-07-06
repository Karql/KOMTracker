# Refresh athletes

**Status:** Done
**Date:** 2026-07-06

## Goal

Athlete profile data (name, avatar/`profile_medium`, etc.) is only refreshed at login. Athletes who haven't logged in for a long time keep stale data — most visibly broken avatar links, which look bad across the app. Add a `RefreshAthletesCommand` that iterates all athletes, fetches each one's latest profile from Strava (`GET /athlete`), and updates the DB. Run it daily and expose a manual admin trigger.

## Decisions

- **Refresh via `GET /athlete` per athlete, using that athlete's own token** — because Strava's `/athlete` returns the *token owner's* profile (no bulk endpoint), and we already persist a per-athlete token; token acquisition reuses `AthleteService.GetValidTokenAsync` (auto-refresh).
- **Reuse `AthleteSummaryModel` + the existing `AthleteSummaryModel → AthleteEntity` AutoMapper map** — because `GET /athlete` (DetailedAthlete) is a superset of the summary; extra JSON fields are ignored, so no new model/mapping is needed.
- **Persist with `AddOrUpdateAthleteAsync` (immediate FlexLabs Upsert)** — because it already updates the mutable profile fields; the write is immediate so no `SaveChanges`/tracking concerns.
- **On `429` break the whole run; per-athlete errors are logged and skipped** — because of Strava rate limits and the project's resilience convention (mirrors `TrackKoms`/`RefreshSegments`).
- **Job at 23:45 Europe/Warsaw, toggled by `RefreshAthletesJobEnabled`** — as requested; sits just before `RefreshStats` (23:55).
- **Command lives in `Commands/Account`** — as requested (profile/account concern).

## Checklist

- [x] Strava client: `IAthleteApi.GetAthleteAsync(string token)` + `AthleteApi` impl (`GET https://www.strava.com/api/v3/athlete`, Bearer, 401/429/unknown handling like `GetKomsAsync`); `Model/Athlete/Error/GetAthleteError.cs`.
- [x] Application Strava `IAthleteService.GetAthleteAsync(int athleteId, string token)` + `GetAthleteError`; Infrastructure `AthleteService.GetAthleteAsync` (map `AthleteSummaryModel → AthleteEntity`, map API errors).
- [x] `RefreshAthletesCommand` + handler (`Commands/Account`): iterate `GetAllAthletesAsync`; per athlete get valid token, `GET /athlete`, `AddOrUpdateAthleteAsync`; `429` → stop; other errors logged + continue.
- [x] `RefreshAthletesJob` (`[DisallowConcurrentExecution]`) → command; schedule `0 45 23 * * ?` in `Startup`; `RefreshAthletesJobEnabled` in `ApplicationConfiguration` (default true).
- [x] `AdminController`: `PUT /admin/refresh-athletes`.
- [x] FAQ "How often data is refreshed?" — add athlete-profile line (23:45 Europe/Warsaw).
- [x] `CHANGELOG.md` `## UPCOMMING` → `### Features`.
- [x] `dotnet build` + `dotnet test` green (114 tests).

## Verify

- Admin `PUT /admin/refresh-athletes` updates `athlete` rows (e.g. `profile_medium`, name) from Strava; athletes with invalid tokens are skipped; a `429` stops the run without failing everything.

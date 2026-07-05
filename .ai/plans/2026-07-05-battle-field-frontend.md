# Battle Field — page + API

**Status:** Done
**Date:** 2026-07-05

## Goal

A "Battle Field" page (styled like Ranking / Koms Changes) showing a head-to-head KOM-takeover ranking (`A vs B 5:3`) over the `kom_takeover` data, plus two API endpoints. Data is computed and oriented server-side (winner always on the left); the client only filters and paginates. Filters: **Period**, **Activity Type**, **Club**. Clicking a KOM count opens a modal listing the segments (columns like Koms Changes).

## Decisions

- **Club filter = both athletes must be members** — because a club view is an intra-club leaderboard ("who among us beats whom"); a pair involving a non-member isn't relevant to that board.
- **Naming: range combo = `Period`; endpoints `GET /kom-takeovers/pairs` and `GET /kom-takeovers/efforts`** — because these are more intent-revealing than the initial `Range` / `grouped` / `flat`.
- **Only non-reverted takeovers counted/shown everywhere** — because reverted ones (winning ride flagged / deleted / set private, then undone) aren't real competitive wins and would inflate the ranking.
- **`Total` period → `date_from`/`date_to` = `null` end-to-end** — because `null` cleanly means "no date bound" (repo already treats it so); avoids a sentinel min/max date.
- **Year list start = `StartYear` from WEB config, not hardcoded** — because the combo is a frontend concern (the WASM client can't read the API's `ApplicationConfiguration`), and making it configurable keeps the project forkable for datasets starting in a different year.
- **Details date column = taken effort's `StartDate`** — because it's the actual ride date of the KOM effort (consistent with the Koms page), more meaningful than the detection/track date.
- **Tie (`WinnerKoms == LoserKoms`) → lower `AthleteId` on the left** — because orientation must be deterministic/stable across reloads when both directions are equal.
- **Details modal reuses `EffortViewModel`** — because the columns need only segment + effort data (already modeled); a dedicated view model would be redundant and `SummarySegmentEffort` is unused there.

## Backend

- [x] Models: `KomTakeoverPairModel`, `KomTakeoverCountModel` (Application/Models/Segment).
- [x] ViewModel: `KomTakeoverPairViewModel` (API.Shared/ViewModels/KomTakeover).
- [x] Mapping in `DtoProfile`: `KomTakeoverPairModel → KomTakeoverPairViewModel`.
- [x] Repo `GetTakeoverCountsAsync(athleteIds?, from, to, activityType)` → directed counts (both athletes in set when provided; `!Reverted`; `GROUP BY takenBy, lostBy`).
- [x] Repo `GetTakeoverEffortsAsync(takenBy, lostBy, from, to, activityType)` → `EffortModel` (taken effort + segment), `StartDate` desc.
- [x] `ISegmentService` thin wrappers for both repo methods.
- [x] Pure orientation helper `GetKomTakeoverPairsQueryHandler.OrientPairs` (winner-left, tie→lower id, order by total desc) + unit tests (6 cases).
- [x] Query `GetKomTakeoverPairsQuery` (+ handler) — club→athletes (both-in-club) or all.
- [x] Query `GetKomTakeoverEffortsQuery` (+ handler).
- [x] Controller `KomTakeoversController` (`/kom-takeovers`, `[BearerAuthorize]`): `GET /pairs`, `GET /efforts`.

## Frontend (WEB)

- [x] `StartYear` in `wwwroot/appsettings.json` and `deployments/prod/conf/kom-tracker-web/appsettings.json`.
- [x] `Pages/BattleField.razor` + `BattleField.cs` (`/battle-field`): Period / Activity Type / Club combos; `GET /kom-takeovers/pairs` on change; MudTable grid (Winner Athlete, Winner KOMs, Loser KOMs, Loser Athlete) with client-side search + pager; clickable KOM counts open the dialog.
- [x] `Shared/BattleEffortsDialog.razor`: `GET /kom-takeovers/efforts`, details table (Date, Segment link, Distance, Elev diff, Grade, Speed, HR, Power, Time, Cat, Type).
- [x] `NavMenu.razor`: `SportsKabaddi` link to `battle-field`.

## Docs / verify

- [x] `CHANGELOG.md` `## UPCOMMING` → `### Features`: `Battle field: page with head-to-head KOM takeover ranking`.
- [x] `dotnet build` + `dotnet test` green (114 tests, incl. 6 orientation tests).
- [X] Manual (needs running app + backfilled data): endpoints return oriented pairs / correct efforts; page filters reload; modal shows correct segments; links & icons correct.

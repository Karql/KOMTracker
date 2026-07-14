# KOM ratings everywhere + Ranking click-to-open KOM lists

**Status:** Done (live UI smoke pending on a running stack)
**Date:** 2026-07-12

## Goal

Spread the existing The Bar / The Burn ratings to the remaining effort tables and make the Ranking page interactive: clicking a count opens a modal listing the underlying KOMs. A UX boost that reuses the rating pipeline built on 2026-07-11.

## Decisions (with rationale)

- **Shared rating enricher** — `Application/Services/KomRatingEnricher.Apply(effort, sex, weight)` sets `Bar`/`Burn` on an `EffortModel`. Three query handlers needed the same logic, so it's extracted once (DRY). The Bar is athlete-independent; The Burn needs the effort holder's sex/weight.
- **Compute per effort's own athlete where multi-athlete** — the KomsChanges page (`stats/koms-changes`) is multi-athlete; each row already pairs the effort with its `AthleteEntity`, so enrich in the join. Koms list, takeover efforts and the changes-detail endpoint are single-athlete (load once).
- **Ranking Total modal reuses `athletes/{id}/koms`** — the ranking payload carries only counts. The koms endpoint already returns `EffortViewModel[]` with `Bar`/`Burn` + `Segment.ExtendedCategory`, so the client fetches it and filters by the selected activity type (+ clicked category). Live data vs precomputed counts can rarely drift — acceptable (maintainer's call).
- **Ranking Koms-changes modal needs a new endpoint** — the New/Lost lists live only in the precomputed `AthleteStats` (not in the ranking payload), and sending them all would bloat it. Added a lazy `GET /ranking/koms-changes-details?athlete_id=&period=&direction=&activity_type=` reading the stats.
- **Activity-type filter is applied AND shown in the modal title**; the Club filter only scopes which athletes are ranked, so it does not affect a single athlete's KOM list.
- **One dialog for all effort lists** — `KomsListDialog` (param `Efforts`, title via `IDialogService`) is the single "dumb" renderer; every caller (Battle Field, both Ranking modals) fetches/filters and passes the list in. `BattleEffortsDialog` was removed and Battle Field's fetch moved into `BattleField.cs` — no two near-identical dialogs.

## Changes

Backend:
- New `Services/KomRatingEnricher.cs`; `Queries/Athlete/GetAllKomsQuery.cs` refactored to use it.
- `Queries/Stats/GetLastKomsChangesQuery.cs` — enrich each row with its athlete (multi-athlete).
- `Queries/KomTakeover/GetKomTakeoverEffortsQuery.cs` — inject `IKOMUnitOfWork`, load the winner, enrich.
- New `Queries/Ranking/GetKomsChangesDetailsQuery.cs` (+ `KomsChangesPeriod`/`KomsChangeDirection` enums) reading `AthleteStats`; `RankingController` `koms-changes-details` action.

Frontend:
- `Pages/Koms.razor` — segment name links to `segment_efforts/{SegmentEffort.Id}`.
- `Pages/KomsChanges.razor` — "The Bar"/"The Burn" columns between Cat and Type (`RankBadge`).
- New `Shared/KomsListDialog.razor` (Efforts table with the full column set + ratings); **removed `Shared/BattleEffortsDialog.razor`** and moved its fetch into `BattleField.cs`.
- `Pages/Ranking.razor` + `Ranking.cs` — clickable cells (Total: category + total; Koms changes: 6 windows) opening `KomsListDialog` via `IDialogService`; not clickable when 0.

## Verification

- `dotnet build` + `dotnet test` green (132 tests; +3 `KomRatingEnricherTests`; existing `GetAllKomsQueryTests` still passes after the refactor).
- Live UI smoke (running stack): ratings visible on koms-changes + Battle Field; `/koms` link opens the effort; Ranking Total category/total counts (>0) open the filtered KOM list; Koms-changes New/Lost counts (>0) open the matching list; activity type reflected in filter + title; zero cells inert.

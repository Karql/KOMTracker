# KOM difficulty + effort indicators ("The Bar" / "The Burn")

**Status:** Done (calibration + live UI smoke pending on a running stack)
**Date:** 2026-07-11

## Goal

Not all KOMs are equal — a soft time on a busy boulevard is an easy KOM, a savage time on a remote climb is a hard one. Express this with **two complementary, physics-based indicators on one shared scale** (a strong-amateur power-duration curve → "% of the human limit for that duration"):

1. **The Bar** — *difficulty (estimated)*: from the KOM **time** + segment terrain, for a reference rider. "How hard is this KOM to take?" (property of the record). Full coverage.
2. **The Burn** — *effort (measured)*: the holder's real `AverageWatts / athlete.Weight` = actual W/kg. "How hard did the holder actually work?" (property of the performance). Power-meter rides only.

Same scale ⇒ comparable, and the **Bar↔Burn gap is signal** (high Bar + low Burn ⇒ time earned cheaply / attack opportunity).

## Decisions (with rationale)

- **Two metrics, not one** — different questions (difficulty-to-take vs effort-spent); the divergence is a feature.
- **Reference = Sauce4Strava's continuous Coggan model, ported to C#** — Sauce (`src/common/lib.js`, MIT) fits Coggan's profile as a smooth `_rankScaler(duration, c)` curve (5 constants, `high`/`low` bound per gender) → continuous W/kg at any duration (solves the "only 3 points" problem), endurance decay >1 h built in. Better than hand-picked anchors, and it's the real thing a working tool uses.
- **Primary number = "World Ranking"** (`round(max(0, level)×100)`, `level = (wKg−low)/(high−low)`) — one monotonic scale, drives sorting and the future prestige-weighted ranking. ~88 = World Class start. **Uncapped above 100** (only floored at 0) so freakish/aided times show how far beyond World Class they are and the top sorts without ties; matches Sauce (no clamp). `CategoryRanking` stays clamped 0–100. **FAQ must not imply 100 = World Class** — the category badge is the level; the number just shows how extreme (WC ~88+, can exceed 100).
- **Category = badge; tooltip shows W/kg + the category's W/kg range + watts** — Coggan category (cutoffs on `level`: ≥7/8 WC, 6/8 Pro, 5/8 Cat1, 4/8 Cat2, 3/8 Cat3, 2/8 Cat4, 1/8 Cat5, else Recreational) is the readable label. Tooltip: effort W/kg, the assigned category's `[min–max]` W/kg for that duration (so at WC you see the top of the band and how far the effort exceeds it), and watts. Result carries `CategoryMinWKg`/`CategoryMaxWKg`. Units capitalised (`W`, `W/kg`). Gender-aware via `athlete.Sex`.
- **NP blending drops out** — Sauce blends Normalized Power (1200–3600 s), but NP needs the per-second stream we don't have; its guard makes passing no NP just use plain power. We keep the endurance decay (it's in the curve).
- **Difficulty estimated from time, ignoring measured watts** — neutral-condition estimation absorbs wind/draft as "the bar is high" (correct for difficulty-to-take) and has full coverage.
- **Effort uses measured power only** — `AverageWatts` when `DeviceWatts=true`; `athlete.Weight` for W/kg. Missing meter/weight ⇒ n/a.
- **Terrain (difficulty) = steady-speed model on NET average grade** — gravity term uses `average_grade` (signed), which is the correct energy balance for *average* power. **Do NOT use `total_elevation_gain`**: it counts only the climbing metres and ignores the energy given back on descents, so on rolling / net-flat segments it double-counts and massively inflates the estimate (real bug found on "Grajów - Górki": 3892 m, net −0.4%, gain 66 m → the gain model gave 563 W / World Class; the net-grade model gives ~363 W / Cat 3, vs the holder's measured 315 W).
- **Cycling only** — the whole power/physics model is bike-specific, so both indicators return null for any activity type other than `Ride` (a walking/running KOM was showing a nonsense Bar).
- **Exclude only clear descents & ultra-short** — grade < −3% (moderate descents still rated) / P≤0 (coast-down) / duration < 10 s ⇒ n/a. Fast aided flats deliberately stay rated (often World Class): The Bar = "how hard to take solo", and a 60+ km/h flat time genuinely is near-unbeatable solo — the Burn/gap reveals it was aided.
- **Compute server-side into `EffortViewModel.Bar/.Burn`** — The Burn needs the *holder's* weight/sex, and `/koms` can show another athlete via `?AthleteId=`, so the client can't supply it. Calculator stays pure in `Application.Shared` (reusable client-side later). No migration/backfill.

## Reference model (ported from Sauce; MIT — credit Sauce4Strava + Coggan)

```
_rankScaler(duration, c):
    t     = (c.slopePeriod / duration) * c.slopeAdjust
    slope = log10(t + c.slopeOffset)
    wKgDiff = slope ^ c.slopeFactor
    enduro  = duration > 3600 ? 1 / (ln(duration/3600)*0.1 + 1) : 1
    return (wKgDiff + c.baseOffset) * enduro
high = _rankScaler(d, C[gender].high);  low = _rankScaler(d, C[gender].low)
level = (wKg - low) / (high - low)
```
Constants: male.high `{2.82, 2500, 1.4, 3.6, 6.08}`, male.low `{2, 3000, 1.3, 1, 1.74}`; female.high `{2.65, 2500, 1, 3.6, 5.39}`, female.low `{2.15, 300, 6, 1.5, 1.4}` (slopeFactor, slopePeriod, slopeAdjust, slopeOffset, baseOffset). Sanity: male 1 h → high≈6.37/low≈1.84; 4.5 W/kg FTP → Cat 2.

Physics (The Bar), steady speed `v=distance/elapsed`, `θ=atan(avgGrade/100)`: `P=[m·g·sinθ·v + Crr·m·g·cosθ·v + ½·ρ·CdA·v³]/η`; `Wkg=P/m_rider`. Gravity is signed (assists on descents). Constants: total ≈78 kg, `m_rider=70`, `Crr=0.005`, `CdA=0.32`, `ρ=1.225`, `η=0.97`, `g=9.81`.

Result model: `{ Level, Ranking(0–100), CategoryRanking(0–100), Category(key/label/short), WKg, Watts, IsRated }`.

## Checklist

- [x] `CogganRank` (Application.Shared) — port `_rankScaler` + constants + level→category; MIT/Coggan credit.
- [x] `KomDifficultyCalculator` (Application.Shared) — `EstimateDifficulty(...)` (physics) + `MeasuredEffort(...)`; `KomRankResult` + `KomCategory`.
- [x] Unit tests (Application.Tests/Helpers) — difficulty cases, effort cases, port cross-check vs Sauce numbers, n/a edges (11 tests).
- [x] `EffortModel` + `EffortViewModel` + `Bar`/`Burn`; computed in `GetAllKomsQueryHandler` (loads athlete Weight/Sex once via `IKOMUnitOfWork`); AutoMapper carries them across.
- [x] `/koms` (`Koms.razor`): two columns between Cat and Type; `RankBadge` component (color+short category+ranking, tooltip w/kg·watts·cat%); sort by `Ranking`; `–` when unrated.
- [x] `ViewHelper.GetRankCategoryColor/Short/Label`; palette in the spirit of Sauce `images/ranking` (colours only; PNGs not copied).
- [x] FAQ (`Faq.razor`): user-friendly "What are The Bar and The Burn?" panel with the category badge legend (mirrors the segment-categories panel).
- [x] CHANGELOG `## UPCOMMING` (### Features).
- [x] `dotnet build` + `dotnet test` green (125 tests). Live UI smoke on `/koms` + calibration pass deferred to a running stack.

## Deferred

Prestige-weighted ranking (Σ Bar), Battle Field difficulty, category filter, Bar↔Burn gap highlight; final tier bands; persist for SQL sort; ρ-by-altitude; use estimate↔measured gap to calibrate.

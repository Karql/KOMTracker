# CLAUDE.md

Guidance for AI assistants (Claude Code) working in this repository.

## Conventions
- **Conversation with the maintainer is in Polish. Everything committed to the repo is in English** — code, comments, docs, plans, commit messages, `CHANGELOG.md`.
- Spec-driven workflow: non-trivial feature specs/checklists live in **`.ai/plans/`** (dated `YYYY-MM-DD-<slug>.md`, checklist format with a `Decisions` section). Tick items off during implementation.
  - **On plan approval, the first implementation step is to persist the spec to `.ai/plans/YYYY-MM-DD-<slug>.md`** (so every non-trivial feature is documented in the repo, even if implementation is deferred).
  - **Every `Decisions` entry must include a short rationale** (state the *why*, not just the *what*) — these files are the durable record of why a feature was built the way it was.
  - **Small/trivial changes may skip the `.ai/plans/` spec, but `CHANGELOG.md` (`## UPCOMMING`) must ALWAYS be updated.** When unsure whether a change needs a spec, ask rather than skip silently.
- Full project overview: **`.ai/README.md`** (architecture, flows, conventions). Keep it up to date after each feature.
- Design notes & history: `NOTES.md`. Local dev deps: `DEV.md`.

## Build & test
- Build: `dotnet build src/KomTracker.sln`
- Test: `dotnet test src/KomTracker.sln` (xUnit + FluentAssertions + NSubstitute + AutoFixture)
- EF migration: `dotnet ef migrations add <Name> --project src/KomTracker/KomTracker.Infrastructure --startup-project src/KomTracker/KomTracker.API`

## Layout
Clean Architecture under `src/KomTracker` (Domain / Application / Infrastructure / API / WEB) + `src/Strava` (Strava API client) + `src/Utils`. See `.ai/README.md`.

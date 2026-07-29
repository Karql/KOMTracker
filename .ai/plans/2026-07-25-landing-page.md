# Landing page (public marketing page)

**Status:** Done (live UI smoke pending on a running stack)
**Date:** 2026-07-25

## Goal

Turn the bare login screen into a real, shareable marketing landing page so a single link sells the app — no more DM'ing screenshots + a manual pitch. Light & clean look (not a generic dark AI-landing), brand orange `#FC4C02`, Roboto, tasteful scroll animations.

## Decisions (with rationale)

- **Rebuilt in place at `Pages/Auth/Login.razor`** (kept `@page "/login"`, `@layout EmptyLayout`, the `<AuthorizeView>` → `<Authorized><RedirectToHomePage/></Authorized>`, and the Strava trigger `<a href="authentication/login">`). Reason: the whole anon flow already routes here (`/` is `[Authorize]` → `RedirectToLoginPage` → `/login`), so no routing/auth/redirect changes were needed, and logged-in users still bounce to the dashboard. The official Strava "Connect with" SVG stays the CTA (brand compliance).
- **Custom HTML + CSS in the global `wwwroot/css/app.css`** (a `.lp`-prefixed `/* #region landing */` block) instead of MudBlazor layout components — a landing needs bespoke layout/motion. Kept `<MudThemeProvider>` (so `MudIcon`s + theme vars resolve; icon color inherits `currentColor`, no `::deep` needed); dropped the unused popover/snackbar providers.
  - **Why not scoped `Login.razor.css`:** scoped CSS requires the `KomTracker.WEB.styles.css` bundle `<link>` in `index.html`, which this app never had (no scoped sheets existed) — so the scoped file simply never loaded and the page rendered unstyled. `app.css` is already linked and already cache-busted by the build's `?v=__ASSET_HASH__` pipeline, so folding the styles there is the least-surprise fix. All selectors are `.lp`-prefixed → no collisions.
- **Progressive-enhancement motion** via a new ES module `wwwroot/js/landing.js` (first custom JS), imported with `IJSRuntime` in `OnAfterRenderAsync` (idempotent `init()`, re-run-safe once the anon branch mounts / after auth resolves; disposed in `IAsyncDisposable`, swallowing `JSDisconnectedException`). JS adds a `landing--js` flag so the fade-up reveal styles only hide content when JS runs — **no JS ⇒ everything visible**. `prefers-reduced-motion` disables all animation. Sticky header gains a shadow past the hero via a scroll listener.
- **Light design with character**: warm off-white section banding (`#fff`/`#fff6f1`), ink `#1b1c22`, orange accent + word-highlights, a faint radial-glow + hairline pattern in the hero, big `clamp()` headline, alternating two-column feature rows. Hero/CTA headlines use `text-wrap: balance` so lines stay even (no orphan words); the hero column is `flex`-centered (not `margin:auto`) so the sub-copy is reliably centered under the title. The ranking/Battle-Field avatars are playful animal emoji (golden retriever, cats, dogs) in soft peach circles — a small bit of personality.
- **Visuals — faithful in-page HTML/CSS mockups, not screenshots or images.** Every section's visual is a small HTML mockup styled to look exactly like the real app: an orange app-bar strip + a mini koms-changes / koms table inside a browser frame, an email card replicating the real notification email, a rankings leaderboard, a Battle Field scoreboard, and a Bar/Burn + category/direction badge card. This renders crisp at any size, is theme-consistent and responsive, needs no image files or maintainer swap, and — being built by us — exposes no real user data. **Badge colours/letters are pulled from the real `ViewHelper`** (`GetRankCategoryColor`/`GetExtendedCategoryColor`: e.g. The Bar `C1` `#f4511e`, `WL` `#f40`, `SP` `#00d`, `D1` `#000`) so the mockups match the app pixel-for-pixel in spirit.
  - **Why mockups, not real screenshots:** real ranking/Battle-Field/koms-changes screens show other athletes' names + avatars — a public landing must not expose them. Mockups use plausible content (fake rivals "Kasia W."/"The rival"; the maintainer's own public segment names in the email/koms visuals) with **no private data** (the email address is `you@example.com`, not the real one).
  - **Why not fabricated numbers:** the first pass invented UI that doesn't exist (a "beaten by 0.4s" sub-line, sub-second deltas, a "new-segment steal" chip in blue — in-app blue means *Improved*). The blind-spot chips + change icons now use the app's **real change types**: New (green), Improved (blue), Lost (red), Returned (grey) — matching `KomsChanges.razor` (`Success`/`Info`/`Error`/`Dark`). The hunt mock shows only the filters that exist (Direction, Location) plus a **sort-ascending arrow on The Bar** column (there is no "low Bar" filter — you sort). Rankings and Battle Field are rendered inside the same app browser-frame as the other screens (leaderboard; head-to-head pairs with the winner on the left).
  - Earlier generated PNG placeholders (`wwwroot/img/landing-page/*`) were removed once the HTML mockups replaced them.
- **Copy** written in a light, human, salesy voice; feature wording sourced from `Pages/Faq.razor` (The Bar / The Burn / categories) for accuracy.

## Files
- `Pages/Auth/Login.razor` — full rewrite (header, hero, 6 feature sections, final CTA, footer).
- `wwwroot/css/app.css` — landing styles appended as a `/* #region landing */` block (global, `.lp`-prefixed; not scoped — see decision above).
- `wwwroot/js/landing.js` — `init()`/`dispose()` (IntersectionObserver reveals + header scroll + `landing--js` flag).
- `CHANGELOG.md` — `## UPCOMMING` Features entry.

(No image assets: all section visuals are HTML/CSS mockups; only the existing brand logos in `wwwroot/img/` are reused.)

## Verification
- `dotnet build src/KomTracker/KomTracker.WEB/KomTracker.WEB.csproj -c Debug` green.
- UI smoke (running stack, logged **out**, `/login`): sections reveal on scroll (all visible with JS off); sticky header shadow past the hero; both Strava CTAs start OAuth; email mock renders in its frame; clean mobile stacking; reduced-motion disables animation. Logged **in**: `/login` → dashboard (unchanged).

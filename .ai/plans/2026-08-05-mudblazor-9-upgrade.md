# MudBlazor 8.15.0 → 9.8.0 upgrade

**Status:** Done (code + build + tests green; live UI smoke pending)
**Date:** 2026-08-05

## Outcome
Bumped to **9.8.0**; restore + build on net10 clean. Only **two** code fixes were needed (audit was accurate):
1. `Shared/MainLayout.razor` — avatar `MudMenu` `ActivatorContent` → `Context="menu"` + `@onclick="@menu.ToggleAsync"` (v9 menus don't auto-open).
2. `Pages/Account.razor` — `MudTabs` `PanelClass` → `TabPanelsClass` (v9 rename; surfaced as `MUD0002`, only a *warning* because the Debug MSBuild prop `MudIllegalParameters=V7IgnoreCase` downgrades illegal-attribute errors — worth remembering: **param renames show as warnings, not errors** here, so scan MUD0002 warnings after Mud upgrades).

Theme palette (`PaletteLight`/`PaletteDark` init), `IMudDialogInstance.Close()/Close(DialogResult)/Cancel()` (KomsListDialog, LocationFilterDialog), `MudSelect`/`MudTable`/`MudField`/`MudSlider`/`MudExpansionPanels` all compiled unchanged. `dotnet test src/KomTracker.sln` = 151 passed.

## Context
Before starting BikeTracker we upgrade MudBlazor (8.15.0 → latest 9.x) — v9 is a major with many breaking changes, and the longer we wait the more UI accumulates on the old version. MudBlazor is referenced **only** by `KomTracker.WEB` (single `PackageReference`), so blast radius is contained to the Blazor front.

## Breaking-change audit (v9 guide #12666 vs our usage)
Grepped the WEB project for every v9 breaking API. **Only one real hit in our code:**
- **`MudMenu` / `ActivatorContent`** (MainLayout avatar menu) — v9 makes `ActivatorContent` a `RenderFragment<MenuContext>` and the menu **no longer opens implicitly**; the activator must call `context.ToggleAsync`. → fix required.

Checked and **not used** (no change needed): custom `Converter`s, `MudGlobal` theming, `MudChart`/`MudTimeSeriesChart`/`MudChat`, `MudChip*`, `ObserveSystemThemeChange`/`GetSystemPreference`/system-theme APIs, `TextUpdateSuppression`/`ForceUpdate`, `DialogService.Show(...)` sync overloads (we already use `ShowAsync`), `ShowMessageBox`/`ShowForm`.

To verify at compile time (expected fine, fix if not):
- **Theme palette** (`Theme.cs`): property type changed `PaletteLight/PaletteDark` → `Palette`; we assign `new PaletteLight()/new PaletteDark()` (still derive from `Palette`) — should compile.
- **`IMudDialogInstance.Close()/Close(DialogResult)/Cancel()`** (KomsListDialog, LocationFilterDialog) — the removed `Close()` was on `DialogService`; the dialog-instance methods should remain. Switch to `CloseAsync` only if the build complains.
- MSBuild props `MudIllegalParameters=V7IgnoreCase` + `MudAllowedAttributePattern=LowerCase` (Debug) — v7-compat analyzer knobs; may be obsolete in v9. Drop/adjust if they warn/error.

Minor visual (accept): **MudLink** default `Typo` `body1` → `inherit` (koms table segment/time links now inherit cell font); **Grid/Stack** default spacing hard-coded; **popover Modal** default `true`→`false`. No action unless it looks off.

## Changes
- `KomTracker.WEB.csproj` — `MudBlazor` `8.15.0` → **`9.x`** (latest).
- `Shared/MainLayout.razor` — `MudMenu` avatar activator: `ActivatorContent Context="menu"` + clickable wrapper `@onclick="@menu.ToggleAsync"`.
- Fix any further compile errors surfaced by the build (per the audit list above).
- `index.html` MudBlazor CSS/JS references are version-agnostic (`_content/MudBlazor/...`) — no change.
- `CHANGELOG.md` `## UPCOMMING` entry.

## Verification
- `dotnet build src/KomTracker/KomTracker.WEB/KomTracker.WEB.csproj -c Debug` green (restore pulls 9.x for net10).
- `dotnet build src/KomTracker.sln` + `dotnet test src/KomTracker.sln` green.
- UI smoke (running stack): app shell renders; **avatar menu opens** (Account/Logout); dark-mode toggle works; tables/filters/dialogs (koms, location map dialog, ranking modal) behave; FAQ expansion panels; landing page unaffected.

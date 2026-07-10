# Web asset fingerprinting + immutable caching

**Status:** Done
**Date:** 2026-07-09

## Goal

After deploying a new `KomTracker.WEB` build the browser served the old app from cache. Content-hash every static asset so it can be cached `immutable` (no revalidation), and serve only `index.html` fresh. Standalone Blazor WASM behind nginx.

## Decisions

- **Framework: SDK fingerprinting (`OverrideHtmlAssetPlaceholders=true` + `blazor.webassembly#[.{fingerprint}].js`)** — the SDK hashes the loader in its filename and emits an import map for the hashed `_framework` assemblies. This is the heavy payload (~10 MB) → immutable.
- **CSS + RCL `_content/*` (MudBlazor, Auth): hashed by a build script, not the SDK** — verified the SDK does NOT fingerprint these (neither filename nor `?v=` placeholder resolves; forcing a `*.js` pattern even broke the framework fingerprint). So a POSIX script (`Docker/fingerprint-assets.sh`) replaces a `?v=__ASSET_HASH__` placeholder in `index.html` with each file's short sha256 at Docker image build. The placeholder is **harmless if unreplaced** (query ignored) → local `dotnet run` still loads assets.
- **Plotly refs point to the versioned filenames the package actually ships** (`plotly-3.5.0.min.js`, `plotly-interop-7.1.0.js`) — because the old `plotly-latest.min.js`/`plotly-interop.js` names don't exist in Plotly.Blazor 7.1.0 (they were 404 → broken dashboard chart). These are version-named by the library, so they self-bust and need no `?v=` hash. **Maintenance note:** bump these names when Plotly.Blazor is upgraded.
- **nginx: `immutable` for `/_framework/`, `/_content/`, `/css/`; `no-cache` only for `/` (index.html + appsettings.json + favicons)** — everything under the immutable paths is content-hashed (filename or `?v=`); the shell must stay fresh so it points at the new hashes.
- **Asset locations use `try_files $uri =404;` (only `/` falls back to `index.html`)** — because the single SPA fallback returned `index.html` (200) for ANY missing path, masking missing/renamed assets (this is how the broken Plotly reference stayed invisible instead of 404-ing).
- **The fingerprint build fails if a referenced file is missing** — catches stale refs (exactly how the Plotly bug was found).

## Done

- [x] `KomTracker.WEB.csproj`: `<OverrideHtmlAssetPlaceholders>true>`.
- [x] `wwwroot/index.html`: importmap + preload + `blazor.webassembly#[.{fingerprint}].js`; `?v=__ASSET_HASH__` on `css/app.css`, MudBlazor css/js, `AuthenticationService.js`; corrected Plotly refs.
- [x] `Docker/fingerprint-assets.sh` + Dockerfile step (runs on published wwwroot in the publish stage).
- [x] `Docker/nginx/nginx.conf`: immutable for `/_framework/` `/_content/` `/css/`; no-cache for `/`.
- [x] `CHANGELOG.md` `## UPCOMMING` → `### Bug fixes` (cache-busting + Plotly ref fix).
- [x] Verified via clean `dotnet publish` + running the script: all `?v=__ASSET_HASH__` replaced with real hashes, referenced files exist, framework fingerprinted, Plotly files present.

## Gotcha
Incremental publish can leave the framework placeholder unresolved (stale `obj` static-web-assets manifest); a clean build resolves it. The Docker image always builds clean, so prod is unaffected.

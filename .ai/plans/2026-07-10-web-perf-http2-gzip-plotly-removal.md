# Web performance: HTTP/2, gzip, remove Plotly

**Status:** Done
**Date:** 2026-07-10

## Goal

Lighthouse (desktop) on `/app/battle-field` scored Performance **25** — FCP 6.0 s, LCP 21.7 s, TBT 1.44 s. First-load payload was 21 MiB, uncompressed, over HTTP/1.1. Cut first-load bytes and round-trips with low-risk config changes (cache/fingerprint was already fine — this targets the *first* visit).

## Decisions

- **Enable HTTP/2 at the reverse proxy, not the app nginx** (`listen 443 ssl http2;`) — TLS terminates at the reverse proxy, so that connection is what the browser measures and where multiplexing matters; the app nginx sits behind it on plain HTTP/1.1 and the browser never talks to it directly. Syntax is `listen … http2` (image `nginx 1.21`; the `http2 on;` directive is 1.25.1+). The conf is bind-mounted → deploy = edit file + `nginx -s reload`, no rebuild.
- **Compress at the app nginx with `gzip_static on` + `gzip on`** — `_framework` already ships `.gz`/`.br` siblings, so `gzip_static` serves the precompressed `.gz` with zero runtime CPU (wasm/js/icu.dat); `gzip on` covers `_content`/`css` which the SDK does NOT precompress. Put it on the app nginx (not the reverse proxy) because that is where the files live; the reverse proxy forwards `Accept-Encoding` and passes `Content-Encoding` through, so no double compression.
- **No brotli** — better ratio than gzip on wasm (~20%) but the official `nginx:alpine` has no `ngx_brotli` module; would require a custom app-nginx image. Not worth it for the gain; revisit if needed.
- **Remove Plotly entirely instead of lazy-loading it** — it was ~9.6 MiB (4.7 MiB JS + 4.9 MiB `Plotly.Blazor.wasm`) loaded on *every* page, but the only usage (`Dashboard.razor`) was fully commented-out leftover experiment code; there is no chart in the app. Deleting the package/scripts is simpler and lower-risk than lazy-load machinery. Re-add a charting lib (with lazy-load) if/when charts are actually built.

### Follow-up (same day) — parity + API + nginx upgrade

- **HTTP/2 covers the API too, with no extra config** — the API is proxied from the same reverse-proxy `server` block as the web app, so the `listen 443 ssl http2;` already put both browser↔proxy connections on HTTP/2. The proxy→upstream hop stays HTTP/1.1 (nginx `proxy_pass` has no HTTP/2 upstream, and it's a zero-RTT local hop where multiplexing wouldn't help).
- **Compress the API at the reverse proxy, not in Kestrel** — API JSON was the one remaining uncompressed payload (`gzip` was off on the reverse proxy). Enabling `gzip on; gzip_proxied any;` there compresses dynamic JSON on the fly at the single TLS-terminating entry point, with no API code change/redeploy. Web assets already carry `Content-Encoding` from the app nginx, so nginx leaves them untouched (no double compression) — the edge gzip also acts as a safety net for any web type the app nginx didn't cover.
- **Added `http2` to the local reverse-proxy conf** (`kom-tracker-local.conf`, `listen 9999 ssl http2;`) for prod/local parity.
- **Kept `listen … ssl http2` over the newer `http2 on;` directive** — the former works on both nginx 1.21 and 1.31, so the mounted conf isn't coupled to the image tag (a rollback to 1.21 with `http2 on;` would fail to start).
- **Upgraded the custom reverse-proxy image 1.21 → 1.31** (`docker-images/nginx/1.31-alpine/`, c&p of the 1.21 Dockerfile) — 1.21 is EOL (~2021, many CVEs). Not required for HTTP/2 (that works on 1.21); done for security/currency. `headers-more` bumped 0.33 → 0.37 to compile against modern nginx. Both compose files repoint to `karql/kom-tracker-nginx:1.31-alpine` (build & publish the image before recreating).

### Fix: `gzip_static` was serving stale/wrong root files

- **`gzip_static on` must be scoped to the immutable asset dirs, not global.** The .NET publish emits `*.gz`/`*.br` for *every* static file, including the wwwroot root. With `gzip_static on` at `http` level, nginx served the precompressed sibling for root files too, which were wrong: (1) `index.html.gz` was produced during publish, *before* `fingerprint-assets.sh` rewrites `index.html`, so it still had literal `?v=__ASSET_HASH__`; (2) `appsettings.json.gz` was the baked-in default, shadowing the `appsettings.json` mounted per-environment via docker-compose. Fix: `gzip_static on;` only inside `location /_framework/ | /_content/ | /css/` (content there is immutable and never rewritten/mounted); the root `location /` uses on-the-fly `gzip` so it always serves the current `index.html`/`appsettings.json`. **This means no precompressed appsettings need to be prepared in `deployments/`.**
- **`fingerprint-assets.sh` also deletes root-level `*.gz`/`*.br`** (`-maxdepth 1`) as cleanup — those siblings are stale (rewritten index.html) or shadowed (mounted appsettings); subdir precompressed assets are kept. Belt-and-suspenders on top of the location scoping.

### Brotli (ngx_brotli in the custom image)

- **Added `ngx_brotli` to the custom nginx image and reused that image for the app nginx too** — the precompressed `*.br` from `dotnet publish` (framework wasm/js, css, some `_content`) live in the **app** container, but its nginx was stock `nginx:1.29-alpine` with no brotli module. Rather than maintain two custom images, the web `Dockerfile` final stage now `FROM karql/kom-tracker-nginx:1.31-alpine` (headers-more + brotli). One module build, two consumers (app nginx + reverse proxy); module ABI also now matches (both 1.31, built `--with-compat`). **Build order:** publish the custom nginx image before building the web image.
- **`ngx_brotli` built with its bundled brotli submodule (cmake), statically linked** — the current `ngx_brotli` does NOT link the system libbrotli (`./configure` errors: "Brotli library is missing from deps/brotli/c"; system-libs was a wrong first attempt). Recipe: `git clone --recurse-submodules`, build `deps/brotli/out` with `cmake -DBUILD_SHARED_LIBS=OFF -DCMAKE_POSITION_INDEPENDENT_CODE=ON` (PIC needed: static `.a` linked into the dynamic `.so`), then `--add-dynamic-module`. Static link ⇒ no runtime `brotli-libs`. Build deps: `cmake` (+ `git` from `alpine-sdk`). Verified: image builds and `nginx -t` passes with headers-more + both brotli modules loaded.
- **app nginx: `brotli_static on` + `gzip_static on` in the immutable asset locations** — serves the precompressed `*.br` (best ratio on wasm, zero CPU), falling back to `*.gz`, then on-the-fly. `brotli on`/`gzip on` at http handle the on-the-fly cases (root index.html, `_content` assets without a precompressed sibling).
- **reverse proxy: `brotli on` for proxied API JSON** (filter module only — it serves no static). Deliberately **no `brotli_proxied`** directive — its existence in ngx_brotli is uncertain and an unknown directive would fail `nginx -t`; gzip (`gzip_proxied any`) already covers API JSON as the guaranteed fallback. Static responses from the app nginx already carry `Content-Encoding` and are passed through untouched.

## Done

- [x] `deployments/prod-reverse-proxy/conf/nginx/conf.d/kom-tracker-prod.conf`: `listen 443 ssl http2;`.
- [x] `src/KomTracker/KomTracker.WEB/Docker/nginx/nginx.conf`: `gzip on; gzip_static on; gzip_proxied any; gzip_vary on; gzip_comp_level 6; gzip_min_length 1024; gzip_types text/css application/javascript application/json image/svg+xml;`.
- [x] Removed Plotly: `KomTracker.WEB.csproj` package ref, two `<script>` tags in `index.html`, `@using` lines in `_Imports.razor`, commented dead code in `Dashboard.razor`; reworded the Plotly example in the app nginx comment.
- [x] Docs: `.ai/README.md` (drop Plotly), `.ai/plans/2026-07-09-web-asset-fingerprinting.md` (note Plotly removed), `CHANGELOG.md` `### Performance`.
- [x] `dotnet build` + `dotnet test` green.

### Follow-up (done)
- [x] Local reverse-proxy conf: `deployments/local/conf/nginx/conf.d/kom-tracker-local.conf` → `listen 9999 ssl http2;`.
- [x] Reverse-proxy gzip for proxied/API responses: `deployments/{prod-reverse-proxy,local}/conf/nginx/nginx.conf`.
- [x] New image `docker-images/nginx/1.31-alpine/Dockerfile` (VERSION 1.31-alpine, headers-more 0.37).
- [x] Repoint compose to `1.31-alpine`: `deployments/local/docker-compose.yml`, `deployments/prod-reverse-proxy/docker-compose.yml`.
- [x] CHANGELOG: API gzip + nginx 1.21→1.31 lines.
- [x] Bug fix: scope `gzip_static on` to `/_framework/ /_content/ /css/` (app nginx) + delete root `*.gz`/`*.br` in `fingerprint-assets.sh`; verified via clean publish + script (root siblings gone, `_framework`/`css` kept, index.html fingerprinted, no `__ASSET_HASH__`/Plotly leftovers).
- [x] Brotli: `ngx_brotli` in `docker-images/nginx/1.31-alpine/Dockerfile` (system libbrotli); web `Dockerfile` app nginx now `FROM` the custom image; `brotli_static on`+`brotli on` in app nginx.conf; `brotli on` (filter) in both reverse-proxy nginx.conf. Verified publish emits `*.br` for `_framework` (70), `css`, some `_content` (8).

## Verify (post-deploy)
- `curl -sI --http2 https://komtracker.karkula.pl/app/` → `HTTP/2 200`.
- `curl -sI -H 'Accept-Encoding: gzip' …/_framework/dotnet.native.<hash>.wasm` → `content-encoding: gzip`.
- `curl -sI -H 'Accept-Encoding: gzip' '…/_content/MudBlazor/MudBlazor.min.css?v=<hash>'` → `content-encoding: gzip`.
- No `_content/Plotly.Blazor/*` requests; total transfer ≈ 5–6 MiB (was 21). Re-run Lighthouse.

## Deployment
- App web image (`karql/kom-tracker-web`): gzip + Plotly removal → rebuild + version bump, update tag in `deployments/prod/docker-compose.yml`, recreate.
- Reverse proxy: edit mounted conf on server + `nginx -s reload`. No rebuild.

#!/bin/sh
# Replace `?v=__ASSET_HASH__` placeholders in index.html with a short content hash of each
# referenced file. Used for assets the .NET SDK does not fingerprint (app.css, RCL _content/*),
# so they bust the cache on change and can be served immutable. Run at Docker image build over
# the published wwwroot. The placeholder is harmless if this never runs (query is ignored),
# so local `dotnet run` still loads the assets.
set -eu

WWWROOT="${1:?usage: fingerprint-assets.sh <wwwroot-dir>}"
INDEX="$WWWROOT/index.html"

[ -f "$INDEX" ] || { echo "fingerprint-assets: index.html not found at $INDEX" >&2; exit 1; }

grep -oE '[^"]+\?v=__ASSET_HASH__' "$INDEX" \
    | sed 's/?v=__ASSET_HASH__$//' \
    | sort -u \
    | while IFS= read -r path; do
        file="$WWWROOT/$path"
        [ -f "$file" ] || { echo "fingerprint-assets: MISSING referenced file $file" >&2; exit 1; }

        hash=$(sha256sum "$file" | cut -c1-12)
        # escape regex metachars in the path for the sed match side
        esc=$(printf '%s' "$path" | sed 's/[.[*]/\\&/g')
        sed -i "s|${esc}?v=__ASSET_HASH__|${path}?v=${hash}|g" "$INDEX"
        echo "fingerprint-assets: $path -> ?v=$hash"
    done

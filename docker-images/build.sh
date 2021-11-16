#!/usr/bin/env bash

# http://redsymbol.net/articles/unofficial-bash-strict-mode/
set -euo pipefail

IMAGE=${1?IMAGE parameter is required}
VERSION=${2?VERSION parameter is required}


# Use paths relative to script dir
SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"

TAG="karql/kom-tracker-${IMAGE}:${VERSION}"

echo "Build: ${TAG}"
docker build \
    --label="BUILD_GIT_SHA=$(git rev-parse HEAD)" \
    --label="BUILD_DATE=$(date -u +'%Y-%m-%dT%H:%M:%SZ')" \
    -t $TAG \
    -f "${SCRIPT_DIR}/${IMAGE}/${VERSION}/Dockerfile" \
     "${SCRIPT_DIR}/${IMAGE}/${VERSION}"

echo "Publish: ${TAG}"
docker push $TAG
#!/usr/bin/env bash

# http://redsymbol.net/articles/unofficial-bash-strict-mode/
set -euo pipefail


DOCKER_FILE=${1?DOCKER_FILE parameter is required - path relative to src folder}
NAME=${2?NAME prameter is required}
VERSION=${3?VERSION parameter is required}

# Use paths relative to script dir
SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"

TAG="karql/kom-tracker/${NAME}:${VERSION}"

echo "Build: ${TAG}"
docker build \
    --label="BUILD_GIT_SHA=$(git rev-parse HEAD)" \
    --label="BUILD_DATE=$(date -u +'%Y-%m-%dT%H:%M:%SZ')" \
    -t $TAG \
    -f "${SCRIPT_DIR}/../src/${DOCKER_FILE}" \
    "${SCRIPT_DIR}/../src"

#echo "Publish: ${TAG}"
#docker push $TAG
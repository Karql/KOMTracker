#!/usr/bin/env bash

# http://redsymbol.net/articles/unofficial-bash-strict-mode/
set -euo pipefail

DOCKER_FILE="KomTracker/KomTracker.WEB/Dockerfile"
NAME="kom-tracker-web"
VERSION=${1?VERSION parameter is required}

# Use paths relative to script dir
SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
"${SCRIPT_DIR}/docker-build.sh" $DOCKER_FILE $NAME $VERSION
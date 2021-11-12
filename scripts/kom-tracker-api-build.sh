#!/usr/bin/env bash

# http://redsymbol.net/articles/unofficial-bash-strict-mode/
set -euo pipefail

DOCKER_FILE="Services/KOMTracker.API/Dockerfile"
NAME="kom-tracker-api"
VERSION=${1?VERSION parameter is required}

# Use paths relative to script dir
SCRIPT_DIR="$(dirname "$(readlink -f "$0")")"
"${SCRIPT_DIR}/api-build.sh" $DOCKER_FILE $NAME $VERSION
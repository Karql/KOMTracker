#!/usr/bin/env bash

# http://redsymbol.net/articles/unofficial-bash-strict-mode/
set -euo pipefail

scp -r ./. opc@oraclecloud1:~/env/kom-tracker-prod/

ssh opc@oraclecloud1 'docker-compose --env-file ~/env/kom-tracker-prod/.env -f ~/env/kom-tracker-prod/docker-compose.yml -p kom-tracker-prod pull && \
docker-compose --env-file ~/env/kom-tracker-prod/.env -f ~/env/kom-tracker-prod/docker-compose.yml -p kom-tracker-prod up -d --remove-orphans'


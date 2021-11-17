#!/usr/bin/env bash

# http://redsymbol.net/articles/unofficial-bash-strict-mode/
set -euo pipefail

scp -r ./. opc@oraclecloud1:~/env/reverse-proxy/

ssh opc@oraclecloud1 'docker-compose -f ~/env/reverse-proxy/docker-compose.yml -p reverse-proxy pull && \
docker-compose -f ~/env/reverse-proxy/docker-compose.yml -p reverse-proxy up -d --remove-orphans'
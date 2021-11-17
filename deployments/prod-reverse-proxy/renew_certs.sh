#!/usr/bin/env bash

# http://redsymbol.net/articles/unofficial-bash-strict-mode/
set -euo pipefail

echo "### Run certbot renew ..."
docker-compose -p reverse-proxy run --rm --entrypoint "\
  certbot renew" certbot
echo

echo "### Reloading nginx ..."
docker-compose -p reverse-proxy exec nginx-reverse-proxy nginx -s reload
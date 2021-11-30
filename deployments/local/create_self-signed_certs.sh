#!/usr/bin/env bash

# http://redsymbol.net/articles/unofficial-bash-strict-mode/
set -euo pipefail

# Based on ./get_certs.sh
# only create self-signed

if ! [ -x "$(command -v docker-compose)" ]; then
  echo 'Error: docker-compose is not installed.' >&2
  exit 1
fi

domains=(
  localhost
)
rsa_key_size=4096
data_path="./volumes/reverse-proxy/certbot"

if [ ! -e "$data_path/conf/options-ssl-nginx.conf" ] || [ ! -e "$data_path/conf/ssl-dhparams.pem" ]; then
  echo "### Downloading recommended TLS parameters ..."
  mkdir -p "$data_path/conf"
  # http_proxy block this
  # curl -s https://raw.githubusercontent.com/certbot/certbot/master/certbot-nginx/certbot_nginx/_internal/tls_configs/options-ssl-nginx.conf > "$data_path/conf/options-ssl-nginx.conf"
  # curl -s https://raw.githubusercontent.com/certbot/certbot/master/certbot/certbot/ssl-dhparams.pem > "$data_path/conf/ssl-dhparams.pem"

  cp conf/certbot/options-ssl-nginx.conf "$data_path/conf/options-ssl-nginx.conf"
  cp conf/certbot/ssl-dhparams.pem "$data_path/conf/ssl-dhparams.pem"
  echo
fi

for domain in "${domains[@]}"; do
  echo "### Creating self-signed certificate for $domain ..."

  path="$data_path/conf/live/$domain"

  if [ -d "$path" ]; then
    read -p "Existing data found. Continue and replace existing certificate? (y/N) " decision
    if [ "$decision" != "Y" ] && [ "$decision" != "y" ]; then      
      echo "skiping..."
      continue
    fi
  fi

  mkdir -p $path
  # cert with CA
  # https://gist.github.com/fntlnz/cf14feb5a46b2eda428e000157447309
  # Add SAN for avoid untrusted waring
  # https://stackoverflow.com/a/41366949/11391667
  # https://gist.github.com/fntlnz/cf14feb5a46b2eda428e000157447309#gistcomment-3292665 
  openssl req -new -nodes -sha256 -newkey rsa:$rsa_key_size -out "$path/request.csr" -keyout "$path/privkey.pem" -subj "/CN=$domain" -sha256 -extensions san -config \
  <(echo "[req]"; 
    echo distinguished_name=req; 
    echo "[san]"; 
    echo subjectAltName=DNS:$domain
    ) 

  # signing 
  openssl x509 -req -in "$path/request.csr" -CA ./rootca/KomTrackerRootCA.crt -CAkey ./rootca/KomTrackerRootCA.key -CAcreateserial -out "$path/fullchain.pem" -days 3650 -extfile <(printf "subjectAltName=DNS:$domain")      
done

echo "### Starting nginx ..."
docker-compose -p komtracker-local up --force-recreate -d nginx-reverse-proxy
#docker-compose -p komtracker-local exec nginx-reverse-proxy nginx -s reload
echo
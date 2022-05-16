#!/usr/bin/env bash

# http://redsymbol.net/articles/unofficial-bash-strict-mode/
set -euo pipefail

# Based on https://github.com/wmnnd/nginx-certbot
# This modification change approach a bit
# org:  download single certificate for all domains
# this: download separate certificate for each domain

if ! [ -x "$(command -v docker-compose)" ]; then
  echo 'Error: docker-compose is not installed.' >&2
  exit 1
fi

domains=(komtracker.karkula.pl)
rsa_key_size=4096
data_path="/u01/volumes/reverse-proxy/certbot"
email="karql.pl@gmail.com" # Adding a valid address is strongly recommended
staging=0 # Set to 1 if you're testing your setup to avoid hitting request limits

if [ -d "$data_path" ]; then
  read -p "Existing data found. Continue and replace existing certificate? (y/N) " decision
  if [ "$decision" != "Y" ] && [ "$decision" != "y" ]; then
    exit
  fi
fi

if [ ! -e "$data_path/conf/options-ssl-nginx.conf" ] || [ ! -e "$data_path/conf/ssl-dhparams.pem" ]; then
  echo "### Downloading recommended TLS parameters ..."
  mkdir -p "$data_path/conf"
  # curl -s https://raw.githubusercontent.com/certbot/certbot/master/certbot-nginx/certbot_nginx/_internal/tls_configs/options-ssl-nginx.conf > "$data_path/conf/options-ssl-nginx.conf"
  # curl -s https://raw.githubusercontent.com/certbot/certbot/master/certbot/certbot/ssl-dhparams.pem > "$data_path/conf/ssl-dhparams.pem"

  cp conf/certbot/options-ssl-nginx.conf "$data_path/conf/options-ssl-nginx.conf"
  cp conf/certbot/ssl-dhparams.pem "$data_path/conf/ssl-dhparams.pem"
  echo
fi

for domain in "${domains[@]}"; do
  echo "### Creating dummy certificate for $domain ..."
  path="/etc/letsencrypt/live/$domain"
  mkdir -p "$data_path/conf/live/$domain"
  docker-compose -p reverse-proxy run --rm --entrypoint "\
  openssl req -x509 -nodes -newkey rsa:$rsa_key_size -days 1\
    -keyout '$path/privkey.pem' \
    -out '$path/fullchain.pem' \
    -subj '/CN=localhost'" certbot
  echo
done

echo "### Starting nginx ..."
docker-compose -p reverse-proxy up --force-recreate -d nginx-reverse-proxy
echo

# Select appropriate email arg
case "$email" in
  "") email_arg="--register-unsafely-without-email" ;;
  *) email_arg="--email $email" ;;
esac

# Enable staging mode if needed
staging_arg=""
if [ $staging != "0" ]; then staging_arg="--staging"; fi

for domain in "${domains[@]}"; do
  echo "### Deleting dummy certificate for $domain ..."
  docker-compose -p reverse-proxy run --rm --entrypoint "\
  rm -Rf /etc/letsencrypt/live/$domain && \
  rm -Rf /etc/letsencrypt/archive/$domain && \
  rm -Rf /etc/letsencrypt/renewal/$domain.conf" certbot
  echo

  echo "### Requesting Let's Encrypt certificate for $domain ..."

  docker-compose -p reverse-proxy run --rm --entrypoint "\
    certbot certonly --webroot -w /var/www/certbot \
      $staging_arg \
      $email_arg \
      -d $domain \
      --rsa-key-size $rsa_key_size \
      --agree-tos \
      --force-renewal" certbot
  echo

done

echo "### Reloading nginx ..."
docker-compose -p reverse-proxy exec nginx-reverse-proxy nginx -s reload
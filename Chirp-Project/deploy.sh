#!/usr/bin/env bash
# shellcheck source=/dev/null
source ~/.bash_profile
cd ../vagrant || exit
docker compose pull
docker compose up -d
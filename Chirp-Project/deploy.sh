#!/usr/bin/env bash
set -euo pipefail

# shellcheck source=/dev/null
if [ -f "$HOME/.bash_profile" ]; then
	source "$HOME/.bash_profile"
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR"

if [ ! -f "$PROJECT_DIR/docker-compose.yml" ] && [ -f "/vagrant/docker-compose.yml" ]; then
	PROJECT_DIR="/vagrant"
fi

if [ ! -f "$PROJECT_DIR/docker-compose.yml" ]; then
	echo "Could not find docker-compose.yml in $PROJECT_DIR or /vagrant" >&2
	exit 1
fi

cd "$PROJECT_DIR"

# Make Grafana storage resilient to re-deploys and accidental compose cleanup.
docker volume create minitwit-grafana-storage >/dev/null

docker compose -f docker-stack.yml pull
docker stack deploy -c docker-stack.yml $MINITWIT_STACK_NAME
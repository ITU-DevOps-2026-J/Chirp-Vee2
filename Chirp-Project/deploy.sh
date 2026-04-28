#!/usr/bin/env bash
set -euo pipefail

# shellcheck source=/dev/null
if [ -f "$HOME/.bash_profile" ]; then
	source "$HOME/.bash_profile"
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR"

if [ ! -f "$PROJECT_DIR/docker-stack.yml" ] && [ -f "/vagrant/docker-stack.yml" ]; then
	PROJECT_DIR="/vagrant"
fi

if [ ! -f "$PROJECT_DIR/docker-stack.yml" ]; then
	echo "Could not find docker-stack.yml in $PROJECT_DIR or /vagrant" >&2
	exit 1
fi

cd "$PROJECT_DIR"

# Make Grafana storage resilient to re-deploys and accidental compose cleanup.
docker volume create minitwit-grafana-storage >/dev/null

# Default stack name if not provided in environment
: ${MINITWIT_STACK_NAME:=minitwit}

# Deploy the services using Docker Stack (Swarm). Use --with-registry-auth
# when pulling private images from registries that require credentials.
docker stack deploy --with-registry-auth -c docker-stack.yml "$MINITWIT_STACK_NAME"
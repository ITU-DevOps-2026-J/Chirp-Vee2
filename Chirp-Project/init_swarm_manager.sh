#!/bin/bash
set -e

if [[ -z "$MANAGER_IP" ]]; then
    echo "Error: Manager IP address not provided"
    exit 1
fi

echo "========================================="
echo "Initializing Docker Swarm Manager..."
echo "========================================="

# Initialize Docker Swarm
docker swarm init --advertise-addr=$MANAGER_IP

# Display swarm status
docker node ls
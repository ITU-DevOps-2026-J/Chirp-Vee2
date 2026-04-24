#!/bin/bash
set -e

echo "========================================="
echo "Joining Docker Swarm as Worker..."
echo "========================================="

MANAGER_IP=$1
MANAGER_TOKEN=$2

echo "Joining the swarm with manager IP: $MANAGER_IP and token: $MANAGER_TOKEN"

docker swarm join --token $MANAGER_TOKEN $MANAGER_IP:2377